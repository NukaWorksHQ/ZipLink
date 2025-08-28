using Shared.DTOs;
using Shared.Entities;
using System.Net.Http.Json;
using Web.Common;

namespace Web.Services
{
    public class LinkService : ILinkService
    {
        private readonly HttpClient _httpClient;
        private readonly IAuthValidator _authValidator;

        public LinkService(HttpClient httpClient, IAuthValidator authValidator)
        {
            _httpClient = httpClient;
            _authValidator = authValidator;
        }

        public async Task<Link> Create(LinkCreateDto dto)
        {
            await _authValidator.EnsureIsAuthenticated();

            var res = await _httpClient.PutAsJsonAsync("/Links", dto);
            res.EnsureSuccessStatusCode();

            var link = await res.Content.ReadFromJsonAsync<Link>();
            return link is null ? throw new InvalidOperationException("Link object response returned a null object") : link;
        }

        public async Task<bool> Delete(string id)
        {
            await _authValidator.EnsureIsAuthenticated();

            var res = await _httpClient.DeleteAsync($"/Links/{id}");
            res.EnsureSuccessStatusCode();

            return res.IsSuccessStatusCode;
        }

        public async Task<Link> Edit(string id, LinkUpdateDto dto)
        {
            await _authValidator.EnsureIsAuthenticated();

            var res = await _httpClient.PostAsJsonAsync($"/Links/{id}", dto);
            res.EnsureSuccessStatusCode();
            
            var link = await res.Content.ReadFromJsonAsync<Link>();
            return link is null ? throw new InvalidProgramException("Link object response returned a null object") : link;
        }

        public async Task<Link> Get(string id)
        {
            await _authValidator.EnsureIsAuthenticated();

            var res = await _httpClient.GetFromJsonAsync<Link>($"/Links/{id}");
            return res is null ? throw new InvalidOperationException("Link object response returned a null object") : res;
        }

        public async Task<IEnumerable<Link>> GetAll()
        {
            await _authValidator.EnsureIsAuthenticated();

            var res = await _httpClient.GetFromJsonAsync<IEnumerable<Link>>("/Links");
            return res is null ? throw new InvalidOperationException("Link object response returned a null object") : res;
        }

        public async Task<LinkStatsDto> GetLinkStats(string id)
        {
            await _authValidator.EnsureIsAuthenticated();

            var res = await _httpClient.GetFromJsonAsync<LinkStatsDto>($"/Links/{id}/stats");
            return res is null ? throw new InvalidOperationException("LinkStats response returned a null object") : res;
        }

        public async Task RecordAccess(string id, string ipAddress, string? userAgent, string? referer)
        {
            // Cette méthode n'a pas besoin d'authentification car elle est appelée lors des redirections publiques
            var requestData = new
            {
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Referer = referer
            };

            var res = await _httpClient.PostAsJsonAsync($"/Links/{id}/access", requestData);
            res.EnsureSuccessStatusCode();
        }
    }
}
