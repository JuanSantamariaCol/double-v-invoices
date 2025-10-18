namespace InvoicesService.Application.DTOs.Responses;

public class InvoiceItemResponse
{
    public Guid Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
}
