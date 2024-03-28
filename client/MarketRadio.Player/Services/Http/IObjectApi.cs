using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarketRadio.Player.DataAccess.Domain;
using Player.ClientIntegration.Base;
using Refit;
using Object = MarketRadio.Player.DataAccess.Domain.Object;

namespace MarketRadio.Player.Services.Http
//Код представляет собой определение интерфейса IObjectApi для работы с HTTP API в рамках .NET приложения, использующего библиотеку Refit. Refit – это библиотека типа REST клиента для .NET, которая позволяет автоматически превращать интерфейс C# в live HTTP-сервис с использованием аннотаций для описания запросов. Вот основные моменты реализации IObjectApi:
{
    public interface IObjectApi
    {
        [Get("/api/v1/current-user/objects")]
        Task<UserObjectsResponse> GetAll([Header("Authorization")] string token);
        //GetAll метод: Делает GET-запрос к эндпоинту /api/v1/current-user/objects для получения объектов текущего пользователя. В качестве параметра принимает токен авторизации, который передаётся в заголовке Authorization.

        [Get("/api/v1/object/{id}")]
        Task<ObjectInfo> GetFullInfo([AliasAs("id")] Guid objectId); //GetFullInfo метод: Делает GET-запрос к эндпоинту /api/v1/object/{id}, чтобы получить полную информацию об объекте по его идентификатору objectId. Использует атрибут AliasAs для связи параметра метода с переменной в пути URL.

        [Get("/api/v1/client/{id}/settings")] //GetSettings метод: Выполняет GET-запрос к /api/v1/client/{id}/settings для получения настроек объекта по его идентификатору objectId, также с использованием атрибута AliasAs для указания параметра в URL.
        Task<string> GetSettings([AliasAs("id")] Guid objectId);
    }

    public class UserObjectsResponse
    //Этот класс представляет собой модель ответа для метода GetAll. Он содержит коллекцию объектов SimpleModel, которые представляют объекты, доступные текущему пользователю. Конкретная структура SimpleModel не показана, но ожидается, что она предоставляет базовые данные об объектах, такие как идентификатор, имя или другие характеристики.
    {
        public ICollection<SimpleModel> Objects { get; set; } = new List<SimpleModel>();
    }
    //Этот интерфейс может быть использован в .NET приложении для абстрагирования взаимодействия с внешним HTTP API, управляющим объектами пользователя. При помощи DI (Dependency Injection) экземпляр клиента IObjectApi, сконфигурированный с помощью Refit, может быть инжектирован в сервисы или контроллеры, где потребуется доступ к данным об объектах пользователя. Это облегчает разработку, тестирование и поддержку кода, делая взаимодействие с HTTP API более типизированным и безопасным.

}