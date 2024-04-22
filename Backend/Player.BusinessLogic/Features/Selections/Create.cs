using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;
using Player.Domain;
using Player.Services.Abstractions;
using UpdateSelectionModel = Player.BusinessLogic.Features.Selections.Models.UpdateSelectionModel;

namespace Player.BusinessLogic.Features.Selections
{
    public class Create
    {
        public class Handler : IRequestHandler<Command>
        {
            private readonly PlayerContext _context;
            private readonly IMapper _mapper;
            private readonly ITelegramMessageSender _telegramMessageSender;
            private readonly IUserManager _userManager;

            public Handler(
                PlayerContext context,
                IMapper mapper,
                ITelegramMessageSender telegramMessageSender,
                IUserManager userManager)
            {
                _context = context;
                _mapper = mapper;
                _telegramMessageSender = telegramMessageSender;
                _userManager = userManager;
            }
            //Этот метод Handle класса обработчика команды отвечает за создание новой подборки (селекции) музыкальных треков в системе. Он выполняет несколько ключевых шагов в процессе создания подборки:
            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {//    Прежде чем создать новую подборку, метод проверяет, существует ли уже подборка с таким же названием, приводя название к нижнему регистру для регистронезависимого сравнения. Если такая подборка найдена, метод генерирует исключение, тем самым предотвращая создание дубликатов.
                //TODO Вынести в валидацию
                if (await _context.Selections.AnyAsync(s => s.Name.ToLower() == request.Model.Name.ToLower()))
                {             //     Перед созданием новой подборки происходит проверка на её уникальность по имени в рамках системы.

                    throw new Exception("Selection exists");
                }

                var user = await _userManager.GetCurrentUser(cancellationToken);
                //    Система определяет организацию, к которой принадлежит текущий пользователь. Это делается путём запроса к базе данных для поиска организации, где клиенты включают текущего пользователя. Эта информация используется для связывания новой подборки с организацией пользователя.     Метод _userManager.GetCurrentUser(cancellationToken) используется для получения информации о текущем пользователе системы. Это нужно для связывания создаваемой подборки с данными пользователя, например, его организацией.
                var organization = await _context.Organizations     //Осуществляется запрос к контексту базы данных _context.Organizations, чтобы найти организацию, в которой состоит текущий пользователь. Это делается через проверку, что пользователь является клиентом организации.
                    .Where(o => o.Clients.Select(c => c.User).Contains(user))
                    .SingleOrDefaultAsync(cancellationToken); // Создается связь подборки с организацией текущего пользователя.


                var model = request.Model; //UpdateSelectionModel 
                //    На основе данных из запроса (request.Model) создается новый объект Selection, который включает в себя название подборки, даты начала и окончания действия, признак публичности и привязку к организации пользователя.

                var selection = new Selection //    Создается новый объект Selection с параметрами из request.Model, включая имя, даты начала и окончания, признак публичности и связь с организацией пользователя.
                {
                    Name = model.Name,
                    DateBegin = request.Model.DateBegin,
                    DateEnd = request.Model.DateEnd,
                    Organization = organization,
                    IsPublic = request.Model.IsPublic,
                };

                for (var i = 0; i < model.Tracks.Count; i++) //    В цикле для каждого идентификатора трека из списка model.Tracks создается связь между подборкой и музыкальным треком через объект MusicTrackSelection. Это включает индексацию треков и установление связи с подборкой.
                {//    Каждый трек из списка в запросе добавляется в подборку. Для каждого трека создается объект MusicTrackSelection, который связывает трек с подборкой и устанавливает его порядковый номер (Index) в подборке.
                    var musicTrackSelection = new MusicTrackSelection
                    {
                        Index = i,
                        MusicTrackId = model.Tracks[i],
                        Selection = selection,
                    };

                    selection.MusicTracks.Add(musicTrackSelection);
                }

                if (model.ObjectId.HasValue)
                {//    Если в запросе указан идентификатор объекта (ObjectId), создается дополнительная связь ObjectSelection, которая связывает подборку с данным объектом.
                    selection.Objects.Add(new ObjectSelection
                    {
                        ObjectId = model.ObjectId.Value,
                        Selection = selection
                    });
                }

                _context.Selections.Add(selection); //    Подготовленная подборка добавляется в контекст базы данных, после чего происходит сохранение всех изменений асинхронно. Это включает запись всех новых объектов и связей в соответствующие таблицы базы данных.
                await _context.SaveChangesAsync(cancellationToken);
                return Unit.Value;
                //Этот метод асинхронно обрабатывает команду на создание новой подборки. Сначала происходит проверка на существование подборки с таким же именем. Затем определяется организация текущего пользователя. Далее создается новый объект Selection с информацией из request.Model, включая название подборки, даты начала и окончания, принадлежность к организации и публичный статус. После этого добавляются музыкальные треки, указанные в команде, и связь с объектом, если таковая указана. В конце подборка добавляется в базу данных, и изменения сохраняются.
            }

            //             Особенности реализации:

            //     Перед созданием новой подборки происходит проверка на её уникальность по имени в рамках системы.
            //     Создается связь подборки с организацией текущего пользователя.
            //     В подборку добавляются музыкальные треки с сохранением их порядка в подборке.
            //     Если в команде указан объект, с которым должна быть связана подборка, создается соответствующая связь.
            //     Предполагается, что метод может быть расширен для отправки уведомлений через Telegram при успешном создании подборки, хотя в текущем коде это не реализовано.

            // Код обеспечивает возможность создания подборок музыкальных треков, которые могут использоваться в различных  объектах системы, например, для воспроизведения в определенных местах или на мероприятиях.
        }

        public class Command : IRequest<Unit>
        {
            public UpdateSelectionModel Model { get; set; }
        }
    }
}

// Да, ваше описание корректно отражает процесс создания подборки в базе данных. Вот подробное разъяснение шагов, происходящих в методе:

//     Модель запроса (model):
//         Модель model получается из request.Model и содержит данные, необходимые для создания новой подборки. Это включает имя подборки, даты начала и окончания, список идентификаторов треков, идентификатор объекта (если присутствует), и признак публичности подборки.

//     Создание объекта Selection:
//         Создается объект Selection, который будет представлять новую подборку в базе данных. В этот объект вносятся все необходимые данные, включая связь с организацией пользователя, что определяется через запрос к базе данных.

//     Добавление треков в подборку:
//         Для каждого идентификатора трека в списке model.Tracks создается объект MusicTrackSelection. Этот объект связывает конкретный трек с создаваемой подборкой и сохраняет порядковый номер трека в подборке (индекс).

//     Связь подборки с объектом:
//         Если указан ObjectId, создается связь между подборкой и объектом через ObjectSelection. Это позволяет ассоциировать подборку с конкретным объектом (например, местоположением или событием).

//     Сохранение подборки в базе данных:
//         Готовый объект Selection с включенными в него треками и возможной связью с объектом добавляется в контекст базы данных. Затем выполняется SaveChangesAsync, который асинхронно сохраняет все изменения в базе данных, фиксируя новую подборку и связанные с ней данные.