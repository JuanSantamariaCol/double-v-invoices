namespace InvoicesService.Application.DTOs.Requests;

public class UpdateInvoiceRequest
{
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public string? Notes { get; set; }
    public List<InvoiceItemDto> Items { get; set; } = new();
}
