using ASC.Business;
using ASC.Model;
using ASC.Web.Controllers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ASC.Web.Areas.ServiceRequests.Controllers
{
    [Area("ServiceRequests")]
    public class DashboardController : BaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IServiceRequestOperations _serviceRequestOps;

        public DashboardController(
            UserManager<ApplicationUser> userManager,
            IServiceRequestOperations serviceRequestOps)
        {
            _userManager = userManager;
            _serviceRequestOps = serviceRequestOps;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole(Constants.Roles.Admin);
            var isEngineer = User.IsInRole(Constants.Roles.Engineer);

            IEnumerable<ServiceRequest> requests;

            if (isAdmin)
                requests = await _serviceRequestOps.GetAllServiceRequestsAsync();
            else if (isEngineer)
                requests = await _serviceRequestOps.GetServiceRequestsByEngineerAsync(user!.Email!);
            else
                requests = await _serviceRequestOps.GetServiceRequestsByCustomerAsync(user!.Email!);

            ViewBag.TotalRequests = requests.Count();
            ViewBag.PendingRequests = requests.Count(r => r.Status == "Pending");
            ViewBag.QuoteSentRequests = requests.Count(r => r.Status == "QuoteSent");
            ViewBag.InProgressRequests = requests.Count(r => r.Status == "InProgress");
            ViewBag.CompletedRequests = requests.Count(r => r.Status == "Completed");

            return View(requests.OrderByDescending(r => r.RequestedDate).Take(10).ToList());
        }
    }
}
