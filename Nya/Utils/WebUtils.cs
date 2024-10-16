using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SiraUtil.Logging;
using SiraUtil.Web;

namespace Nya.Utils
{
    internal sealed class WebUtils
    {
        private readonly SiraLog _siraLog;
        private readonly IHttpService _httpService;

        public WebUtils(SiraLog siraLog, IHttpService httpService)
        {
            _siraLog = siraLog;
            _httpService = httpService;
        }

        internal async Task<IHttpResponse?> GetAsync(string url, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpService.GetAsync(url, cancellationToken: cancellationToken);
                
                return response;
            }
            catch (Exception e)
            {
                if (e is TaskCanceledException)
                {
                    return default;
                }
                
                _siraLog.Critical(e);
                return default;
            }
        }
        
        internal async Task<T?> GetAsync<T>(string url, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpService.GetAsync(url, cancellationToken: cancellationToken);

                var parsed = await ParseWebResponse<T>(response);
                return parsed;
            }
            catch (Exception e)
            {
                if (e is TaskCanceledException)
                {
                    return default;
                }
                
                _siraLog.Critical(e);
                return default;
            }
        }

        private async Task<T?> ParseWebResponse<T>(IHttpResponse webResponse)
        {
            if (!webResponse.Successful)
            {
                _siraLog.Error($"Unsuccessful web response for parsing {typeof(T)}. Status code: {webResponse.Code}");
                return default;
            }

            try
            {
                using var streamReader = new StreamReader(await webResponse.ReadAsStreamAsync());
                using var jsonTextReader = new JsonTextReader(streamReader);
                var jsonSerializer = new JsonSerializer();
                return jsonSerializer.Deserialize<T>(jsonTextReader);
            }
            catch (Exception e)
            {
                _siraLog.Critical(e);
                return default;
            }
        }
    }
}