namespace Denarius.Application.Auth.Services;

public interface IPasswordService
{
    string Hash(string password);
    bool Verify(string hashedPassword, string password);
}
