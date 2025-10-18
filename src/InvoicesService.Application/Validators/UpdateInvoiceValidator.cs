using FluentValidation;
using InvoicesService.Application.DTOs.Requests;

namespace InvoicesService.Application.Validators;

public class UpdateInvoiceValidator : AbstractValidator<UpdateInvoiceRequest>
{
    public UpdateInvoiceValidator()
    {
        RuleFor(x => x.IssueDate)
            .NotEmpty().WithMessage("Issue date is required")
            .Must(BeAValidDate).WithMessage("Issue date must be a valid date");

        RuleFor(x => x.DueDate)
            .NotEmpty().WithMessage("Due date is required")
            .Must(BeAValidDate).WithMessage("Due date must be a valid date")
            .GreaterThanOrEqualTo(x => x.IssueDate)
            .WithMessage("Due date must be greater than or equal to issue date");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one item is required")
            .Must(x => x.Count > 0).WithMessage("At least one item is required");

        RuleForEach(x => x.Items).SetValidator(new InvoiceItemValidator());
    }

    private bool BeAValidDate(DateTime date)
    {
        return date != default && date > DateTime.MinValue;
    }
}
