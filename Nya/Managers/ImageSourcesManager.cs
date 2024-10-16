using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IPA.Loader;
using IPA.Utilities;
using Newtonsoft.Json;
using Nya.Configuration;
using Nya.Entries;
using SiraUtil.Logging;
using SiraUtil.Web;
using SiraUtil.Zenject;

namespace Nya.Managers
{
    internal sealed class ImageSourcesManager
    {
        private readonly SiraLog _siraLog;
        private readonly IHttpService _httpService;
        private readonly PluginConfig _pluginConfig;
        private readonly PluginMetadata _pluginMetadata;

        public ImageSourcesManager(SiraLog siraLog, IHttpService httpService, PluginConfig pluginConfig, UBinder<Plugin, PluginMetadata> pluginMetadata)
        {
            _siraLog = siraLog;
            _httpService = httpService;
            _pluginConfig = pluginConfig;
            _pluginMetadata = pluginMetadata.Value;
        }
        
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        
        private Dictionary<string, ImageSourceEntry>? _sources;
        private bool _sourceFetchSuccesful;

        private async Task<Dictionary<string, ImageSourceEntry>> PopulateSources()
        {
            var remoteSources = await FetchRemoteSources();
            var customSources = FetchCustomSources();
            var localSources = FetchLocalSources();

            Dictionary<string, ImageSourceEntry> sources = new Dictionary<string, ImageSourceEntry>();
            foreach (var item in remoteSources)
            {
                // Move this over to a method if more checks are needed
                if (item.Value.ResponseType == ImageSourceEntry.ResponseTypeEnum.URL && item.Value.UrlResponseEntry is null)
                {
                    _siraLog.Error($"{item.Key} has a URL response type but no URL response entry, skipping");
                    continue;
                }
                
                sources[item.Key] = item.Value;
            }
            foreach (var item in customSources)
            {
                if (item.Value.ResponseType == ImageSourceEntry.ResponseTypeEnum.URL && item.Value.UrlResponseEntry is null)
                {
                    _siraLog.Error($"{item.Key} has a URL response type but no URL response entry, skipping");
                    continue;
                }
                
                sources[item.Key] = item.Value;
            }
            foreach (var item in localSources)
            {
                sources[item.Key] = item.Value;
            }
            
            _siraLog.Info($"Loaded following APIs: [ {string.Join(", ", sources.Keys)} ]");
            
            return sources;
        }

        private async Task<Dictionary<string, ImageSourceEntry>> FetchRemoteSources()
        {
            Dictionary<string, ImageSourceEntry> sources = new Dictionary<string, ImageSourceEntry>();
            _sourceFetchSuccesful = false;
            
            try
            {
                if (new Uri(Plugin.ImageSourcesJsonLink).IsFile)
                {
                    var jsonString = File.ReadAllText(Plugin.ImageSourcesJsonLink);
                    sources = JsonConvert.DeserializeObject<Dictionary<string, ImageSourceEntry>>(jsonString);
                }
                else
                {
                    var response = await _httpService.GetAsync(Plugin.ImageSourcesJsonLink);

                    if (!response.Successful)
                    {
                        _siraLog.Error("Failed to fetch image sources from " + Plugin.ImageSourcesJsonLink);
                        _siraLog.Error(response.Error());
                    }
                    else
                    {
                        var jsonString = await response.ReadAsStringAsync();
                        sources = JsonConvert.DeserializeObject<Dictionary<string, ImageSourceEntry>>(jsonString);
                    }   
                }
            }
            catch (Exception e)
            {
                _siraLog.Error("Failed to fetch image sources from " + Plugin.ImageSourcesJsonLink);
                _siraLog.Error(e);
            }
            
            _sourceFetchSuccesful = true;
            return sources;
        }
        
        private Dictionary<string, ImageSourceEntry> FetchCustomSources()
        {
            try 
            {
                var jsonString = File.ReadAllText(Plugin.CustomImageSourcesPath);
                
                var commentedImageSourcesEntries = JsonConvert.DeserializeObject<CommentedImageSourcesEntries>(jsonString);
                return commentedImageSourcesEntries.Sources;
            }
            catch (Exception e)
            {
                _siraLog.Error($"Failed to read CustomImageSources.json: {e.Message}");
                return new Dictionary<string, ImageSourceEntry>();;
            }
        }
        
        private Dictionary<string, ImageSourceEntry> FetchLocalSources()
        {
            Dictionary<string, ImageSourceEntry> sources = new Dictionary<string, ImageSourceEntry>();

            sources["Local Files"] = new ImageSourceEntry
            {
                Url = Path.Combine(UnityGame.UserDataPath, "Nya"),
                IsLocal = true,
                SfwEndpoints = PopulateLocalEndpoints(false),
                NsfwEndpoints = PopulateLocalEndpoints(true)
            };
            
            return sources;
        }
        
        private static List<string> PopulateLocalEndpoints(bool nsfw)
        {
            var baseFolder = nsfw ? "nsfw" : "sfw";
            var endpoints = new List<string> {baseFolder};

            foreach (var folder in Directory.GetDirectories(Path.Combine(UnityGame.UserDataPath, "Nya", baseFolder)))
            {
                endpoints.Add(Path.GetFileName(folder));
            }

            return endpoints;
        }

        private void FixConfigImageSourcesIssues(Dictionary<string, ImageSourceEntry> sources)
        {
            if (!_sourceFetchSuccesful)
            {
                // Don't want to nuke the config just because the user booted the game without internet or something
                return;
            }
            
            // Stops any changes to the config happening until this method is done
            using var _ = _pluginConfig.ChangeTransaction;
            
            // Check that the config doesn't have any invalid sources saved
            // Can't use a foreach loop here because we're modifying the dictionary, doing so would throw an InvalidOperationException.
            for (int i = _pluginConfig.SelectedEndpoints.Count - 1; i >= 0; i--)
            {
                var source = _pluginConfig.SelectedEndpoints.ElementAt(i);
                if (!sources.ContainsKey(source.Key))
                {
                    _pluginConfig.SelectedEndpoints.Remove(source.Key);
                }
            }
            
            // Checks that the selected source is valid
            if (!sources.ContainsKey(_pluginConfig.SelectedAPI))
            {
                _pluginConfig.SelectedAPI = sources.First().Key;
            }
            
            foreach (var source in sources)
            {
                // Adds a new source to the selected endpoints dictionary
                if (!_pluginConfig.SelectedEndpoints.ContainsKey(source.Key))
                {
                    _pluginConfig.SelectedEndpoints[source.Key] = new EndpointData
                    {
                        SelectedSfwEndpoint = source.Value.SfwEndpoints.FirstOrDefault(),
                        SelectedNsfwEndpoint = source.Value.NsfwEndpoints.FirstOrDefault()
                    };
                }
                
                // Checks the sfw endpoint of the source exists
                var sfwEndpoint = _pluginConfig.SelectedEndpoints[source.Key].SelectedSfwEndpoint;
                if (sfwEndpoint == null || !source.Value.SfwEndpoints.Contains(sfwEndpoint))
                {
                    _pluginConfig.SelectedEndpoints[source.Key].SelectedSfwEndpoint = source.Value.SfwEndpoints.FirstOrDefault();
                }
                
                // Checks the nsfw endpoint of the source exists
                var nsfwEndpoint = _pluginConfig.SelectedEndpoints[source.Key].SelectedNsfwEndpoint;
                if (nsfwEndpoint == null || !source.Value.NsfwEndpoints.Contains(nsfwEndpoint))
                {
                    _pluginConfig.SelectedEndpoints[source.Key].SelectedNsfwEndpoint = source.Value.NsfwEndpoints.FirstOrDefault();
                }
            }
            
            _pluginConfig.Changed();
        }
        
        public async Task<Dictionary<string, ImageSourceEntry>> GetSourcesDictionary()
        {
            await _semaphore.WaitAsync();
            try
            {
                if (_sources != null)
                    return _sources;

                _sources = await PopulateSources();
                FixConfigImageSourcesIssues(_sources);

                return _sources;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}