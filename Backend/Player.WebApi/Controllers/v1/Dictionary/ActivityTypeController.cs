using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Player.BusinessLogic.Features.ActivityTypes;
using Player.Domain;

namespace Player.WebApi.Controllers.v1.Dictionary
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    //    [ApiController]: Указывает, что класс является контроллером API, что включает определенные поведения по умолчанию (например, автоматическую обработку ошибок запросов).
    // [ApiVersion("1.0")]: Определяет версию API, которую обслуживает данный контроллер.
    // [Route("api/v{version:apiVersion}/[controller]")]: Устанавливает маршрут к API, где {version:apiVersion} извлекается из адреса запроса, а [controller] автоматически заменяется на имя контроллера.
    public class ActivityTypeController : ControllerBase
    //Это контроллер в вашем Web API, предназначенный для работы с данными, связанными с типами активностей. Он наследуется от ControllerBase, что предоставляет ему набор базовых функциональностей для работы с HTTP-запросами.
    {
        private readonly IMediator _mediator;

        public ActivityTypeController(IMediator mediator)
        {
            _mediator = mediator;
            //Принимает интерфейс IMediator, который используется для отправки запросов (или команд) через MediatR. Это позволяет контроллеру быть слабосвязанным с логикой бизнес-процессов и службами.
        }

        [HttpGet]
        //    Декорируется атрибутами [HttpGet] для обработки HTTP GET запросов и [Authorize] для применения политик авторизации, используя JWT (JSON Web Token) для аутентификации. Policy = Permission.ReadAllActivityTypes означает, что пользователь должен иметь разрешение на чтение всех типов активностей, чтобы вызывать этот метод.
        // Возвращает коллекцию моделей типов активностей. Асинхронно отправляет запрос через MediatR, используя созданный экземпляр запроса List.Query(), и передает токен отмены cancellationToken, который может быть использован для отмены операции, если HTTP-запрос отменяется.
        [Authorize(Policy = Permission.ReadAllActivityTypes, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ICollection<List.ActivityTypeModel>> Get(CancellationToken cancellationToken) => await _mediator.Send(new List.Query(), cancellationToken);

        //Это должен быть класс, наследующийся от интерфейса запроса MediatR, определенный где-то в вашем проекте в пространстве имен Player.BusinessLogic.Features.ActivityTypes. Этот класс не показан в представленном коде, но он должен определять параметры и данные для выполнения запроса на получение списка типов активностей. Возвращаемое значение: Возвращает коллекцию объектов ActivityTypeModel, включающую идентификаторы и названия типов активностей.ActivityTypeController - контроллер, который обрабатывает запросы к типам деятельности (/api/v1/activity-type). Возвращает список типов деятельности, доступных в системе.
        //Когда метод Get в вашем контроллере AdvertTypesController возвращает результат работы обработчика, который является Task<ICollection<List.AdvertTypeModel>>, этот результат асинхронно ожидается с использованием await. После того как задача выполнена и данные получены, ASP.NET Core использует свой системный сериализатор (по умолчанию это System.Text.Json для .NET Core 3.0 и выше) для преобразования возвращаемого ICollection<List.AdvertTypeModel> в JSON-формат перед отправкой данных клиенту в теле HTTP-ответа.  Описание: Получение списка всех типов активностей. Возвращает список типов активностей с их идентификаторами и названиями.
        //  Возвращаемое значение: Возвращается HTTP 200 с JSON содержащим список типов активностей.
    }
    //Таким образом, этот контроллер демонстрирует, как можно реализовать простой endpoint API для чтения данных с использованием паттернов CQRS и MediatR в ASP.NET Core приложении.
}