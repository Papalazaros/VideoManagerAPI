using AutoMapper;
using VideoManager.Models;
using VideoManager.Models.DTO;

namespace VideoManager
{
    public class AutoMapping : Profile
    {
        public AutoMapping()
        {
            CreateMap<Video, PostVideoDTO>()
                .ForMember(dest => dest.VideoId, opt => opt.MapFrom(src => src.VideoId))
                .ForMember(dest => dest.OriginalFileName, opt => opt.MapFrom(src => src.OriginalFileName))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));
        }
    }
}
