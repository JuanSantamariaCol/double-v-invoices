using FluentValidation;
using InvoicesService.Application.DTOs.Requests;

namespace InvoicesService.Application.Validators;

public class CreateInvoiceValidator : AbstractValidator<CreateInvoiceRequest>
{
    public CreateInvoiceValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required")
            .MaximumLength(50).WithMessage("Customer ID cannot exceed 50 characters");

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

public class InvoiceItemValidator : AbstractValidator<InvoiceItemDto>
{
    public InvoiceItemValidator()
    {
        RuleFor(x => x.ProductCode)
            .NotEmpty().WithMessage("Product code is required")
            .MaximumLength(50).WithMessage("Product code cannot exceed 50 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Unit price cannot be negative");

        RuleFor(x => x.TaxRate)
            .GreaterThanOrEqualTo(0).WithMessage("Tax rate cannot be negative")
            .LessThanOrEqualTo(1).WithMessage("Tax rate cannot exceed 1 (100%)");
    }
}
