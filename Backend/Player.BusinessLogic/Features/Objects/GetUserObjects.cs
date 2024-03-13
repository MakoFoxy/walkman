using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;
using Player.Services.Abstractions;
using ObjectModel = Player.BusinessLogic.Features.Objects.Models.ObjectModel;

namespace Player.BusinessLogic.Features.Objects
{
    public class GetUserObjects
    {
        public class Handler : IRequestHandler<Query, Response>
        {
            private readonly PlayerContext _context;
            private readonly IMapper _mapper;
            private readonly IUserManager _userManager;

            public Handler(IUserManager userManager, PlayerContext context, IMapper mapper)
            {
                _userManager = userManager;
                _context = context;
                _mapper = mapper;
            }

            public async Task<Response> Handle(Query query, CancellationToken cancellationToken)
            {
                var user = await _userManager.GetCurrentUser(cancellationToken);
                var currentDate = DateTime.Now.Date;

                var userObjects = await _context.Users.Where(u => u == user)
                    .SelectMany(u => u.Objects)
                    .Select(uo => uo.Object)
                    .Select(info => new ObjectModel
                    {
                        Id = info.Id,
                        Name = info.Name,
                        ActualAddress = info.ActualAddress,
                        BeginTime = info.BeginTime,
                        EndTime = info.EndTime,
                        FirstPerson = null,
                        Loading = info.Playlists.Where(p => p.PlayingDate == currentDate)
                            .Select(p => p.Loading).SingleOrDefault(),
                        UniqueAdvertCount = info.Playlists
                            .Where(p => p.PlayingDate == currentDate)
                            .Select(p => p.UniqueAdvertsCount).SingleOrDefault(),
                        AllAdvertCount = info.Playlists
                            .Where(p => p.PlayingDate == currentDate)
                            .Select(p => p.AdvertsCount).SingleOrDefault(),
                        Overloaded = info.Playlists
                            .Where(p => p.PlayingDate == currentDate)
                            .Select(p => p.Overloaded).SingleOrDefault(),
                        PlaylistExist = info.Playlists.Any(p => p.PlayingDate == currentDate),
                        IsOnline = info.IsOnline,
                    })
                    .ToListAsync(cancellationToken);

                return new Response
                {
                    Objects = userObjects,
                };
                //Асинхронный метод Handle обрабатывает запрос на получение списка объектов пользователя. Он получает данные текущего пользователя, дату и вытаскивает из базы данных информацию об объектах, к которым пользователь имеет доступ. Информация о каждом объекте преобразуется в модель ObjectModel и включает различные параметры, такие как адрес, рабочее время и данные о плейлистах на текущую дату.
                // Процесс получения объектов:
                //     Извлекается информация о текущем пользователе.
                //     Затем выбираются все объекты, связанные с этим пользователем.
                //     Для каждого объекта извлекается дополнительная информация, включая данные о плейлистах для текущей даты.
                //     Результаты компонуются в список моделей ObjectModel.
            }
        }

        public class Query : IRequest<Response>
        {
            //Query представляет собой запрос без дополнительных данных, так как для получения списка объектов текущего пользователя не требуются никакие параметры.
        }

        public class Response
        {
            public List<ObjectModel> Objects { get; set; } = new();
            //Response содержит список объектов в формате ObjectModel, который представляет собой ответ на запрос.
        }
    }
    //В итоге, этот код обеспечивает функциональность для получения и отображения списка объектов, ассоциированных с текущим пользователем, что может быть использовано, например, в пользовательском интерфейсе для отображения списка мест или помещений, которыми управляет пользователь.
}