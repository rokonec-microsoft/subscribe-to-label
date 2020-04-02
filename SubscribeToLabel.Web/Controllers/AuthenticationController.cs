using AspNet.Security.OAuth.GitHub;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Mvc.Client.Controllers
{
    public class AuthenticationController : Controller
    {
        [HttpGet("~/login")]
        public IActionResult LogIn() => View("Login");

        [HttpPost("~/login")]
        public IActionResult GitHubLogin()
        {
            return Challenge(new AuthenticationProperties { RedirectUri = "/" }, GitHubAuthenticationDefaults.AuthenticationScheme);
        }

        [HttpGet("~/logout"), HttpPost("~/logout")]
        public IActionResult SignOut()
        {
            return SignOut(new AuthenticationProperties { RedirectUri = "/" }, CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
