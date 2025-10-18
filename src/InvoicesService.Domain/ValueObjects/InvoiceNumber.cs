using System.Text.RegularExpressions;

namespace InvoicesService.Domain.ValueObjects;

public record InvoiceNumber
{
    private static readonly Regex InvoiceNumberPattern = new Regex(@"^INV-\d{4}-\d{6}$", RegexOptions.Compiled);

    public string Value { get; }

    public InvoiceNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Invoice number cannot be empty", nameof(value));

        if (!InvoiceNumberPattern.IsMatch(value))
            throw new ArgumentException($"Invoice number must match format INV-YYYY-NNNNNN. Received: {value}", nameof(value));

        Value = value;
    }

    public static InvoiceNumber Generate(int year, int sequenceNumber)
    {
        var value = $"INV-{year:D4}-{sequenceNumber:D6}";
        return new InvoiceNumber(value);
    }

    public static implicit operator string(InvoiceNumber invoiceNumber) => invoiceNumber.Value;
    public override string ToString() => Value;
}
