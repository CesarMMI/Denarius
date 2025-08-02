using Denarius.Application.Domain.Commands.Auth;
using Denarius.Application.Domain.Queries.Auth;
using Denarius.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Denarius.Web.Controllers;

[Route("[controller]")]
[ApiController]
public class AuthController(
    ILoginCommand loginCommand,
    IRefreshCommand refreshCommand,
    IRegisterCommand registerCommand
) : ControllerBase
{
    [HttpPost("login")]
    public Task<IActionResult> Login([FromBody] LoginQuery body) => loginCommand
        .Execute(body)
        .Ok();

    [HttpPost("refresh")]
    public Task<IActionResult> Refresh([FromBody] RefreshQuery body) => refreshCommand
        .Execute(body)
        .Ok();

    [HttpPost("register")]
    public Task<IActionResult> Register([FromBody] RegisterQuery body) => registerCommand
        .Execute(body)
        .Created();
}
