using AssignmentManagement.Api.Dtos;
using FluentValidation;

namespace AssignmentManagement.Api.Validators;

public class CreateAssignmentRequestValidator : AbstractValidator<CreateAssignmentRequest>
{
    public CreateAssignmentRequestValidator()
    {
        RuleFor(x => x.ClassSubjectId)
            .GreaterThan(0);

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(5000);

        RuleFor(x => x.MaxMarks)
            .GreaterThan(0);

        RuleFor(x => x.DeadlineUtc)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Deadline must be in the future.");

        RuleFor(x => x.StartsAtUtc)
            .Must((request, start) => start is null || start < request.DeadlineUtc)
            .WithMessage("Start time must be before the deadline.")
            .When(x => x.DeadlineUtc > DateTime.UtcNow);
    }
}

public class UpdateAssignmentRequestValidator : AbstractValidator<UpdateAssignmentRequest>
{
    public UpdateAssignmentRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(5000);

        RuleFor(x => x.MaxMarks)
            .GreaterThan(0);

        RuleFor(x => x.DeadlineUtc)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Deadline must be in the future.");

        RuleFor(x => x.StartsAtUtc)
            .Must((request, start) => start is null || start < request.DeadlineUtc)
            .WithMessage("Start time must be before the deadline.")
            .When(x => x.DeadlineUtc > DateTime.UtcNow);
    }
}
