using System.Linq;
using AutoMapper;
using Player.Domain;

namespace Player.BusinessLogic.Features.Selections
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Selection, List.SelectionListModel>().ReverseMap();
            CreateMap<Track, Details.TrackModel>().ReverseMap();
            CreateMap<Selection, Details.SelectionModel>()
                .ForMember(model => model.Tracks,
                    config => config.MapFrom(selection => selection.MusicTracks.Select(mt => mt.MusicTrack).ToList()))
                .ReverseMap();
            //TODO поправить
            CreateMap<Selection, DTOs.UpdateSelectionModel>()
                .ForMember(model => model.Tracks,
                    conf =>
                        conf.MapFrom(selection => selection.MusicTracks.Select(mt => mt.MusicTrackId)));

        }
    }
}