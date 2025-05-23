﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BrunoZell.ModelBinding;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Player.BusinessLogic.Features.Adverts;
using Player.BusinessLogic.Features.Objects;
using Player.BusinessLogic.Features.Objects.Models;
using Player.Domain;
using Player.DTOs;
using Create = Player.BusinessLogic.Features.Objects.Create;
using Details = Player.BusinessLogic.Features.Objects.Details;
using List = Player.BusinessLogic.Features.Objects.List;
using ObjectModel = Player.BusinessLogic.Features.Objects.Models.ObjectModel;

namespace Player.WebApi.Controllers.v1.Dictionary
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ObjectController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ObjectController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize(Policy = Permission.ReadAllObjects, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<BaseFilterResult<ObjectModel>> Get([FromQuery] List.ObjectFilterModel model, CancellationToken cancellationToken) => await _mediator.Send(new List.Query { Filter = model }, cancellationToken); //фильтрация используется для поиска объектов в базе данных по имени объекта, а также предоставляет возможность фильтрации объектов по их онлайн статусу и применения других критериев фильтрации, указанных в ObjectFilterModel. //Описание: Метод выполняет запрос к базе данных, используя фильтры из ObjectFilterModel, включая дату, наличие в сети и имя объекта. Возвращает подробную информацию об объектах включая ID, название, физический адрес, времена начала и окончания деятельности, загрузку по плейлисту, уникальное и общее количество реклам, признак перегрузки и статус наличия в сети. Возвращаемое значение: При успешном выполнении возвращает данные о объектах в виде BaseFilterResult содержащем список моделей ObjectModel и общее количество объектов. Возвращаемые данные включают статус ответа HTTP 200 и JSON с данными об объектах. //http://localhost:8082/home#objects Обрабатывает запросы на получение списка всех объектов с учётом определённых фильтров, таких как дата, название, и статус онлайн.  Подготовка и выполнение запроса: Строится SQL запрос для извлечения данных об объектах, удовлетворяющих критериям фильтрации. Происходит фильтрация по имени, дате, и статусу онлайн. В запросе также учитываются данные о плейлистах объекта.

        //Возвращаемое значение: Возвращает BaseFilterResult1[ObjectModel], включающий отфильтрованный список объектов, общее количество элементов и информацию для пагинации (страницы, количество элементов на странице). Каждый ObjectModel` может включать детали как идентификатор, наименование, адреса, времена начала и окончания работы, загрузку и другие атрибуты, связанные с плейлистами на заданную дату.
        //Метод: Get(ObjectFilterModel, CancellationToken)
        // Действие: Этот метод обрабатывает GET-запрос для получения списка объектов с фильтрацией по определенным критериям, таким как дата, наличие в онлайне, и т.д. В результате запроса база данных фильтрует объекты, используя предоставленные параметры фильтрации.
        [HttpGet("user")]
        [Authorize(Policy = Permission.ReadAllObjects, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<GetUserObjects.Response> GetUserObjects(CancellationToken cancellationToken) => await _mediator.Send(new GetUserObjects.Query(), cancellationToken);
        //Метод GetUserObjects вызывается для получения объектов, ассоциированных с пользователем. Это происходит по маршруту /api/v1/object/user.Описание:Ответ включает данные объектов, такие как идентификаторы, имена, начальное и конечное время, и другие детали в формате  //GetUserObjects.Response содержит список объектов, каждый из которых является экземпляром ObjectModel. Вот подробное описание того, как это работает в контексте вашего API: GetUserObjects.Response — это класс, предназначенный для хранения и передачи результатов запроса на получение объектов, связанных с пользователем. Он включает в себя список объектов типа ObjectModel, каждый из которых представляет собой конкретный объект с атрибутами и данными, такими как загрузка, количество реклам и другие детали. Возвращает: Объект GetUserObjects+Response, который, скорее всего, содержит список объектов, ассоциированных с пользователем.
        [HttpGet("all")]
        //TODO Логику авторизации надо добавить на клиент
        //[Authorize(Policy = Permission.ReadAllObjectsForDropDown, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<List<SimpleListAll.ObjectDto>> GetAll(CancellationToken cancellationToken) => await _mediator.Send(new SimpleListAll.Query(), cancellationToken); //Описание: Метод асинхронно возвращает список всех объектов в системе. Возвращаемый список содержит объекты ObjectDto, включающие информацию о каждом объекте, такую как Id, Name и Priority.

        //    Возвращает список всех объектов, упорядоченный по имени объекта. Каждый объект в списке содержит идентификатор, имя и приоритет. Ответ также сопровождается статусом HTTP 200.
        //    Метод GetAll: Этот метод возвращает список всех объектов, упорядоченных по имени. Возвращаемый список содержит элементы типа ObjectDto, включающие ID, имя и приоритет каждого объекта. Метод также асинхронный и использует Entity Framework Core для выполнения запроса.

        [HttpGet("{id:Guid}")]
        //TODO Логику авторизации надо добавить на клиент
        // [Authorize(Policy = Permission.ReadObjectById, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ObjectInfoModel> Get(Guid id, CancellationToken cancellationToken) => await _mediator.Send(new Details.Query { Id = id }, cancellationToken); //Описание: Получает детали конкретного объекта по его идентификатору. Возвращаемое значение: Возвращает ObjectInfoModel, содержащий подробную информацию об объекте, включая область, посещаемость, бизнес-информацию, идентификаторы связанных сущностей (города, типа активности), адреса, времена начала и окончания работы, и другие характеристики.

        [HttpGet("{objectId:Guid}/{date}/adverts")]
        public async Task<ListInObjectOnSelectedDate.Response> Get([FromRoute] Guid objectId, [FromRoute] DateTime date,
            CancellationToken cancellationToken) => await _mediator.Send(
            new ListInObjectOnSelectedDate.Query { Date = date, ObjectId = objectId }, cancellationToken);

        [HttpPut]
        [Authorize(Policy = Permission.EditObject, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Put([FromBody] Edit.Command command, CancellationToken cancellationToken)
        {//Действие: Этот метод обрабатывает PUT-запрос для обновления данных объекта в базе данных. Он получает идентификатор объекта, а также новые данные для обновления (например, тип деятельности, город, количество свободных дней и т.д.). Результат: Метод возвращает статус 200, указывая на успешное выполнение запроса без возвращения конкретного содержимого (возвращается только статусный код).
            await _mediator.Send(command, cancellationToken);
            return Ok();
        }

        [HttpPost]
        [Authorize(Policy = Permission.CreateObject, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Post([ModelBinder(BinderType = typeof(JsonModelBinder))]
            [FromBody]ObjectInfoModel objectInfoModel, IFormFileCollection images, CancellationToken cancellationToken)
        {
            await _mediator.Send(new Create.Command { Model = objectInfoModel, Images = images }, cancellationToken);
            return Ok();
            //UserObjects и ObjectSelection записываються данные
            //Описание: Создание нового объекта с переданными данными и файлами. Принимает данные объекта и коллекцию файлов.
            // Возвращаемое значение: Возвращается HTTP 200, подтверждая успешное создание объекта. Описание: Метод предназначен для создания нового объекта в системе. Пользователь отправляет данные в формате multipart/form-data, которые могут включать файлы и данные объекта.
        }
    }
}