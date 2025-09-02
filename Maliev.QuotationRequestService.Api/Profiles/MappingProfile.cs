using AutoMapper;
using Maliev.QuotationRequestService.Api.DTOs;
using Maliev.QuotationRequestService.Data.Entities;

namespace Maliev.QuotationRequestService.Api.Profiles
{
    /// <summary>
    /// AutoMapper profile for mapping between DTOs and entities.
    /// </summary>
    public class MappingProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MappingProfile"/> class.
        /// </summary>
        public MappingProfile()
        {
            CreateMap<Request, RequestDto>().ReverseMap();
            CreateMap<CreateRequestRequest, Request>().ReverseMap();
            CreateMap<UpdateRequestRequest, Request>().ReverseMap();

            CreateMap<RequestFile, RequestFileDto>().ReverseMap();
            CreateMap<CreateRequestFileRequest, RequestFile>().ReverseMap();
            CreateMap<UpdateRequestFileRequest, RequestFile>().ReverseMap();
        }
    }
}