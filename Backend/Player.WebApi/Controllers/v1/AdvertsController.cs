using System;
using System.Threading;
using System.Threading.Tasks;
using BrunoZell.ModelBinding;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Player.BusinessLogic.Features.Adverts;
using Player.BusinessLogic.Features.Adverts.Models;
using Player.Domain;
using Player.DTOs;

namespace Player.WebApi.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AdvertsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdvertsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Authorize(Policy = Permission.CreateAdvert, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Post([ModelBinder(BinderType = typeof(JsonModelBinder))][FromBody] Create.AdvertData advert, IFormFile advertFile)
        {
            await _mediator.Send(new Create.Command { Advert = advert, AdvertFile = advertFile });
            return Ok();
            //             Путь: /api/v1/adverts
            // Действие: Создание нового объявления.
            // Модель данных в ответе: Используется статус код ответа для указания успешного создания.
            // Ответ: Подтверждение создания рекламного объявления.
        }

        [HttpGet("check-possibility")]
        [Authorize(Policy = Permission.CreateAdvert, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CheckPossibility(
            [FromQuery] CheckPossibility.Query query,
            CancellationToken token
            )
        //             Путь: /api/v1/adverts/check-possibility
        // Действие: Проверка возможности размещения рекламы с учетом заданных параметров.
        // Параметры: advertLength, repeatCount, dateBegin, dateEnd, objects[].
        // Модель данных в ответе: CheckPossibility+Response, включающая статус возможности размещения и связанную информацию.
        // Ответ: Возвращает данные о возможности размещения рекламы на заданные даты и для выбранных объектов.
        {
            var response = await _mediator.Send(query, token);
            return Ok(response);
        }

        [HttpGet]
        [Authorize(Policy = Permission.ReadAllAdverts, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<BaseFilterResult<AdvertModel>> Get([FromQuery] List.AdvertFilterModel model, CancellationToken cancellationToken) => await _mediator.Send(new List.Query { Filter = model }, cancellationToken);
        //     Метод GET:

        // Путь: /api/v1/adverts
        // Действие: Получение списка рекламных объявлений с пагинацией.
        // Параметры: page, itemsPerPage.
        // Модель данных в ответе: BaseFilterResult<AdvertModel>, содержащая пагинированный список рекламных объявлений и связанные данные.
        //         // Ответ: Возвращает список рекламных объявлений в зависимости от заданных фильтров.
        // В данном запросе используется контроллер Player.WebApi.Controllers.v1.AdvertsController с методом Get, который обрабатывает запрос на получение списка рекламных объявлений. Этот метод принимает модель AdvertFilterModel и возвращает результат в виде Player.DTOs.BaseFilterResult<Player.BusinessLogic.Features.Adverts.Models.AdvertModel>. Вот детали о возвращаемой модели данных:
        // Модель данных AdvertModel:

        //     Id: Уникальный идентификатор рекламного объявления.
        //     Name: Название рекламного объявления.
        //     FromDate: Минимальная дата начала показа из всех жизненных циклов данного объявления (AdLifetimes).
        //     ToDate: Максимальная дата окончания показа из всех жизненных циклов.
        //     Objects: Список объектов (Objects), где показывается реклама, каждый объект представлен как SimpleDto с полями Id и Name.
        //     CreateDate: Дата создания объявления.
        //     TotalItems: Общее количество объявлений, соответствующих фильтру.

        // Этот метод использует Entity Framework для выполнения запросов в базе данных и фильтрации объявлений на основе переданных параметров фильтрации, таких как дата окончания показа и актуальность объявления. Возвращаемые данные пагинируются согласно указанным в запросе параметрам page и itemsPerPage.
        [HttpGet("{id:Guid}")]
        [Authorize(Policy = Permission.ReadAdvertById, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] //Проверка разрешений пользователя: Сначала проверяются права доступа пользователя, который инициировал запрос, к определенным функциям или данным.
        public async Task<BaseAdvertModel> Get(Guid id, CancellationToken cancellationToken) => await _mediator.Send(new Details.Query { AdvertId = id }, cancellationToken); //    Этот метод асинхронно обрабатывает запрос на получение деталей рекламного объявления, идентифицируемого по GUID.     Эта модель, вероятно, представляет карточку объявления с расширенными деталями для отображения на фронтенде или в других потребительских интерфейсах.

        //     Запрос к базе данных:
        // Выполняется запрос к таблице Adverts, при этом данные соединяются с таблицей Organizations для получения информации об организации, связанной с объявлением.
        // Для объявления извлекаются также связанные времена жизни (AdLifetimes) и времена показа (AdTimes).
        // Отбирается рекламное объявление по указанному идентификатору.
        // Результаты запроса проецируются на модель CardAdvertModel, которая включает даты начала и окончания действия объявления, количество повторений и другие связанные атрибуты.
    }
}