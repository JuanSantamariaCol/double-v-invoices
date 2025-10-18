using AutoMapper;
using FluentAssertions;
using InvoicesService.Application.DTOs.Requests;
using InvoicesService.Application.Interfaces;
using InvoicesService.Application.Mappings;
using InvoicesService.Application.Services;
using InvoicesService.Domain.Entities;
using InvoicesService.Domain.Exceptions;
using InvoicesService.Domain.Interfaces;
using Moq;

namespace InvoicesService.Application.Tests.Services;

public class InvoiceServiceTests
{
    private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock;
    private readonly Mock<IOutboxRepository> _outboxRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICustomerValidationService> _customerValidationServiceMock;
    private readonly IMapper _mapper;
    private readonly InvoiceService _invoiceService;

    public InvoiceServiceTests()
    {
        _invoiceRepositoryMock = new Mock<IInvoiceRepository>();
        _outboxRepositoryMock = new Mock<IOutboxRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _customerValidationServiceMock = new Mock<ICustomerValidationService>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<InvoiceMappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        _invoiceService = new InvoiceService(
            _invoiceRepositoryMock.Object,
            _outboxRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _customerValidationServiceMock.Object,
            _mapper);
    }

    [Fact]
    public async Task CreateInvoiceAsync_WithValidCustomer_ShouldCreateInvoice()
    {
        // Arrange
        var request = new CreateInvoiceRequest
        {
            CustomerId = "CUST-001",
            IssueDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Notes = "Test invoice",
            Items = new List<InvoiceItemDto>
            {
                new InvoiceItemDto
                {
                    ProductCode = "PROD-001",
                    Description = "Test Product",
                    Quantity = 10,
                    UnitPrice = 100,
                    TaxRate = 0.19m
                }
            }
        };

        var customerInfo = new CustomerInfo
        {
            Id = "CUST-001",
            Name = "Test Customer",
            Identification = "123456789"
        };

        _customerValidationServiceMock
            .Setup(x => x.GetCustomerInfoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(customerInfo);

        _invoiceRepositoryMock
            .Setup(x => x.GenerateInvoiceNumberAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("INV-2025-000001");

        _invoiceRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Invoice inv, CancellationToken ct) => inv);

        // Act
        var result = await _invoiceService.CreateInvoiceAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.InvoiceNumber.Should().Be("INV-2025-000001");
        result.CustomerId.Should().Be("CUST-001");
        result.CustomerName.Should().Be("Test Customer");
        result.Items.Should().HaveCount(1);
        result.TotalAmount.Should().Be(1190);

        _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateInvoiceAsync_WithInvalidCustomer_ShouldThrowInvalidCustomerException()
    {
        // Arrange
        var request = new CreateInvoiceRequest
        {
            CustomerId = "INVALID-CUST",
            IssueDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Items = new List<InvoiceItemDto>
            {
                new InvoiceItemDto
                {
                    ProductCode = "PROD-001",
                    Description = "Test Product",
                    Quantity = 1,
                    UnitPrice = 100,
                    TaxRate = 0.19m
                }
            }
        };

        _customerValidationServiceMock
            .Setup(x => x.GetCustomerInfoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerInfo?)null);

        // Act
        var act = async () => await _invoiceService.CreateInvoiceAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidCustomerException>()
            .WithMessage("*INVALID-CUST*");

        _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetInvoiceByIdAsync_WithNonExistingId_ShouldThrowInvoiceNotFoundException()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();

        _invoiceRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Invoice?)null);

        // Act
        var act = async () => await _invoiceService.GetInvoiceByIdAsync(invoiceId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvoiceNotFoundException>();
    }
}
