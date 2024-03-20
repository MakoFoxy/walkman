using System;
using System.Threading.Tasks;
using Player.DTOs;
using Refit;

namespace Player.Helpers.ApiInterfaces.PublisherApiInterfaces
{
    public interface IObjectApi
    {
        [Post("/api/v1/object/object-info-changed/{id}")] //Атрибут [Post("/api/v1/object/object-info-changed/{id}")] указывает, что метод представляет HTTP POST запрос к ресурсу /api/v1/object/object-info-changed/{id}. {id} является переменной в URL, которая будет заменена на значение id, переданное в метод.
        Task ObjectInfoChanged([AliasAs("id")] Guid id, [Header("Authorization")] string bearerToken); //Параметр ([AliasAs("id")] Guid id) указывает, что Guid, переданный в метод, должен быть вставлен в URL в место {id}.

        //Параметр ([Header("Authorization")] string bearerToken) представляет заголовок авторизации, который должен быть включен в запрос. Значение для этого заголовка обычно представляет собой токен JWT или другой токен для аутентификации и авторизации.
        //Метод возвращает Task, что означает, что метод является асинхронным и возвращает void после завершения, что типично для операций API, не возвращающих данные.
        [Post("/api/v1/object/object-volume-changed/{id}")] //Атрибут [Post("/api/v1/object/object-volume-changed/{id}")] определяет, что метод представляет HTTP POST запрос к ресурсу /api/v1/object/object-volume-changed/{id}.
        Task ObjectVolumeChanged([AliasAs("id")] Guid id, [Body] ObjectVolumeChangedDto volumeData, [Header("Authorization")] string bearerToken);
        //Параметр ([AliasAs("id")] Guid id) аналогично указывает, что Guid, переданный в метод, должен быть использован в URL.
        //Параметр ([Body] ObjectVolumeChangedDto volumeData) указывает, что объект ObjectVolumeChangedDto должен быть сериализован и отправлен в теле запроса. Это используется для передачи данных об изменении уровня громкости.
        //Параметр ([Header("Authorization")] string bearerToken) также представляет собой заголовок авторизации для запроса.
        //Также возвращает Task, указывая на асинхронность метода.
    }
}
//Общие замечания:

// Использование Refit позволяет значительно упростить создание клиентов API в .NET, автоматически преобразуя интерфейс в HTTP-клиент.
// Атрибуты, такие как [Post], [Get], [Header], [Body], и [AliasAs], позволяют настроить запросы HTTP и их содержимое прямо в коде, делая интерфейс более читаемым и легким для поддержки.
// Методы, определенные в IObjectApi, предназначены для асинхронного взаимодействия с внешним API, что является хорошей практикой для сетевых вызовов, т.к. это предотвращает блокировку основного потока выполнения приложения.