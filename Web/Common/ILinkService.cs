using Shared.DTOs;
using Shared.Entities;

namespace Web.Common
{
    public interface ILinkService
    {
        public Task<Link> Create(LinkCreateDto dto);

        public Task<Link> Edit(string id, LinkUpdateDto dto);

        public Task<IEnumerable<Link>> GetAll();

        public Task<Link> Get(string id);

        public Task<bool> Delete(string id);

        public Task<LinkStatsDto> GetLinkStats(string id);

        public Task RecordAccess(string id, string ipAddress, string? userAgent, string? referer);
    }
}
