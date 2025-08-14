using System.Text.Json;
using Microsoft.JSInterop;

namespace Web.Services
{
    public interface ILocalizationService
    {
        Task InitializeAsync();
        string GetString(string key);
        string GetString(string key, params object[] args);
        string CurrentLanguage { get; }
        Task SetLanguageAsync(string language);
        event Action? OnLanguageChanged;
        IEnumerable<(string Code, string Name)> GetAvailableLanguages();
    }

    public class LocalizationService : ILocalizationService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;
        private Dictionary<string, object> _translations = new();
        private string _currentLanguage = "en";

        public string CurrentLanguage => _currentLanguage;
        public event Action? OnLanguageChanged;

        private readonly Dictionary<string, string> _availableLanguages = new()
        {
            { "en", "English" },
            { "fr", "Français" }
        };

        public LocalizationService(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        public async Task InitializeAsync()
        {
            // Détection automatique de la langue du navigateur
            var browserLanguage = await GetBrowserLanguageAsync();
            var language = _availableLanguages.ContainsKey(browserLanguage) ? browserLanguage : "en";
            
            await SetLanguageAsync(language);
        }

        public async Task SetLanguageAsync(string language)
        {
            if (!_availableLanguages.ContainsKey(language))
                language = "en";

            try
            {
                var response = await _httpClient.GetAsync($"locales/{language}.json");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var translations = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                    
                    if (translations != null)
                    {
                        _translations = translations;
                        _currentLanguage = language;
                        
                        OnLanguageChanged?.Invoke();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement de la langue {language}: {ex.Message}");
                // Fallback vers l'anglais si erreur
                if (language != "en")
                {
                    await SetLanguageAsync("en");
                }
            }
        }

        public string GetString(string key)
        {
            return GetNestedValue(_translations, key) ?? key;
        }

        public string GetString(string key, params object[] args)
        {
            var value = GetString(key);
            try
            {
                return string.Format(value, args);
            }
            catch
            {
                return value;
            }
        }

        public IEnumerable<(string Code, string Name)> GetAvailableLanguages()
        {
            return _availableLanguages.Select(kvp => (kvp.Key, kvp.Value));
        }

        private string? GetNestedValue(Dictionary<string, object> dict, string key)
        {
            var keys = key.Split('.');
            object current = dict;

            foreach (var k in keys)
            {
                if (current is Dictionary<string, object> currentDict && currentDict.ContainsKey(k))
                {
                    current = currentDict[k];
                }
                else if (current is JsonElement element)
                {
                    if (element.TryGetProperty(k, out var prop))
                    {
                        current = prop;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }

            return current switch
            {
                string str => str,
                JsonElement elem when elem.ValueKind == JsonValueKind.String => elem.GetString(),
                _ => null
            };
        }

        private async Task<string> GetBrowserLanguageAsync()
        {
            try
            {
                // Utiliser l'interop JavaScript pour détecter la langue du navigateur
                var browserLanguage = await _jsRuntime.InvokeAsync<string>("getBrowserLanguage");
                return browserLanguage ?? "en";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la détection de la langue du navigateur: {ex.Message}");
                return "en";
            }
        }
    }
}
