using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Sanssoussi.Areas.Identity.Data;
using Sanssoussi.Models;
using Sanssoussi.Services;

namespace Sanssoussi.Controllers
{
    [Authorize]
    [EnableRateLimiting(Startup.RateLimitPolicy)]
    public class HomeController : Controller
    {
        private const int MaxSearchLength = 100;

        private readonly CommentService _comments;

        private readonly ILogger<HomeController> _logger;

        private readonly UserManager<SanssoussiUser> _userManager;

        // Never log passwords, tokens or the text of a comment: only who did what, and from where
        private string RemoteIp => this.HttpContext.Connection.RemoteIpAddress?.ToString();

        public HomeController(ILogger<HomeController> logger, UserManager<SanssoussiUser> userManager, CommentService comments)
        {
            this._logger = logger;
            this._userManager = userManager;
            this._comments = comments;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            this.ViewData["Message"] = "Parce que marcher devrait se faire SansSoussi";
            return this.View();
        }

        [HttpGet]
        public async Task<IActionResult> Comments()
        {
            return this.View(await this._comments.GetAsync(this._userManager.GetUserId(this.User)));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Comments(string comment)
        {
            var userId = this._userManager.GetUserId(this.User);
            if (string.IsNullOrWhiteSpace(comment) || comment.Length > Comment.MaxLength)
            {
                this._logger.LogWarning("Comment rejected (invalid input) for {UserId} from {Ip}", userId, this.RemoteIp);
                return this.BadRequest("Commentaire invalide");
            }

            if (!await this._comments.AddAsync(userId, comment))
            {
                this._logger.LogWarning("Comment rejected (quota reached) for {UserId} from {Ip}", userId, this.RemoteIp);
                return this.BadRequest("Nombre maximal de commentaires atteint");
            }

            this._logger.LogInformation("Comment added by {UserId} from {Ip}", userId, this.RemoteIp);
            return this.Ok("Commentaire ajouté");
        }

        public async Task<IActionResult> Search(string searchData)
        {
            if (string.IsNullOrEmpty(searchData))
            {
                return this.View(new List<string>());
            }

            var userId = this._userManager.GetUserId(this.User);
            if (searchData.Length > MaxSearchLength)
            {
                this._logger.LogWarning("Search rejected (invalid input) for {UserId} from {Ip}", userId, this.RemoteIp);
                return this.BadRequest("Recherche invalide");
            }

            this._logger.LogInformation("Search by {UserId} from {Ip}", userId, this.RemoteIp);
            return this.View(await this._comments.SearchAsync(userId, searchData));
        }

        [AllowAnonymous]
        public IActionResult About()
        {
            return this.View();
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return this.View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return this.View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public IActionResult Emails()
        {
            return this.View();
        }

        [HttpPost]
        [ActionName(nameof(Emails))]
        [Authorize(Roles = "admin")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EmailList()
        {
            this._logger.LogWarning("Email list read by {UserId} from {Ip}", this._userManager.GetUserId(this.User), this.RemoteIp);
            return this.Json(await this._userManager.Users.Select(u => u.Email).ToListAsync());
        }
    }
}
