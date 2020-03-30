using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VideoManager.Models;

namespace VideoManager
{
    public class AutoMapping : Profile
    {
        public AutoMapping()
        {
            CreateMap<Video, VideoDto>()
                .ForMember(dest => dest.VideoId, opt => opt.MapFrom(src => src.VideoId))
                .ForMember(dest => dest.DurationInSeconds, opt => opt.MapFrom(src => src.DurationInSeconds))
                .ForMember(dest => dest.Length, opt => opt.MapFrom(src => src.EncodedLength))
                .ForMember(dest => dest.OriginalFileName, opt => opt.MapFrom(src => src.OriginalFileName));
        }
    }
}
