namespace InvoicesService.Application.DTOs.Requests;

public class CreateInvoiceRequest
{
    public string CustomerId { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public string? Notes { get; set; }
    public List<InvoiceItemDto> Items { get; set; } = new();
}
