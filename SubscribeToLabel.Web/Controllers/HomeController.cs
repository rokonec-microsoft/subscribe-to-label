using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DotNet.SubscribeToLabel.Web.Features.LabelSubscriptions;
using DotNet.SubscribeToLabel.Web.Features.Repositories;
using DotNet.SubscribeToLabel.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DotNet.SubscribeToLabel.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ILabelSubscription _userLabelSubscription;
        private readonly IListRepositories _listRepositories;

        public HomeController(ILogger<HomeController> logger, ILabelSubscription userLabelSubscription, IListRepositories listRepositories)
        {
            _logger = logger;
            _userLabelSubscription = userLabelSubscription;
            _listRepositories = listRepositories;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            string? userId = User.Identity.Name;

            if (userId is null)
                throw new InvalidOperationException("User.Identity.Name is null");

            var viewModel = await GetHomeViewModel(userId);

            return View(viewModel);
        }

        private async Task<HomeViewModel> GetHomeViewModel(string userId)
        {
            // TODO: remove it, and add repo selection process (in url segment) its for testing only, or cease to use it for a while
            var repos = await _listRepositories.GetAllRepositories();
            var labels = await _userLabelSubscription.GetUserSubscriptions(repos.First().Owner, repos.First().Name, userId);

            var labelsJoined = string.Join(", ", labels.Select(o => o.Label).OrderBy(o => o));
            var viewModel = new HomeViewModel(user: userId, labels: labelsJoined);

            return viewModel;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize]
        public IActionResult Profile()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(UpdateLabelSubscriptionsRequestModel updateLabelSubscriptions)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string? userId = User.Identity.Name;

            if (userId is null)
                throw new InvalidOperationException("User.Identity.Name is null");

            if (updateLabelSubscriptions is null)
                throw new ArgumentNullException(nameof(updateLabelSubscriptions));

            if (updateLabelSubscriptions.Labels is null)
                throw new ArgumentNullException(nameof(updateLabelSubscriptions.Labels));

            var labels = updateLabelSubscriptions.Labels
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(o => o.Trim()).ToList();

            var repos = await _listRepositories.GetAllRepositories();
            await _userLabelSubscription.SetUserSubscriptions(repos.First().Owner, repos.First().Name, userId, labels);

            var viewModel = await GetHomeViewModel(userId);

            return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
