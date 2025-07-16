using Shared.Common;
using Shared.DTOs;
using Shared.Entities;

namespace Web.Common
{
    public interface IAccountService
    {
        public Task<string> Create(AuthDto dto);

        public Task<string> Login(AuthDto dto);
        public Task<string> ResetPassword(PasswordDto dto);

        public Task<User> Edit(string id, UserUpdateDto dto);

        public Task<User> GetUser(string id);

        public Task<bool> Delete(string id);

        public Task<UserClaimResponse> GenerateTempAccount();

        public Task Logout();
    }
}
