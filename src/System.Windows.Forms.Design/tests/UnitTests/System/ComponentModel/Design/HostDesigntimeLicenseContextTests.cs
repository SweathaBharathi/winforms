// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.



using Moq;

namespace System.ComponentModel.Design.Tests;

public class HostDesigntimeLicenseContextTests
{
    [Fact]
    public void HostDesigntimeLicenseContext_Ctor_IServiceProvider()
    {
        Mock<IServiceProvider> serviceProvider = new();

        HostDesigntimeLicenseContext context =
            new(serviceProvider.Object);

        Assert.Equal(
            LicenseUsageMode.Designtime,
            context.UsageMode);
    }

    [Fact]
    public void HostDesigntimeLicenseContext_GetSavedLicenseKey_NoKey_ReturnsNull()
    {
        Mock<IServiceProvider> serviceProvider = new();

        HostDesigntimeLicenseContext context =
            new(serviceProvider.Object);

        string? result = context.GetSavedLicenseKey(
            typeof(Component),
            resourceAssembly: null);

        Assert.Null(result);
    }

    [Fact]
    public void HostDesigntimeLicenseContext_SetSavedLicenseKey_GetReturnsNull()
    {
        Mock<IServiceProvider> serviceProvider = new();

        HostDesigntimeLicenseContext context =
            new(serviceProvider.Object);

        context.SetSavedLicenseKey(
            typeof(Component),
            "licenseKey");

        string? result = context.GetSavedLicenseKey(
            typeof(Component),
            resourceAssembly: null);

        Assert.Null(result);
    }
}
