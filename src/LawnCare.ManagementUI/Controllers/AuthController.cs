using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace LawnCare.ManagementUI.Controllers;

public class AuthController : Controller
{
    [HttpGet("login")]
    public IActionResult Login(string returnUrl = "/")
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = returnUrl
        };
        
        return Challenge(properties, "Auth0");
    }

    [HttpGet("logout")]
    public IActionResult Logout()
    {
        return SignOut(new AuthenticationProperties
        {
            RedirectUri = "/login"
        }, "Auth0");
    }
}
