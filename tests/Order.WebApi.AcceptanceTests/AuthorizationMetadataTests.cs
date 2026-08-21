using Microsoft.AspNetCore.Authorization;
using Shouldly;
using WolverineApp.Application.Common.Authorization;
using WolverineApp.Controllers;
using Xunit;

namespace Order.WebApi.AcceptanceTests;

public sealed class AuthorizationMetadataTests
{
    [Fact]
    public void PermissionMatrixMustNotBeAnonymous()
    {
        var method = typeof(RolesController).GetMethod(nameof(RolesController.GetPermissionsMatrix));
        method.ShouldNotBeNull();
        method!.GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true).ShouldBeEmpty();
        method.GetCustomAttributes(typeof(HasPermissionAttribute), inherit: true).ShouldNotBeEmpty();
    }
}
