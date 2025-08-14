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
            // Vérifier d'abord si une langue est sauvegardée dans le localStorage
            var savedLanguage = await GetSavedLanguageAsync();

            string language;
            if (!string.IsNullOrEmpty(savedLanguage) && _availableLanguages.ContainsKey(savedLanguage))
            {
                language = savedLanguage;
            }
            else
            {
                // Détection automatique de la langue du navigateur si aucune langue sauvegardée
                var browserLanguage = await GetBrowserLanguageAsync();
                language = _availableLanguages.ContainsKey(browserLanguage) ? browserLanguage : "en";
            }

            await SetLanguageAsync(language, saveToStorage: false); // Ne pas sauvegarder lors de l'initialisation
        }

        public async Task SetLanguageAsync(string language)
        {
            await SetLanguageAsync(language, saveToStorage: true);
        }

        private async Task SetLanguageAsync(string language, bool saveToStorage)
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

                        // Sauvegarder dans le localStorage si demandé (changement manuel)
                        if (saveToStorage)
                        {
                            await SaveLanguageToStorageAsync(language);
                        }

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
                    await SetLanguageAsync("en", saveToStorage);
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

        private async Task<string?> GetSavedLanguageAsync()
        {
            try
            {
                return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "preferredLanguage");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération de la langue sauvegardée: {ex.Message}");
                return null;
            }
        }

        private async Task SaveLanguageToStorageAsync(string language)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "preferredLanguage", language);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la sauvegarde de la langue: {ex.Message}");
            }
        }
    }
}
