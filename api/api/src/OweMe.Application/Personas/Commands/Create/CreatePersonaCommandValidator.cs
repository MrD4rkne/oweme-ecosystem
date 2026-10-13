using FluentValidation;
using OweMe.Application.Personas.Commands.Create;

namespace OweMe.Application.Personas.Commands;

public sealed class CreatePersonaCommandValidator : AbstractValidator<CreatePersonaCommand>
{
    public CreatePersonaCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Persona name is required.")
            .MaximumLength(PersonaConstants.MaxNameLength)
            .WithMessage($"Persona name must not exceed {PersonaConstants.MaxNameLength} characters.");
    }
}