// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.


using Moq;

namespace System.ComponentModel.Design.Tests;

public class ReferenceServiceTests
{
    [Fact]
    public void ReferenceService_Ctor_IServiceProvider()
    {
        using Container container = new();
        Mock<IServiceProvider> provider = GetServiceProvider(container);

        ReferenceService service = new(provider.Object);

        Assert.NotNull(service);

        ((IDisposable)service).Dispose();
    }

    [Fact]
    public void ReferenceService_GetReference_String_ReturnsExpected()
    {
        using Container container = new();
        using Component component = new();

        container.Add(component, "component");

        Mock<IServiceProvider> provider = GetServiceProvider(container);
        ReferenceService referenceService = new(provider.Object);
        IReferenceService service = referenceService;

        object? result = service.GetReference("component");

        Assert.Same(component, result);

        ((IDisposable)referenceService).Dispose();
    }

    [Fact]
    public void ReferenceService_GetReference_InvalidName_ReturnsNull()
    {
        using Container container = new();

        Mock<IServiceProvider> provider = GetServiceProvider(container);
        ReferenceService referenceService = new(provider.Object);
        IReferenceService service = referenceService;

        object? result = service.GetReference("invalidName");

        Assert.Null(result);

        ((IDisposable)referenceService).Dispose();
    }

    [Fact]
    public void ReferenceService_GetName_Object_ReturnsExpected()
    {
        using Container container = new();
        using Component component = new();

        container.Add(component, "component");

        Mock<IServiceProvider> provider = GetServiceProvider(container);
        ReferenceService referenceService = new(provider.Object);
        IReferenceService service = referenceService;

        string? result = service.GetName(component);

        Assert.Equal("component", result);

        ((IDisposable)referenceService).Dispose();
    }

    [Fact]
    public void ReferenceService_GetComponent_Object_ReturnsExpected()
    {
        using Container container = new();
        using Component component = new();

        container.Add(component, "component");

        Mock<IServiceProvider> provider = GetServiceProvider(container);
        ReferenceService referenceService = new(provider.Object);
        IReferenceService service = referenceService;

        IComponent? result = service.GetComponent(component);

        Assert.Same(component, result);

        ((IDisposable)referenceService).Dispose();
    }

    [Fact]
    public void ReferenceService_GetReferences_ReturnsExpected()
    {
        using Container container = new();
        using Component component = new();

        container.Add(component, "component");

        Mock<IServiceProvider> provider = GetServiceProvider(container);
        ReferenceService referenceService = new(provider.Object);
        IReferenceService service = referenceService;

        object[] references = service.GetReferences();

        Assert.Contains(component, references);

        ((IDisposable)referenceService).Dispose();
    }

    [Fact]
    public void ReferenceService_GetReferences_Type_ReturnsExpected()
    {
        using Container container = new();
        using Component component = new();

        container.Add(component, "component");

        Mock<IServiceProvider> provider = GetServiceProvider(container);
        ReferenceService referenceService = new(provider.Object);
        IReferenceService service = referenceService;

        object[] references = service.GetReferences(typeof(Component));

        Assert.Contains(component, references);

        ((IDisposable)referenceService).Dispose();
    }

    private static Mock<IServiceProvider> GetServiceProvider(
        IContainer container)
    {
        Mock<IServiceProvider> provider = new();
        Mock<IComponentChangeService> changeService = new();

        provider
            .Setup(serviceProvider =>
                serviceProvider.GetService(typeof(IContainer)))
            .Returns(container);

        provider
            .Setup(serviceProvider =>
                serviceProvider.GetService(typeof(IComponentChangeService)))
            .Returns(changeService.Object);

        return provider;
    }
}
