using Shared.Entities;
using Web.Services;

namespace Web.Common
{
    public class Utils
    {
        public static string GetFinalLink(Link link, IConfiguration config, string? apiHostName = null)
        {
            if (link is null)
                throw new ArgumentNullException(nameof(link));
            ApiHostService apiHostService;
            if (string.IsNullOrEmpty(apiHostName))
            {
                apiHostService = new ApiHostService(config);
                var apiHosts = apiHostService.GetApiHosts();
                if (!apiHosts.Any())
                    throw new InvalidOperationException("No API hosts configured.");
                
                apiHostName = apiHosts.First().Name;
            }

            apiHostService = new ApiHostService(config);
            var hostUrl = apiHostService.GetApiHostUrl(apiHostName);
            var endpoint = hostUrl + $"/{link.Id}";
            return endpoint;
        }
    }
}
