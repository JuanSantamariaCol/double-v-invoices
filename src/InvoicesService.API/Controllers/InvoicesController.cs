using InvoicesService.Application.DTOs.Requests;
using InvoicesService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InvoicesService.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;
    private readonly ILogger<InvoicesController> _logger;

    public InvoicesController(
        IInvoiceService invoiceService,
        ILogger<InvoicesController> logger)
    {
        _invoiceService = invoiceService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new invoice
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Create(
        [FromBody] CreateInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating invoice for customer {CustomerId}", request.CustomerId);

        var invoice = await _invoiceService.CreateInvoiceAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = invoice.Id },
            new
            {
                data = invoice,
                meta = new { timestamp = DateTime.UtcNow }
            });
    }

    /// <summary>
    /// Gets an invoice by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting invoice {InvoiceId}", id);

        var invoice = await _invoiceService.GetInvoiceByIdAsync(id, cancellationToken);

        return Ok(new
        {
            data = invoice,
            meta = new { timestamp = DateTime.UtcNow }
        });
    }

    /// <summary>
    /// Gets a paginated list of invoices with optional date filtering
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting invoices - Page: {Page}, PageSize: {PageSize}", page, pageSize);

        var result = await _invoiceService.GetInvoicesAsync(
            startDate,
            endDate,
            page,
            pageSize,
            cancellationToken);

        return Ok(new
        {
            data = result.Data,
            meta = new
            {
                currentPage = result.CurrentPage,
                totalPages = result.TotalPages,
                totalCount = result.TotalCount,
                pageSize = result.PageSize,
                hasPreviousPage = result.HasPreviousPage,
                hasNextPage = result.HasNextPage
            }
        });
    }

    /// <summary>
    /// Updates an existing invoice (only Draft status invoices can be updated)
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating invoice {InvoiceId}", id);

        var invoice = await _invoiceService.UpdateInvoiceAsync(id, request, cancellationToken);

        return Ok(new
        {
            data = invoice,
            meta = new { timestamp = DateTime.UtcNow }
        });
    }

    /// <summary>
    /// Deletes an invoice (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting invoice {InvoiceId}", id);

        await _invoiceService.DeleteInvoiceAsync(id, cancellationToken);

        return NoContent();
    }
}
