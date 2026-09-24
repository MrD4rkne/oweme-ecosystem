using FluentValidation;
using OweMe.Domain.Groups;

namespace OweMe.Application.Groups.Commands.Create;

public class CreateGroupCommandValidator : AbstractValidator<CreateGroupCommand>
{
    public CreateGroupCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Group name is required.")
            .MaximumLength(GroupConstants.MaxNameLength)
            .WithMessage($"Group name must not exceed {GroupConstants.MaxNameLength} characters.");

        RuleFor(x => x.Description)
            .MaximumLength(GroupConstants.MaxDescriptionLength)
            .WithMessage($"Group description must not exceed {GroupConstants.MaxDescriptionLength} characters.");
    }
}