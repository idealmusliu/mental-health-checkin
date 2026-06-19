using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace MentalHealth.Tests.Api;

// Minimal MVC plumbing (HttpContext + ProblemDetailsFactory) so ControllerBase
// helpers like Problem()/ValidationProblem() work without a web host.
public static class ApiTestSetup
{
    private static readonly IServiceProvider Provider = BuildProvider();

    private static IServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddControllers();
        return services.BuildServiceProvider();
    }

    public static T Configure<T>(T controller) where T : ControllerBase
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { RequestServices = Provider }
        };
        controller.ProblemDetailsFactory = Provider.GetRequiredService<ProblemDetailsFactory>();
        return controller;
    }
}
