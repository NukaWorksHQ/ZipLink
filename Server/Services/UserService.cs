using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Server.Common;
using Server.Contexts;
using Shared.DTOs;
using Shared.Entities;

namespace Server.Services
{
    public class UserService : GenericService<AppDbContext, User, UserCreateDto, UserUpdateDto>
    {
        public UserService(AppDbContext context, IMapper mapper) : base(context, mapper)
        {
        }
    }
}
