using Denarius.Application.Commands.Accounts;
using Denarius.Application.Interfaces.Accounts;
using Denarius.Application.Results;
using Microsoft.AspNetCore.Mvc;

namespace Denarius.API.Controllers
{
    [ApiController]
    [Route("accounts")]
    public class AccountsController(
        ICreateAccountUseCase createAccountUseCase,
        IDeleteAccountUseCase deleteAccountUseCase,
        IGetAllAccountsUseCase getAllAccountsUseCase,
        IUpdateAccountUseCase updateAccountUseCase) : ControllerBase
    {

        [HttpGet]
        public IEnumerable<AccountResult> GetAll() =>
            getAllAccountsUseCase.Execute(new());

        [HttpPost]
        public Task<AccountResult> Create([FromBody] CreateAccountCommand command) =>
            createAccountUseCase.Execute(command);

        [HttpPut("{id:Guid}")]
        public Task<AccountResult> Update(Guid id, [FromBody] UpdateAccountCommand command) =>
            updateAccountUseCase.Execute(command with { Id = id });

        [HttpDelete("{id:Guid}")]
        public Task<AccountResult> Delete(Guid id) =>
            deleteAccountUseCase.Execute(new() { Id = id });
    }
}
