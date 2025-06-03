using Denarius.Application.Auth.Commands.Login;
using Denarius.Application.Auth.Commands.Refresh;
using Denarius.Application.Auth.Commands.Register;
using Microsoft.AspNetCore.Mvc;

namespace Denarius.Web.Controllers;

[Route("[controller]")]
[ApiController]
public class AuthController(
    ILoginCommand loginCommand,
    IRefreshCommand refreshCommand,
    IRegisterCommand registerCommand
) : Controller
{
    [HttpPost("login")]
    public Task<IActionResult> Login([FromBody] LoginQuery body)
    {
        return HandleCommand(loginCommand, body);
    }   

    [HttpPost("refresh")]
    public Task<IActionResult> Refresh([FromBody] RefreshQuery body)
    {
        return HandleCommand(refreshCommand, body);
    }

    [HttpPost("register")]
    public Task<IActionResult> Register([FromBody] RegisterQuery body)
    {
        return HandleCommand(registerCommand, body);
    }
}
