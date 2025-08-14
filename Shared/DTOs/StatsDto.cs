namespace Shared.DTOs
{
    public class PublicStatsDto
    {
        public int TotalUsers { get; set; }
        public int TotalLinks { get; set; }
        public int LinksToday { get; set; }
        public int LinksThisWeek { get; set; }
        public int LinksThisMonth { get; set; }
        public List<ApiHostStatsDto> TopApiHosts { get; set; } = new();
        public DateTime LastUpdated { get; set; }
    }

    public class ApiHostStatsDto
    {
        public string HostName { get; set; } = string.Empty;
        public int LinkCount { get; set; }
    }
}
