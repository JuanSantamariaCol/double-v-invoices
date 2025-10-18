namespace InvoicesService.Application.DTOs.Responses;

public class InvoiceListResponse
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
