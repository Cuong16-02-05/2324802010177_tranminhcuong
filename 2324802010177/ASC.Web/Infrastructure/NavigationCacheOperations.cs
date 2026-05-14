using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace ASC.Web.Infrastructure
{
    public class NavigationMenu
    {
        public List<MenuItem> MenuItems { get; set; } = new();
    }

    public class MenuItem
    {
        public string? DisplayName { get; set; }
        public string? MaterialIcon { get; set; }
        public string? Link { get; set; }
        public bool IsNested { get; set; }
        public int Sequence { get; set; }
        public List<string> UserRoles { get; set; } = new();
        public List<MenuItem> NestedItems { get; set; } = new();
    }

    public interface INavigationCacheOperations
    {
        Task CreateNavigationCacheAsync();
        NavigationMenu? GetNavigationMenu();
    }

    public class NavigationCacheOperations : INavigationCacheOperations
    {
        private readonly IMemoryCache _cache;
        private readonly IWebHostEnvironment _env;
        private const string CacheKey = "NavigationCache";

        public NavigationCacheOperations(IMemoryCache cache, IWebHostEnvironment env)
        { _cache = cache; _env = env; }

        public async Task CreateNavigationCacheAsync()
        {
            var path = Path.Combine(_env.ContentRootPath, "Navigation.json");
            if (File.Exists(path))
            {
                var json = await File.ReadAllTextAsync(path);
                var menu = JsonConvert.DeserializeObject<NavigationMenu>(json);
                _cache.Set(CacheKey, menu, TimeSpan.FromHours(24));
            }
        }

        public NavigationMenu? GetNavigationMenu() => _cache.Get<NavigationMenu>(CacheKey);
    }
}
