using Denarius.Domain.Entities;
using Denarius.Domain.Exceptions;

namespace Denarius.Domain.Tests.Entities;

public class AccountTests
{
    private static Account ValidAccount() =>
        new(Guid.NewGuid(), "Nubank", "BRL", "#8A05BE");

    // -------------------------------------------------------------------------
    // Creation — happy path
    // -------------------------------------------------------------------------

    [Fact]
    public void Create_WithValidFields_SetsPropertiesCorrectly()
    {
        var userId = Guid.NewGuid();
        var account = new Account(userId, "Nubank", "BRL", "#8A05BE");

        Assert.Equal(userId, account.UserId);
        Assert.Equal("Nubank", account.Name);
        Assert.Equal("BRL", account.CurrencyCode);
        Assert.Equal("#8A05BE", account.Color);
        Assert.NotEqual(Guid.Empty, account.Id);
    }

    [Fact]
    public void Create_BalanceIsZero()
    {
        var account = ValidAccount();

        Assert.Equal(0, account.Balance);
    }

    [Fact]
    public void Create_IsActiveIsTrue()
    {
        var account = ValidAccount();

        Assert.True(account.IsActive);
    }

    // -------------------------------------------------------------------------
    // Creation — errors
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidName_Throws(string? name)
    {
        Assert.Throws<InvalidNameException>(() =>
            new Account(Guid.NewGuid(), name!, "BRL", "#000000"));
    }

    [Theory]
    [InlineData("BR")]       // too short
    [InlineData("BRLL")]     // too long
    [InlineData("")]         // empty
    public void Create_WithCurrencyCodeWrongLength_Throws(string code)
    {
        Assert.Throws<InvalidCurrencyCodeException>(() =>
            new Account(Guid.NewGuid(), "Test", code, "#000000"));
    }

    [Theory]
    [InlineData("brl")]   // lowercase
    [InlineData("Brl")]   // mixed case
    [InlineData("BR1")]   // contains digit
    [InlineData("1BR")]   // starts with digit
    public void Create_WithInvalidCurrencyCodeChars_Throws(string code)
    {
        Assert.Throws<InvalidCurrencyCodeException>(() =>
            new Account(Guid.NewGuid(), "Test", code, "#000000"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidColor_Throws(string? color)
    {
        Assert.Throws<InvalidColorException>(() =>
            new Account(Guid.NewGuid(), "Test", "BRL", color!));
    }

    // -------------------------------------------------------------------------
    // ApplyDelta
    // -------------------------------------------------------------------------

    [Fact]
    public void ApplyDelta_PositiveDelta_IncreasesBalance()
    {
        var account = ValidAccount();

        account.ApplyDelta(100m);

        Assert.Equal(100m, account.Balance);
    }

    [Fact]
    public void ApplyDelta_NegativeDelta_DecreasesBalance()
    {
        var account = ValidAccount();
        account.ApplyDelta(200m);

        account.ApplyDelta(-50m);

        Assert.Equal(150m, account.Balance);
    }

    [Fact]
    public void ApplyDelta_NegativeDelta_AllowsNegativeBalance()
    {
        var account = ValidAccount();

        account.ApplyDelta(-500m);

        Assert.Equal(-500m, account.Balance);
    }

    [Fact]
    public void ApplyDelta_ZeroDelta_Throws()
    {
        var account = ValidAccount();

        Assert.Throws<InvalidDeltaException>(() => account.ApplyDelta(0));
    }

    // -------------------------------------------------------------------------
    // Deactivate
    // -------------------------------------------------------------------------

    [Fact]
    public void Deactivate_ActiveAccount_SetsIsActiveToFalse()
    {
        var account = ValidAccount();

        account.Deactivate();

        Assert.False(account.IsActive);
    }

    [Fact]
    public void Deactivate_AlreadyInactiveAccount_IsIdempotent()
    {
        var account = ValidAccount();
        account.Deactivate();

        var exception = Record.Exception(() => account.Deactivate());

        Assert.Null(exception);
        Assert.False(account.IsActive);
    }
}
