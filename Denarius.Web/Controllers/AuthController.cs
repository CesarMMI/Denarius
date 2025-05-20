using Denarius.Application.Auth.Commands.Login;
using Denarius.Application.Auth.Commands.Refresh;
using Denarius.Application.Auth.Commands.Register;
using Microsoft.AspNetCore.Mvc;

namespace Denarius.Web.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController(ILoginCommand loginCommand, IRefreshCommand refreshCommand, IRegisterCommand registerCommand) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginQuery request)
        {
            return Ok(await loginCommand.Execute(request));
        }
        
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshQuery request)
        {
            return Ok(await refreshCommand.Execute(request));
        }
        
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterQuery request)
        {
            return Ok(await registerCommand.Execute(request));
        }
    }
}
