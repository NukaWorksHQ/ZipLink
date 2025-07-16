using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public async Task<User> EditPassword(string id, string hashedPassword)
        {
            var entity = await _context.Users.FindAsync(id);
            entity.HashedPassword = hashedPassword;
            
            await _context.SaveChangesAsync();
            return entity;
        }

        public override async Task<User> Get(string id)
        {
            var entity = await _context.Users
                .Include(e => e.Links)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (entity is null)
                throw new KeyNotFoundException($"Entity with ID {id} not found.");

            return entity;
        }
    }
}
