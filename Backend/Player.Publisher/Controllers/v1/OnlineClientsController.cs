using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Player.BusinessLogic.Features.SoftwareClient;

namespace Player.Publisher.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    //    [ApiController]: Этот атрибут указывает, что класс OnlineClientsController является контроллером API. Это включает некоторые предварительные настройки, например, автоматическую обработку запросов на основе моделей и другие конвенции, специфичные для API.

    // [ApiVersion("1.0")]: Указывает версию API, которой принадлежит контроллер. В данном случае это версия 1.0.

    // [Route("api/v{version:apiVersion}/[controller]")]: Определяет шаблон маршрута для всех действий в контроллере. [controller] будет заменено на имя контроллера без слова "Controller", в данном случае OnlineClients. {version:apiVersion} позволяет использовать версионирование в маршруте.
    public class OnlineClientsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OnlineClientsController(IMediator mediator)
        {
            _mediator = mediator;
            //    public OnlineClientsController(IMediator mediator): Это конструктор класса, который принимает объект IMediator. IMediator используется в паттерне MediatR для отправки запросов и команд. Это инъекция зависимости, обеспечивающая разделение ответственности и упрощение тестирования.
        }
        
        [HttpGet]
        //[HttpGet]: Атрибут, применяемый к методу действия, который указывает, что данный метод отвечает на HTTP GET запросы. То есть, когда кто-то делает GET запрос к указанному выше маршруту, будет вызван этот метод.
        public async Task<List<OnlineList.OnlineClient>> Get(CancellationToken cancellationToken) => await _mediator.Send(new OnlineList.Query(), cancellationToken);
        //public async Task<List<OnlineList.OnlineClient>> Get(CancellationToken cancellationToken): Это метод действия, который возвращает список онлайн клиентов. Он асинхронный (async), что позволяет обрабатывать запросы эффективно без блокировки потока. CancellationToken позволяет отменить операцию, если запрос был отменен со стороны клиента. Возвращаемый тип — Task<List<OnlineList.OnlineClient>>, что означает асинхронное выполнение с возвращением списка клиентов.
        //await _mediator.Send(new OnlineList.Query(), cancellationToken): Здесь используется MediatR для отправки запроса OnlineList.Query(). MediatR обращается к соответствующему обработчику, который извлекает и возвращает список онлайн клиентов. await используется для ожидания завершения асинхронной операции, прежде чем продолжить выполнение. cancellationToken передается в операцию для обработки возможности отмены запроса.
    }
}
//В целом, OnlineClientsController обеспечивает API для получения информации об онлайн клиентах, используя архитектурный подход CQRS с помощью MediatR и асинхронное программирование для эффективной обработки запросов.