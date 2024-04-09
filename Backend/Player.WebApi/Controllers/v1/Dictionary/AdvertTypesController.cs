using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Player.BusinessLogic.Features.AdvertTypes;
using Player.Domain;

namespace Player.WebApi.Controllers.v1.Dictionary
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AdvertTypesController : Controller
    {
        private readonly IMediator _mediator;

        public AdvertTypesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize(Policy = Permission.ReadAllAdvertTypes, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ICollection<List.AdvertTypeModel>> Get(CancellationToken cancellationToken) => await _mediator.Send(new List.Query(), cancellationToken);

        //    Получение запроса:
        // Клиент (например, веб-браузер) отправляет GET-запрос к вашему API по маршруту, определенному атрибутом маршрутизации [Route("api/v{version:apiVersion}/[controller]")]. В вашем случае это может быть что-то вроде http://localhost:5000/api/v1/adverttypes.

        // Маршрутизация в ASP.NET Core:
        // Приложение определяет, что этот запрос должен быть обработан методом Get в AdvertTypesController, благодаря атрибуту маршрутизации [HttpGet].

        // Авторизация:
        // Прежде чем метод Get будет выполнен, ASP.NET Core проверит, что пользователь авторизован и имеет необходимые разрешения, на основе атрибута [Authorize].

        // Обработка запроса:
        // Метод Get делегирует обработку запроса медиатору, вызывая _mediator.Send(new List.Query(), cancellationToken). Это означает отправку сообщения List.Query() медиатору, который затем находит и вызывает соответствующий обработчик.

        // Обработчик запроса Handler:
        // Медиатор находит Handler, который подписан на обработку List.Query. Handler затем асинхронно выполняет запрос к базе данных, используя DbContext для получения данных из таблицы AdvertTypes. AutoMapper используется для проецирования данных из сущностей AdvertType в модели AdvertTypeModel.

        // Возврат данных:
        // Полученный список объектов AdvertTypeModel возвращается из обработчика и, в конечном итоге, из метода действия Get. ASP.NET Core сериализует этот список в JSON и отправляет его обратно клиенту в ответе HTTP.

        // Отображение данных в браузере:
        // Клиент (веб-браузер) получает ответ в формате JSON и может отобразить эти данные пользователям. Обычно для этого используется JavaScript, который парсит полученный JSON и рендерит соответствующие HTML-элементы на странице.
    }
}