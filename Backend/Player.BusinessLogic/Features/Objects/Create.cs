using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Player.BusinessLogic.Features.Objects.Models;
using Player.DataAccess;
using Player.Domain;
using Player.Services.Abstractions;

namespace Player.BusinessLogic.Features.Objects
{
    public class Create
    {
        public class Validator : AbstractValidator<ObjectInfoModel>
        {
            public Validator()
            {
                RuleFor(o => o.BeginTime).NotEqual(TimeSpan.Zero);
                RuleFor(o => o.EndTime).NotEqual(TimeSpan.Zero);
                RuleFor(o => o.Attendance).NotEqual(0);
                RuleFor(o => o.ActualAddress).NotNull().NotEqual(string.Empty);
                RuleFor(o => o.ActivityType).NotNull()
                    .ChildRules(rules => rules.RuleFor(dto => dto.Id).NotEqual(Guid.Empty));
                RuleFor(o => o.Name).NotNull().NotEqual(string.Empty);
                RuleFor(o => o.Area).NotEqual(0);
                RuleFor(o => o.RentersCount).NotEqual(0);
                RuleFor(o => o.Bin).NotNull().NotEqual(string.Empty);
                //Validator является классом для проверки валидности данных модели ObjectInfoModel перед ее обработкой. Используется FluentValidation для определения правил валидации, таких как проверка на ненулевые значения, наличие и корректность различных полей модели.
            }
        }

        public class Handler : IRequestHandler<Command>
        {
            private readonly PlayerContext _context;
            private readonly IUserManager _userManager;

            public Handler(PlayerContext context, IUserManager userManager)
            {
                _context = context;
                _userManager = userManager;
            }

            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                var objectInfoModel = request.Model;

                var permissions = await _userManager.GetCurrentUserPermissions(cancellationToken);

                var objectInfo = new ObjectInfo
                {
                    BeginTime = objectInfoModel.BeginTime,
                    EndTime = objectInfoModel.EndTime,
                    Attendance = objectInfoModel.Attendance,
                    ActualAddress = objectInfoModel.ActualAddress,
                    LegalAddress = objectInfoModel.LegalAddress,
                    ActivityTypeId = objectInfoModel.ActivityType.Id,
                    Name = objectInfoModel.Name,
                    Area = objectInfoModel.Area,
                    RentersCount = objectInfoModel.RentersCount,
                    Bin = objectInfoModel.Bin,
                    CityId = objectInfoModel.City.Id,
                    Priority = objectInfoModel.Priority,
                    ResponsiblePersonOne = new ObjectInfo.ResponsiblePerson
                    {
                        ComplexName = objectInfoModel.ResponsiblePersonOne.ComplexName,
                        Email = objectInfoModel.ResponsiblePersonOne.Email,
                        Phone = objectInfoModel.ResponsiblePersonOne.Phone,
                    },
                    ResponsiblePersonTwo = new ObjectInfo.ResponsiblePerson
                    {
                        ComplexName = objectInfoModel.ResponsiblePersonTwo.ComplexName,
                        Email = objectInfoModel.ResponsiblePersonTwo.Email,
                        Phone = objectInfoModel.ResponsiblePersonTwo.Phone,
                    },
                    FreeDays = objectInfoModel.FreeDays,
                };

                if (permissions.Any(p => p.Code == Permission.PartnerAccessToObject))
                {
                    var user = await _userManager.GetCurrentUser(cancellationToken);
                    user.Objects.Add(new UserObjects
                    {
                        Object = objectInfo,
                        User = user,
                    });
                }

                objectInfo.Selections = request.Model.Selections.Select(s => new ObjectSelection
                {
                    Object = objectInfo,
                    SelectionId = s.Id
                }).ToList();

                var directoryPath = Directory.GetCurrentDirectory() + "/files/images/";
                Directory.CreateDirectory(directoryPath);

                foreach (var img in request.Images)
                {
                    var path = directoryPath + Guid.NewGuid() + Path.GetExtension(img.FileName);

                    await using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        await img.CopyToAsync(fileStream, cancellationToken);
                    }

                    var image = new Image
                    {
                        Path = path
                    };
                    objectInfo.Images.Add(image);
                }

                _context.Objects.Add(objectInfo);

                await _context.SaveChangesAsync(cancellationToken);
                return Unit.Value;
            }
            // Асинхронный метод Handle обрабатывает команду на создание нового объекта, используя предоставленные данные из request.Model и request.Images. Здесь происходит маппинг данных из DTO в доменную сущность ObjectInfo, проверка прав пользователя на добавление объекта, создание директории для изображений, сохранение изображений на диск и добавление сущности в контекст базы данных перед сохранением изменений.
        }

        public class Command : IRequest<Unit>
        {
            public ObjectInfoModel Model { get; set; }
            public IFormFileCollection Images { get; set; }
            //Command содержит данные, необходимые для выполнения операции создания объекта — это DTO ObjectInfoModel и коллекция изображений IFormFileCollection. Этот класс является частью паттерна CQRS и используется для передачи данных в обработчик.
        }
    }

    //Процесс создания объекта:

    //     Сначала проверяются права текущего пользователя на возможность добавления нового объекта.
    //     Затем инициализируется новый экземпляр ObjectInfo с данными из ObjectInfoModel.
    //     В случае наличия прав у пользователя, к его профилю добавляется новый объект.
    //     Для каждой подборки (Selection) объекта создается новая связь ObjectSelection.
    //     Создается директория для хранения изображений объекта, если она еще не существует.
    //     Каждое изображение сохраняется на диск, и его путь добавляется в коллекцию изображений объекта.
    //     Объект добавляется в базу данных и сохраняются изменения.

    // Этот код обеспечивает комплексную обработку создания новых объектов в системе, включая проверку прав, валидацию данных, сохранение связанных изображений и обновление базы данных.

}