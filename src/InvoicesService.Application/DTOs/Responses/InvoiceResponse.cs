namespace InvoicesService.Application.DTOs.Responses;

public class InvoiceResponse
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerIdentification { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<InvoiceItemResponse> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
