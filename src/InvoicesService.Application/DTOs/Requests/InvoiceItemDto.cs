namespace InvoicesService.Application.DTOs.Requests;

public class InvoiceItemDto
{
    public string ProductCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
}
