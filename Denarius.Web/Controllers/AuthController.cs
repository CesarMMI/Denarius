using Denarius.Application.Auth.Commands.Login;
using Denarius.Application.Auth.Commands.Refresh;
using Denarius.Application.Auth.Commands.Register;
using Microsoft.AspNetCore.Mvc;

namespace Denarius.Web.Controllers;

[Route("[controller]")]
[ApiController]
public class AuthController(ILoginCommand loginCommand, IRefreshCommand refreshCommand, IRegisterCommand registerCommand) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginQuery body)
    {
        return Ok(await loginCommand.Execute(body));
    }
    
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshQuery body)
    {
        return Ok(await refreshCommand.Execute(body));
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterQuery body)
    {
        return Ok(await registerCommand.Execute(body));
    }
}
