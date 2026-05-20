namespace Denarius.Application.Outputs.Auth;

public record LoginOutput(string Token, UserOutput User);
