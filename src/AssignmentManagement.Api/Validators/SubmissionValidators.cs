using AssignmentManagement.Api.Domain;
using AssignmentManagement.Api.Dtos;
using FluentValidation;

namespace AssignmentManagement.Api.Validators;

public class CreateSubmissionRequestValidator : AbstractValidator<CreateSubmissionRequest>
{
    public CreateSubmissionRequestValidator()
    {
        RuleFor(x => x.AnswerText)
            .NotEmpty()
            .MaximumLength(20000);
    }
}

public class UpdateSubmissionRequestValidator : AbstractValidator<UpdateSubmissionRequest>
{
    public UpdateSubmissionRequestValidator()
    {
        RuleFor(x => x.AnswerText)
            .NotEmpty()
            .MaximumLength(20000);
    }
}

public class GradeSubmissionRequestValidator : AbstractValidator<GradeSubmissionRequest>
{
    public GradeSubmissionRequestValidator()
    {
        RuleFor(x => x.MarksObtained)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Feedback)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.Feedback));

        RuleFor(x => x.Status)
            .IsInEnum()
            .Must(x => x != SubmissionStatus.Submitted)
            .WithMessage("A submission cannot be graded back to 'Submitted'.");
    }
}
