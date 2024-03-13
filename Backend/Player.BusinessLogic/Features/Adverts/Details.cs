using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.BusinessLogic.Features.Adverts.Models;
using Player.DataAccess;
using Player.DTOs;

namespace Player.BusinessLogic.Features.Adverts
{
    public class Details
    {
        public class Handler : IRequestHandler<Query, CardAdvertModel>
        {
            private readonly PlayerContext _context;
            private readonly IMapper _mapper;

            public Handler(PlayerContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<CardAdvertModel> Handle(Query request, CancellationToken cancellationToken)
            {
                var adverts = await _context.Adverts
                    .Select(a => new CardAdvertModel
                    {
                        Id = a.Id,
                        Name = a.Name,
                        FromDate = a.AdLifetimes.Select(al => al.DateBegin).Min(db => (DateTime?)db),
                        ToDate = a.AdLifetimes.Select(al => al.DateEnd).Max(de => (DateTime?)de),
                        Objects = a.AdTimes.Select(at => new SimpleDto
                        {
                            Id = at.Object.Id,
                            Name = at.Object.Name
                        })/*.Distinct()*/,//TODO Поправить
                        CreateDate = a.CreateDate,
                        RepeatCount = a.AdTimes.First().RepeatCount,//TODO Проверить эту логику
                        Client = a.Organization.Name
                    })
                    .SingleAsync(a => a.Id == request.AdvertId, cancellationToken);

                //TODO Костыль надо будет потом поправить
                adverts.Objects = adverts.Objects.DistinctBy(o => o.Id);
                return adverts;
                //Определение асинхронного метода Handle, который обрабатывает запрос на получение деталей рекламы. Используется LINQ-запрос к базе данных для выборки конкретного рекламного объявления по идентификатору, предоставленному в запросе.
                //Этот код отражает часть бизнес-логики приложения, отвечающую за обработку запросов на получение детальной информации о конкретном рекламном объявлении. Обработчик выполняет запрос к базе данных для извлечения всех необходимых данных, преобразует их в модель CardAdvertModel, предназначенную для отображения на фронтенде, и возвращает эту модель.
            }
        }

        public class Query : IRequest<CardAdvertModel>
        {
            public Guid AdvertId { get; set; }
        }
    }
}