// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.


namespace System.ComponentModel.Design.Tests;

public class LoadedEventHandlerTests
{
    [Fact]
    public void LoadedEventHandler_Invoke_Object_LoadedEventArgs()
    {
        object expectedSender = new();
        LoadedEventArgs expectedEventArgs = new(
            succeeded: true,
            errors: null);

        object? actualSender = null;
        LoadedEventArgs? actualEventArgs = null;

        LoadedEventHandler handler = (sender, e) =>
        {
            actualSender = sender;
            actualEventArgs = e;
        };

        handler(expectedSender, expectedEventArgs);

        Assert.Same(expectedSender, actualSender);
        Assert.Same(expectedEventArgs, actualEventArgs);
    }
}
