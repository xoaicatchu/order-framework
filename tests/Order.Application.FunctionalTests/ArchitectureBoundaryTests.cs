using Shouldly;
using WolverineApp.Application.Commands.Orders.CreateOrder;
using Xunit;

namespace Order.Application.FunctionalTests;

public sealed class ArchitectureBoundaryTests
{
    [Fact]
    public void ApplicationAssemblyMustNotReferenceInfrastructure()
    {
        var referencesInfrastructure = typeof(CreateOrderCommand).Assembly
            .GetReferencedAssemblies()
            .Any(assembly => assembly.Name == "Order.Infrastructure");

        referencesInfrastructure.ShouldBeFalse();
    }
}
