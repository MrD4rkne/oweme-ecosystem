using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OweMe.Application.Common;
using OweMe.Application.User;

namespace OweMe.Application;

[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    public static void AddApplication(this IHostApplicationBuilder builder)
    {
        builder.Services.AddOptions<ApplicationOptions>();

        builder.Services.AddUserContext();

        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
    }
}