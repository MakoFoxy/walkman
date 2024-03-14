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
    public class Edit
    {
        public class Handler : IRequestHandler<Command>
        {
            private readonly PlayerContext _context;
            private readonly ITelegramMessageSender _telegramMessageSender;
            private readonly IMapper _mapper;

            public Handler(PlayerContext context, ITelegramMessageSender telegramMessageSender, IMapper mapper)
            {
                _context = context;
                _telegramMessageSender = telegramMessageSender;
                _mapper = mapper;
                //Конструктор класса Handler, принимает контекст базы данных, сервис для отправки сообщений в Telegram и маппер для преобразования объектов.
            }

            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                var model = request.Model;
                var selection = await _context.Selections
                    .Include(s => s.MusicTracks)
                    .SingleAsync(s => s.Id == model.Id, cancellationToken);
                selection.Name = model.Name;
                selection.DateBegin = model.DateBegin;
                selection.DateEnd = model.DateEnd;
                selection.IsPublic = request.Model.IsPublic;
                selection.MusicTracks.Clear();

                for (var i = 0; i < model.Tracks.Count; i++)
                {
                    var musicTrackSelection = new MusicTrackSelection
                    {
                        Index = i,
                        MusicTrackId = model.Tracks[i],
                        Selection = selection,
                    };

                    selection.MusicTracks.Add(musicTrackSelection);
                }

                await _context.SaveChangesAsync(cancellationToken);
                return Unit.Value;

                //                 Асинхронно обрабатывает команду на редактирование подборки. Метод загружает подборку из базы данных по ID, обновляет её поля в соответствии с данными из request.Model, очищает текущий список музыкальных треков и добавляет новые, указанные в модели команды. После изменений данные сохраняются в базе.
                // Классы Command и UpdateSelectionModel:

                //     Command - класс, представляющий собой команду, содержащую модель для обновления UpdateSelectionModel.
                //     UpdateSelectionModel - модель данных, используемая для обновления информации о подборке. Содержит название, даты начала и окончания, статус публичности и список треков.

                // Изменение подборки:

                //     Поля Name, DateBegin, DateEnd и IsPublic подборки обновляются согласно данным из модели.
                //     Список треков в подборке (MusicTracks) очищается и заново заполняется треками из команды. Для каждого трека создается связь с подборкой (MusicTrackSelection), где Index задаёт порядок трека в подборке.
            }
        }

        public class Command : IRequest<Unit>
        {
            public UpdateSelectionModel Model { get; set; }
        }
    }
    //После выполнения метода Handle изменения подборки сохраняются в базе данных, и операция редактирования считается завершённой. Это позволяет пользователям системы обновлять существующие музыкальные подборки, меняя их состав и свойства.
}