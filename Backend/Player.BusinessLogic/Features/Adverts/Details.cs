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
            {//В приведенном коде асинхронный метод Handle обрабатывает запрос на получение деталей рекламного объявления. Он использует Entity Framework для извлечения данных из базы данных и формирования модели CardAdvertModel, которая включает информацию о рекламе. Вот подробное описание того, что делает каждый блок кода:
                var adverts = await _context.Adverts //Запрос выбирает рекламные объявления из таблицы Adverts в базе данных.
                    .Select(a => new CardAdvertModel //Используется метод Select, чтобы преобразовать данные в модель CardAdvertModel.
                    {
                        Id = a.Id,
                        Name = a.Name,
                        FromDate = a.AdLifetimes.Select(al => al.DateBegin).Min(db => (DateTime?)db),
                        ToDate = a.AdLifetimes.Select(al => al.DateEnd).Max(de => (DateTime?)de), //FromDate и ToDate — минимальная и максимальная даты из связанных жизненных циклов объявления (AdLifetimes), соответственно.
                        Objects = a.AdTimes.Select(at => new SimpleDto
                        {//Objects — список объектов, где показывалась реклама, формируется из связанных времен показа (AdTimes). Каждый объект преобразуется в SimpleDto, содержащий ID и имя объекта.
                            Id = at.Object.Id,
                            Name = at.Object.Name
                        })/*.Distinct()*/,//TODO Поправить
                        CreateDate = a.CreateDate,
                        RepeatCount = a.AdTimes.First().RepeatCount,//TODO Проверить эту логику RepeatCount — количество повторений объявления, берётся из первой записи времён показа.
                        Client = a.Organization.Name //Client — имя организации клиента, связанной с объявлением.
                    })
                    .SingleAsync(a => a.Id == request.AdvertId, cancellationToken);

                //TODO Костыль надо будет потом поправить
                adverts.Objects = adverts.Objects.DistinctBy(o => o.Id); //    Используется метод DistinctBy для удаления дубликатов в списке объектов по идентификатору, что позволяет устранить возможные повторения в данных.
                return adverts; //    Возвращается объект CardAdvertModel с полной информацией о рекламном объявлении.
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