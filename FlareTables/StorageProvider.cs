using System.Threading.Tasks;
using Blazored.SessionStorage;
using Blazored.LocalStorage;

namespace FlareTables
{
    public interface IStorageProvider
    {
        Task<T> GetItemAsync<T>(string key);
        Task    SetItemAsync<T>(string key, T data);
    }

    public class LocalStorageProvider : IStorageProvider
    {
        private readonly ILocalStorageService _localStorage;

        public LocalStorageProvider(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public Task<T> GetItemAsync<T>(string key) => _localStorage.GetItemAsync<T>(key);

        public Task SetItemAsync<T>(string key, T data) => _localStorage.SetItemAsync(key, data);
    }

    public class SessionStorageProvider : IStorageProvider
    {
        private readonly ISessionStorageService _sessionStorage;

        public SessionStorageProvider(ISessionStorageService sessionStorage)
        {
            _sessionStorage = sessionStorage;
        }

        public Task<T> GetItemAsync<T>(string key) => _sessionStorage.GetItemAsync<T>(key);

        public Task SetItemAsync<T>(string key, T data) => _sessionStorage.SetItemAsync(key, data);
    }
}