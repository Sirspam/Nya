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
        private bool _sourceFetchSuccessful;

        private async Task<Dictionary<string, ImageSourceEntry>> PopulateSources()
        {
            Dictionary<string, ImageSourceEntry> sources = new Dictionary<string, ImageSourceEntry>();

            _sourceFetchSuccessful = false;
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

                // Existing sources added here, keep them as they are.

                // Add fluxpoint.dev as a new source
                AddFluxpointDevSource(sources);

                _sourceFetchSuccessful = true;
            }

                // Add fluxpoint.dev as a new source
                sources["fluxpoint.dev"] = new ImageSourceEntry
                {
                    DisplayName = "Fluxpoint",
                    SfwEndpoints = new List<string> { "https://gallery.fluxpoint.dev/api/sfw/img/anime" },
                    NsfwEndpoints = new List<string> { "https://gallery.fluxpoint.dev/api/nsfw/img" } // Example NSFW endpoint
                };

                _sourceFetchSuccesful = true;
        }
            catch (Exception e)
            {
                _siraLog.Error("Failed to fetch image sources from " + Plugin.ImageSourcesJsonLink);
                _siraLog.Error(e);
            }

            sources["Local Files"] = new ImageSourceEntry
            {
                Url = Path.Combine(UnityGame.UserDataPath, "Nya"),
                IsLocal = true,
                SfwEndpoints = PopulateLocalEndpoints(false),
                NsfwEndpoints = PopulateLocalEndpoints(true)
            };

            return sources;
        }

        private void AddFluxpointDevSource(Dictionary<string, ImageSourceEntry> sources)
        {
            string apiKey = ConfigManager.GetFluxpointApiKey();
            if (string.IsNullOrEmpty(apiKey))
            {
                _siraLog.Error("API key for fluxpoint.dev is missing.");
                return;
            }

            // Use EndpointData to get SFW and NSFW endpoints
            var sfwEndpoints = EndpointData.FluxpointSfwEndpoints.Values.ToList();
            var nsfwEndpoints = EndpointData.FluxpointNsfwEndpoints.Values.ToList();

            sources["fluxpoint.dev"] = new ImageSourceEntry
            {
                DisplayName = "Fluxpoint",
                SfwEndpoints = sfwEndpoints.Select(endpoint => $"https://gallery.fluxpoint.dev/api/{endpoint}").ToList(),
                NsfwEndpoints = nsfwEndpoints.Select(endpoint => $"https://gallery.fluxpoint.dev/api/{endpoint}").ToList()
            };
        }

        private static List<string> PopulateLocalEndpoints(bool nsfw)
        {
            var baseFolder = nsfw ? "nsfw" : "sfw";
            var endpoints = new List<string> { baseFolder };

            foreach (var folder in Directory.GetDirectories(Path.Combine(UnityGame.UserDataPath, "Nya", baseFolder)))
            {
                endpoints.Add(Path.GetFileName(folder));
            }

            return endpoints;
        }

        private void FixConfigImageSourcesIssues(Dictionary<string, ImageSourceEntry> sources)
        {
            if (!_sourceFetchSuccessful)
            {
                // Don't want to nuke the config just because the user booted the game without internet or something
                return;
            }

            // Stops any changes to the config happening until this method is done
            using var _ = _pluginConfig.ChangeTransaction;

            // Check that the config doesn't have any invalid sources saved
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
