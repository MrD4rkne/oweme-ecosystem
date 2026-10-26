using FluentValidation;

namespace OweMe.Application.Groups.Commands.AddMember;

public sealed class AddGroupMemberCommandValidator : AbstractValidator<AddGroupMemberCommand>
{
    public AddGroupMemberCommandValidator()
    {
        RuleFor(x => x.PersonaId)
            .NotEmpty()
            .WithMessage("Persona id is required.");
    }
}
