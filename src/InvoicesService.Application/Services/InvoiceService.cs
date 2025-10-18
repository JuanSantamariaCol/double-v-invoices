using AutoMapper;
using InvoicesService.Application.DTOs.Requests;
using InvoicesService.Application.DTOs.Responses;
using InvoicesService.Application.Interfaces;
using InvoicesService.Domain.Entities;
using InvoicesService.Domain.Enums;
using InvoicesService.Domain.Exceptions;
using InvoicesService.Domain.Interfaces;
using System.Text.Json;

namespace InvoicesService.Application.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICustomerValidationService _customerValidationService;
    private readonly IMapper _mapper;

    public InvoiceService(
        IInvoiceRepository invoiceRepository,
        IOutboxRepository outboxRepository,
        IUnitOfWork unitOfWork,
        ICustomerValidationService customerValidationService,
        IMapper mapper)
    {
        _invoiceRepository = invoiceRepository;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
        _customerValidationService = customerValidationService;
        _mapper = mapper;
    }

    public async Task<InvoiceResponse> CreateInvoiceAsync(CreateInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        // Validate customer exists
        var customerInfo = await _customerValidationService.GetCustomerInfoAsync(request.CustomerId, cancellationToken);
        if (customerInfo == null)
        {
            throw new InvalidCustomerException(request.CustomerId);
        }

        // Generate invoice number
        var invoiceNumber = await _invoiceRepository.GenerateInvoiceNumberAsync(cancellationToken);

        // Create invoice entity
        var invoice = new Invoice(
            invoiceNumber,
            customerInfo.Id,
            customerInfo.Name,
            customerInfo.Identification,
            request.IssueDate,
            request.DueDate,
            request.Notes);

        // Add items
        foreach (var itemDto in request.Items)
        {
            var item = new InvoiceItem(
                itemDto.ProductCode,
                itemDto.Description,
                itemDto.Quantity,
                itemDto.UnitPrice,
                itemDto.TaxRate);

            invoice.AddItem(item);
        }

        // Validate invoice
        invoice.Validate();

        // Begin transaction
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // Save invoice
            await _invoiceRepository.AddAsync(invoice, cancellationToken);

            // Create outbox message
            var outboxMessage = CreateOutboxMessage(
                invoice.Id.ToString(),
                "Invoice",
                "invoice.created",
                new
                {
                    InvoiceId = invoice.Id,
                    InvoiceNumber = invoice.InvoiceNumber,
                    CustomerId = invoice.CustomerId,
                    CustomerName = invoice.CustomerName,
                    IssueDate = invoice.IssueDate,
                    DueDate = invoice.DueDate,
                    TotalAmount = invoice.TotalAmount,
                    Status = invoice.Status.ToString(),
                    CreatedAt = invoice.CreatedAt
                });

            await _outboxRepository.AddAsync(outboxMessage, cancellationToken);

            // Commit transaction
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return _mapper.Map<InvoiceResponse>(invoice);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<InvoiceResponse> GetInvoiceByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id, cancellationToken);
        if (invoice == null)
        {
            throw new InvoiceNotFoundException(id);
        }

        return _mapper.Map<InvoiceResponse>(invoice);
    }

    public async Task<PagedResult<InvoiceListResponse>> GetInvoicesAsync(
        DateTime? startDate,
        DateTime? endDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
            page = 1;

        if (pageSize < 1)
            pageSize = 20;

        if (pageSize > 100)
            pageSize = 100;

        var (invoices, totalCount) = await _invoiceRepository.GetAllAsync(
            startDate,
            endDate,
            page,
            pageSize,
            cancellationToken);

        var invoiceResponses = _mapper.Map<List<InvoiceListResponse>>(invoices);

        return new PagedResult<InvoiceListResponse>(invoiceResponses, page, totalCount, pageSize);
    }

    public async Task<InvoiceResponse> UpdateInvoiceAsync(Guid id, UpdateInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id, cancellationToken);
        if (invoice == null)
        {
            throw new InvoiceNotFoundException(id);
        }

        if (invoice.Status != InvoiceStatus.Draft)
        {
            throw new InvalidInvoiceException($"Cannot update invoice with status {invoice.Status}. Only Draft invoices can be updated.");
        }

        // Update invoice
        invoice.Update(request.IssueDate, request.DueDate, request.Notes);

        // Clear existing items and add new ones
        var existingItemIds = invoice.Items.Select(i => i.Id).ToList();
        foreach (var itemId in existingItemIds)
        {
            invoice.RemoveItem(itemId);
        }

        foreach (var itemDto in request.Items)
        {
            var item = new InvoiceItem(
                itemDto.ProductCode,
                itemDto.Description,
                itemDto.Quantity,
                itemDto.UnitPrice,
                itemDto.TaxRate);

            invoice.AddItem(item);
        }

        // Validate invoice
        invoice.Validate();

        // Begin transaction
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // Update invoice
            await _invoiceRepository.UpdateAsync(invoice, cancellationToken);

            // Create outbox message
            var outboxMessage = CreateOutboxMessage(
                invoice.Id.ToString(),
                "Invoice",
                "invoice.updated",
                new
                {
                    InvoiceId = invoice.Id,
                    InvoiceNumber = invoice.InvoiceNumber,
                    TotalAmount = invoice.TotalAmount,
                    Status = invoice.Status.ToString(),
                    UpdatedAt = invoice.UpdatedAt
                });

            await _outboxRepository.AddAsync(outboxMessage, cancellationToken);

            // Commit transaction
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return _mapper.Map<InvoiceResponse>(invoice);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task DeleteInvoiceAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id, cancellationToken);
        if (invoice == null)
        {
            throw new InvoiceNotFoundException(id);
        }

        // Soft delete
        invoice.SoftDelete();

        // Begin transaction
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // Update invoice
            await _invoiceRepository.UpdateAsync(invoice, cancellationToken);

            // Create outbox message
            var outboxMessage = CreateOutboxMessage(
                invoice.Id.ToString(),
                "Invoice",
                "invoice.deleted",
                new
                {
                    InvoiceId = invoice.Id,
                    InvoiceNumber = invoice.InvoiceNumber,
                    DeletedAt = invoice.UpdatedAt
                });

            await _outboxRepository.AddAsync(outboxMessage, cancellationToken);

            // Commit transaction
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private OutboxMessage CreateOutboxMessage(string aggregateId, string aggregateType, string eventType, object eventData)
    {
        var payload = JsonSerializer.Serialize(eventData);
        return new OutboxMessage(aggregateId, aggregateType, eventType, payload);
    }
}
