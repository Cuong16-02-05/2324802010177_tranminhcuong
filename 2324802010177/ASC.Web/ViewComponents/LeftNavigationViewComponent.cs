using ASC.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ASC.Web.ViewComponents
{
    public class LeftNavigationViewComponent : ViewComponent
    {
        private readonly INavigationCacheOperations _navCache;
        public LeftNavigationViewComponent(INavigationCacheOperations navCache) { _navCache = navCache; }

        public IViewComponentResult Invoke()
        {
            var menu = _navCache.GetNavigationMenu();
            var userRoles = ((ClaimsPrincipal)User).Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value).ToList();

            var filteredItems = menu?.MenuItems
                .Where(m => m.UserRoles.Any(r => userRoles.Contains(r)))
                .OrderBy(m => m.Sequence).ToList();

            return View("Default", filteredItems);
        }
    }
}
