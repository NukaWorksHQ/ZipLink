using AutoMapper;
using Server.Common;
using Server.Contexts;
using Server.DTOs;
using Server.Entities;

namespace Server.Services
{
    public class LinkService : GenericService<AppDbContext, Link, LinkCreateDto, LinkUpdateDto>
    {
        public LinkService(AppDbContext context, IMapper mapper) : base(context, mapper)
        {
        }
    }
}
