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
            return await GetAsync(url, null, null, cancellationToken);
        }

        internal async Task<IHttpResponse?> GetAsync(string url, string? token, string? queryParameters, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                token = null;
            }
            
            if (!string.IsNullOrWhiteSpace(queryParameters))
            {
                if (queryParameters!.StartsWith("?"))
                {
                    queryParameters = queryParameters.Substring(1);
                }
                
                url += $"?{queryParameters}";
            }
            
            try
            {
                _httpService.Token = token;
                var response = await _httpService.GetAsync(url, cancellationToken: cancellationToken);
                
                _httpService.Token = null;
                return response;
            }
            catch (Exception e)
            {
                _httpService.Token = null;
                
                if (e is TaskCanceledException)
                {
                    return default;
                }
                
                _siraLog.Critical(e);
                return default;
            }
        }

        internal async Task<T?> ParseWebResponse<T>(IHttpResponse webResponse)
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