using Shared.DTOs;

namespace Web.Common
{
    public class AppSettings
    {
        public List<ApiHostDto> ApiHosts { get; set; } = new();
    }
}
