using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using OweMe.Api.Description;
using OweMe.Api.Identity;
using OweMe.Application.Personas.Commands;
using OweMe.Application.Personas.Commands.Create;
using Wolverine;

namespace OweMe.Api.Endpoints.Personas;

public sealed class CreatePersonaEndpoint : IEndpoint
{
    [ExcludeFromCodeCoverage]
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/api/personas", CreatePersona)
            .WithName("CreatePersona")
            .WithDescription("Create a persona. A persona is a representation of a user in the system.")
            .WithTags(Tags.Personas)
            .Produces(StatusCodes.Status201Created)
            .ProducesExtendedProblem(StatusCodes.Status400BadRequest)
            .WithStandardProblems()
            .RequireAuthorization(Constants.POLICY_API_SCOPE);
    }

    public static async Task<IResult> CreatePersona(
        [FromBody] CreatePersonaCommand createPersonaRequest,
        IMessageBus messageBus,
        CancellationToken cancellationToken = default)
    {
        var personaCreated = await messageBus.InvokeAsync<CreatePersonaCommandHandler.PersonaCreated>(createPersonaRequest,
                cancellationToken);
        return Results.Created($"/api/personas/{personaCreated.Id}", null);
    }
}
