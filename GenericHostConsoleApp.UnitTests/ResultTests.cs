using System;
using GenericHostConsoleApp.Helpers;
using Xunit;

namespace GenericHostConsoleApp.UnitTests;

public class ResultTests
{
    [Fact]
    public void Result_OnSucceeded_SuccessIsTrue()
    {
        var result = Result.Succeeded();

        Assert.True(result.Success);
    }

    [Fact]
    public void Result_OnSucceeded_ExceptionIsNull()
    {
        var result = Result.Succeeded();

        Assert.Null(result.Exception);
    }

    [Fact]
    public void Result_OnSucceededWithContext_KeepsContext()
    {
        var context = new object();
        var result = Result.Succeeded(context);

        Assert.Equal(result.Context, context);
    }

    [Fact]
    public void Result_OnSucceeded_KeepsValue()
    {
        var value = new object();
        var result = Result.Succeeded<object>(value);

        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void Result_OnFailed_SuccessIsFalse()
    {
        var result = Result.Failed(new Exception());

        Assert.False(result.Success);
    }

    [Fact]
    public void Result_OnFailed_KeepsException()
    {
        var exception = new Exception();
        var result = Result.Failed(exception);

        Assert.Equal(result.Exception, exception);
    }

    [Fact]
    public void Result_OnFailedWithContext_KeepsContext()
    {
        var exception = new Exception();
        var context = new object();
        var result = Result.Failed(exception, context);

        Assert.Equal(result.Context, context);
    }

    [Fact]
    public void Result_OnFailed_ThrowsWhenAccessingValue()
    {
        var result = Result.Failed<string>(new Exception());

        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void Result_OnSuccessWithException_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _ = new Result(true, new Exception(), null));
    }

    [Fact]
    public void Result_OnFAiledWithoutException_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _ = new Result(false, null, null));
    }
}