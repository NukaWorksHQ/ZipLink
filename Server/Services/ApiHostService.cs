using Shared.DTOs;

namespace Server.Services
{
    public interface IApiHostService
    {
        List<ApiHostDto> GetApiHosts();
        string GetApiHostUrl(string hostName);
        bool IsValidApiHost(string hostName);
    }

    public class ApiHostService : IApiHostService
    {
        private readonly IConfiguration _config;

        public ApiHostService(IConfiguration config)
        {
            _config = config;
        }

        public List<ApiHostDto> GetApiHosts()
        {
            var apiHosts = _config.GetSection("ApiHosts").Get<List<ApiHostDto>>() ?? new List<ApiHostDto>();
            return apiHosts;
        }

        public string GetApiHostUrl(string hostName)
        {
            var apiHosts = GetApiHosts();
            var selectedHost = apiHosts.FirstOrDefault(h => h.Name == hostName);
            return selectedHost?.Url ?? throw new ArgumentException($"API Host '{hostName}' not found.");
        }

        public bool IsValidApiHost(string hostName)
        {
            var apiHosts = GetApiHosts();
            return apiHosts.Any(h => h.Name == hostName);
        }
    }
}