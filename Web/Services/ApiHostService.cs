using Shared.DTOs;

namespace Web.Services
{
    public interface IApiHostService
    {
        List<ApiHostDto> GetApiHosts();
        string GetApiHostUrl(string hostName);
    }

    public class ApiHostService(IConfiguration config) : IApiHostService
    {
        public List<ApiHostDto> GetApiHosts()
        {
            var apiHosts = config.GetSection("ApiHosts").Get<List<ApiHostDto>>() ?? new List<ApiHostDto>();
            return apiHosts;
        }

        public string GetApiHostUrl(string hostName)
        {
            var apiHosts = GetApiHosts();
            var selectedHost = apiHosts.FirstOrDefault(h => h.Name == hostName);
            return selectedHost?.Url ?? throw new ArgumentException($"API Host '{hostName}' not found.");
        }
    }
}
