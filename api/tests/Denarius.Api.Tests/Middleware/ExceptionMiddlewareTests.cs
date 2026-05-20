using Denarius.Api.Middleware;
using Denarius.Application.Exceptions.Accounts;
using Denarius.Application.Exceptions.Transactions;
using Denarius.Application.Exceptions.Users;
using Denarius.Domain.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Denarius.Api.Tests.Middleware;

public class ExceptionMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenNoExceptionThrown_CallsNext()
    {
        var nextCalled = false;
        var middleware = new ExceptionMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(CreateContext());

        Assert.True(nextCalled);
    }

    [Theory]
    [InlineData("NotFoundException", 404)]
    [InlineData("InactiveAccountException", 422)]
    [InlineData("InvalidDateRangeException", 400)]
    [InlineData("DomainException", 400)]
    [InlineData("AppException", 400)]
    public async Task InvokeAsync_WhenExceptionThrown_ReturnsExpectedStatusCode(string exceptionKind, int expectedStatus)
    {
        var middleware = new ExceptionMiddleware(_ => throw CreateException(exceptionKind));
        var context = CreateContext();

        await middleware.InvokeAsync(context);

        Assert.Equal(expectedStatus, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WhenUnknownExceptionThrown_Returns500()
    {
        var middleware = new ExceptionMiddleware(_ => throw new Exception("unexpected"));
        var context = CreateContext();

        await middleware.InvokeAsync(context);

        Assert.Equal(500, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionThrown_SetsContentTypeToApplicationProblemJson()
    {
        var middleware = new ExceptionMiddleware(_ => throw new InvalidDateRangeException());
        var context = CreateContext();

        await middleware.InvokeAsync(context);

        Assert.Equal("application/problem+json", context.Response.ContentType);
    }

    private static DefaultHttpContext CreateContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static Exception CreateException(string kind) => kind switch
    {
        "NotFoundException" => new AccountNotFoundException(Guid.NewGuid()),
        "InactiveAccountException" => new InactiveAccountException(Guid.NewGuid()),
        "InvalidDateRangeException" => new InvalidDateRangeException(),
        "DomainException" => new InvalidNameException(),
        "AppException" => new InvalidCredentialsException(),
        _ => throw new ArgumentException($"Unknown kind: {kind}")
    };
}
