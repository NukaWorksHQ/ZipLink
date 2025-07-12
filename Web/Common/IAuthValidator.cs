using Shared.Common;

namespace Web.Common
{
    public interface IAuthValidator
    {
        public Task PrepareAndValidate(string token = "");

        public bool IsAuthenticated();

        public Task EnsureIsAuthenticated();

        public Task<UserClaimResponse> GetUserClaim();
    }
}
