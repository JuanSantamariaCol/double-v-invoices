using AutoMapper;
using InvoicesService.Application.DTOs.Requests;
using InvoicesService.Application.DTOs.Responses;
using InvoicesService.Domain.Entities;

namespace InvoicesService.Application.Mappings;

public class InvoiceMappingProfile : Profile
{
    public InvoiceMappingProfile()
    {
        // Invoice mappings
        CreateMap<Invoice, InvoiceResponse>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<Invoice, InvoiceListResponse>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        // InvoiceItem mappings
        CreateMap<InvoiceItem, InvoiceItemResponse>();

        CreateMap<InvoiceItemDto, InvoiceItem>()
            .ConstructUsing(dto => new InvoiceItem(
                dto.ProductCode,
                dto.Description,
                dto.Quantity,
                dto.UnitPrice,
                dto.TaxRate));
    }
}
