using Blazored.LocalStorage;
using Shared.Common;
using Shared.DTOs;
using Shared.Entities;
using System.Net.Http.Json;
using Web.Common;

namespace Web.Services
{
    public class AccountService : IAccountService
    {
        private readonly HttpClient _httpClient;
        private readonly IAuthValidator _authValidator;
        private readonly ILocalStorageService _localStorage;
        private readonly AccountState _accountState;

        public AccountService(HttpClient httpClient, IAuthValidator authValidator, ILocalStorageService localStorage, AccountState accountState)
        {
            _httpClient = httpClient;
            _authValidator = authValidator;
            _localStorage = localStorage;
            _accountState = accountState;
        }

        public async Task<string> Create(AuthDto dto)
        {
            var res = await _httpClient.PutAsJsonAsync("/Auth/Create", dto);
            res.EnsureSuccessStatusCode();

            var token = await res.Content.ReadAsStringAsync();
            // We store the newly generated token in localStorage for future use
            await _authValidator.PrepareAndValidate(token);
            return token;
        }

        public async Task<User> Edit(string id, UserUpdateDto dto)
        {
            await _authValidator.EnsureIsAuthenticated();

            var res = await _httpClient.PostAsJsonAsync($"/Users/{id}", dto);
            res.EnsureSuccessStatusCode();

            var user = await res.Content.ReadFromJsonAsync<User>();
            return user is null ? throw new HttpRequestException("User object response returned a null object") : user;
        }

        public async Task<bool> Delete(string id)
        {
            await _authValidator.EnsureIsAuthenticated();

            var res = await _httpClient.DeleteAsync($"/Links/{id}");
            res.EnsureSuccessStatusCode();

            return res.IsSuccessStatusCode;
        }

        public async Task<string> Login(AuthDto dto)
        {
            var res = await _httpClient.PutAsJsonAsync("/Auth/Login", dto);
            res.EnsureSuccessStatusCode();

            var token = await res.Content.ReadAsStringAsync();
            // We store the newly generated token in localStorage for future use
            await _authValidator.PrepareAndValidate(token);
            return token;
        }

        // TODO: Remove duplicates
        public async Task<string> ResetPassword(PasswordDto dto)
        {
            var res = await _httpClient.PutAsJsonAsync("/Auth/ResetPassword", dto);
            res.EnsureSuccessStatusCode();

            var token = await res.Content.ReadAsStringAsync();
            // We store the newly generated token in localStorage for future use
            await _authValidator.PrepareAndValidate(token);
            return token;
        }

        public async Task<User> GetUser(string id)
        {
            await _authValidator.EnsureIsAuthenticated();

            var res = await _httpClient.GetFromJsonAsync<User>($"/Users/{id}");
            return res is null ? throw new HttpRequestException("Failed to fetch current User: UserId: {id}") : res;
        }

        public async Task Logout()
        {
            await _authValidator.EnsureIsAuthenticated();
            await _localStorage.RemoveItemAsync("token");
            _authValidator.ClearToken();
            _accountState.Property = null;
        }

        public async Task<UserClaimResponse> GenerateTempAccount()
        {
            var authDto = new AuthDto
            {
                Username = UsernameGenerator.GenerateTemporaryUsername(),
                Password = GuidUtils.GenerateGuid()
            };

            var token = await Create(authDto);
            if (token != null)
            {
                await _authValidator.PrepareAndValidate(token);
                return await _authValidator.GetUserClaim();
            }
            else
                throw new InvalidOperationException("User creation error, token is null");
        }
    }
}
