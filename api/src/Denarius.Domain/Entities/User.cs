using Denarius.Domain.Exceptions;
using Denarius.Domain.Exceptions.Users;

namespace Denarius.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string Name { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public User(string email, string passwordHash, string name)
    {
        if (!IsValidEmail(email))
            throw new InvalidEmailException();

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new InvalidPasswordHashException();

        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidNameException();

        if (name.Trim().Length < 5)
            throw new InvalidNameException("Name must be at least 5 characters.");

        Id = Guid.NewGuid();
        Email = email;
        PasswordHash = passwordHash;
        Name = name;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidNameException();

        if (name.Trim().Length < 2)
            throw new InvalidNameException("Name must be at least 2 characters.");

        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangePassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new InvalidPasswordHashException();

        PasswordHash = passwordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    private static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var atIndex = email.IndexOf('@');
        if (atIndex <= 0 || atIndex != email.LastIndexOf('@'))
            return false;

        var local = email[..atIndex];
        var domain = email[(atIndex + 1)..];

        if (string.IsNullOrWhiteSpace(local) || string.IsNullOrWhiteSpace(domain))
            return false;

        var dotIndex = domain.LastIndexOf('.');
        return dotIndex > 0 && dotIndex < domain.Length - 1;
    }
}
