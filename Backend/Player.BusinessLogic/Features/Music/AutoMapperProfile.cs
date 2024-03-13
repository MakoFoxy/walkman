using System.Linq;
using AutoMapper;
using Player.Domain;
using Player.DTOs;

namespace Player.BusinessLogic.Features.Music
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<MusicTrack, List.MusicModel>().ForMember(mt => mt.Genres,
                conf => conf.MapFrom(track =>
                    track.Genres.Select(g => g.Genre)
                        .Select(g => new SimpleDto
                        {
                            Id = g.Id,
                            Name = g.Name
                        })));
        }

        //В конструкторе определено правило маппинга от доменной модели MusicTrack к модели передачи данных List.MusicModel.

        // CreateMap<MusicTrack, List.MusicModel>() указывает AutoMapper, что нужно создать правило маппинга между типами MusicTrack и List.MusicModel.

        // .ForMember(mt => mt.Genres, conf => conf.MapFrom(track => ...)) определяет настройку маппинга для конкретного свойства Genres в целевой модели (List.MusicModel). Вместо автоматического маппинга, используется пользовательская функция для преобразования данных:
        //     track.Genres.Select(g => g.Genre).Select(g => new SimpleDto { Id = g.Id, Name = g.Name }) преобразует коллекцию жанров из доменной модели MusicTrack в коллекцию DTO (SimpleDto). Это делается путем выбора (через LINQ) каждого жанра, ассоциированного с треком, и преобразования его в SimpleDto, который содержит только Id и Name жанра.
    }
    //В целом, эта настройка AutoMapper используется для упрощения процесса преобразования сложных доменных моделей в более простые модели для передачи данных между различными слоями приложения, что является распространенной практикой в слое бизнес-логики приложений, работающих с данными.
}