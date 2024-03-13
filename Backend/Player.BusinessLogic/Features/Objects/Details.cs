using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.BusinessLogic.Features.Objects.Models;
using Player.DataAccess;
using Player.DTOs;

namespace Player.BusinessLogic.Features.Objects
{
    public class Details
    {
        public class Handler : IRequestHandler<Query, ObjectInfoModel>
        {
            private readonly PlayerContext _context;
            private readonly IMapper _mapper;

            public Handler(PlayerContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public Task<ObjectInfoModel> Handle(Query request, CancellationToken cancellationToken)
            {
                return _context.Objects.Where(o => o.Id == request.Id)
                    .Select(o => new ObjectInfoModel
                    {
                        Area = o.Area,
                        Attendance = o.Attendance,
                        Bin = o.Bin,
                        City = new SimpleDto
                        {
                            Id = o.City.Id,
                            Name = o.City.Name
                        },
                        Id = o.Id,
                        Name = o.Name,
                        ActivityType = new SimpleDto
                        {
                            Id = o.ActivityType.Id,
                            Name = o.ActivityType.Name,
                        },
                        BeginTime = o.BeginTime,
                        EndTime = o.EndTime,
                        ActualAddress = o.ActualAddress,
                        RentersCount = o.RentersCount,
                        Priority = o.Priority,
                        IsOnline = o.IsOnline,
                        ResponsiblePersonOne = new ObjectInfoModel.ResponsiblePersonModel
                        {
                            ComplexName = o.ResponsiblePersonOne.ComplexName,
                            Email = o.ResponsiblePersonOne.Email,
                            Phone = o.ResponsiblePersonOne.Phone,
                        },
                        ResponsiblePersonTwo = new ObjectInfoModel.ResponsiblePersonModel
                        {
                            ComplexName = o.ResponsiblePersonTwo.ComplexName,
                            Email = o.ResponsiblePersonTwo.Email,
                            Phone = o.ResponsiblePersonTwo.Phone,
                        },
                        FreeDays = o.FreeDays,
                        Selections = o.Selections.Select(so => new SimpleDto
                        {
                            Id = so.Selection.Id,
                            Name = so.Selection.Name
                        }).ToList(),
                        MaxAdvertBlockInSeconds = o.MaxAdvertBlockInSeconds,
                    })
                    .SingleAsync(cancellationToken);
            }

            //             Метод Handle асинхронно обрабатывает запрос на получение деталей объекта по его идентификатору. Он извлекает данные из базы данных и проектирует их на модель ObjectInfoModel, используя LINQ и операции выборки. Метод возвращает задачу, результатом которой является заполненная модель ObjectInfoModel.
            // Процесс запроса:
            //     Осуществляется поиск объекта по Id, переданному в запросе.
            //     Для найденного объекта заполняются все необходимые поля модели ObjectInfoModel, включая информацию о городе, типе деятельности, времени работы, адресе, количестве арендаторов, приоритете и других параметрах.
            //     Используются навигационные свойства для заполнения связанных данных, таких как город и тип деятельности, а также отвечающие лица и подборки (Selections).
        }

        public class Query : IRequest<ObjectInfoModel>
        {
            public Guid Id { get; set; }
            //Query является DTO (Data Transfer Object), который передает данные в обработчик. В этом случае он содержит только идентификатор Id, который используется для поиска конкретного объекта в базе данных.
        }


    }
    //     ObjectInfoModel и SimpleDto являются моделями, которые используются для передачи данных от сервера к клиенту. Они содержат поля, соответствующие данным объекта, и используются для структурирования ответа на запрос.

    // В итоге, этот код позволяет получать детальную информацию об объекте по его идентификатору, что может быть использовано в различных частях системы, например, в пользовательском интерфейсе для отображения информации о конкретном объекте
}