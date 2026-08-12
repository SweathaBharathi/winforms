// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.Reflection;

namespace System.ComponentModel.Design.Tests;

public class InheritanceServicerTests
{
    [Fact]
    public void InheritanceService_Constructor()
    {
        InheritanceService underTest = new();
        Assert.NotNull(underTest);
    }

    [Fact]
    public void InheritanceService_Ctor()
    {
        using InheritanceService service = new();

        Assert.NotNull(service);
    }

    [Fact]
    public void InheritanceService_IgnoreInheritedMember_MemberInfo_IComponent()
    {
        MemberInfo member = typeof(Component).GetMethod(nameof(Component.Dispose))!;
        using Component component = new();
        using SubInheritanceService service = new();

        bool result = service.IgnoreInheritedMember(member, component);

        Assert.False(result);
    }

    private sealed class SubInheritanceService : InheritanceService
    {
        public new bool IgnoreInheritedMember(
            MemberInfo member,
            IComponent component)
        {
            return base.IgnoreInheritedMember(member, component);
        }
    }
}
