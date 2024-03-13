using System;
using System.Linq;
using AutoMapper;
using Player.BusinessLogic.Features.Adverts.Models;
using Player.Domain;
using Player.DTOs;

namespace Player.BusinessLogic.Features.Adverts
{
    public class AutoMapperProfile : Profile
    //Это пользовательский класс, наследуемый от класса Profile из AutoMapper, где вы определяете конфигурацию сопоставления между различными типами объектов.
    {
        public AutoMapperProfile()
        {
            CreateMap<Advert, AdvertModel>()
                .ForMember(a => a.FromDate,
                    expression =>
                        expression.MapFrom(a => a.AdLifetimes.Select(al => al.DateBegin).Min(db => (DateTime?)db)))
                //FromDate: Минимальная дата начала из всех временных промежутков (AdLifetimes) рекламного объявления используется как значение FromDate в AdvertModel.
                .ForMember(a => a.ToDate,
                    expression =>
                        expression.MapFrom(a => a.AdLifetimes.Select(al => al.DateEnd).Max(de => (DateTime?)de)))
                //ToDate: Максимальная дата окончания из всех временных промежутков (AdLifetimes) рекламного объявления используется как значение ToDate в AdvertModel.
                .ForMember(a => a.Objects,
                    expression => expression.MapFrom(a => a.AdTimes.Select(at => new SimpleDto
                    {
                        Id = at.Object.Id,
                        Name = at.Object.Name
                    }
                    ) /*.Distinct()*/)) //TODO Поправить

                //Objects: Создается коллекция SimpleDto, состоящая из ID и имени каждого объекта (Object), ассоциированного с временами (AdTimes) рекламного объявления. В исходном коде присутствует комментарий //TODO Поправить, что указывает на необходимость дальнейшей корректировки этого сопоставления, возможно, для исключения дубликатов.
                .ForMember(a => a.Length,
                    expression => expression.MapFrom(a => a.Length.TotalSeconds));

            //Length: Длина рекламного объявления (Length) преобразуется из TimeSpan в общее количество секунд.
        }
    }
}

//Эта конфигурация автоматического маппинга позволяет автоматизировать и упростить процесс преобразования данных между слоями приложения, уменьшая количество шаблонного кода и потенциальные ошибки, связанные с ручным маппингом.