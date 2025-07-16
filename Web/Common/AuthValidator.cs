using Blazored.LocalStorage;
using Shared.Common;
using System.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Web.Common
{
    public class AuthValidator : IAuthValidator
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        public required string Token { get; set; }
        
        public AuthValidator(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }

        public async Task PrepareAndValidate(string token = "")
        {
            if (string.IsNullOrEmpty(token))
            {
                var _token = await _localStorage.GetItemAsStringAsync("token");
                Token = _token is null ? throw new InvalidOperationException("Token is null, please authentificate first") : _token;
            }
            else
            {
                await _localStorage.SetItemAsStringAsync("token", token);
                Token = token;
            }

            // We quickly set the user JWT token into the headers of our HttpClient service instance here
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
        }

        public bool IsAuthenticated()
        {
            return !string.IsNullOrEmpty(Token);
        }

        public async Task<UserClaimResponse> GetUserClaim()
        {
            await EnsureIsAuthenticated();
            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(Token);

            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId");
            var role = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);

            if (userId is null || role is null)
                throw new InvalidOperationException("UserId or Role is null or invalid");

            return new()
            {
                UserId = userId.Value,
                Role = Enum.Parse<UserRole>(role.Value.ToString())
            };
        }

        public async Task EnsureIsAuthenticated()
        {
            await PrepareAndValidate();
            
            if (!IsAuthenticated())
                throw new InvalidOperationException("Token is null, please authentificate first");
        }

        public void ClearToken()
        {
            Token = "";
        }
    }
}
