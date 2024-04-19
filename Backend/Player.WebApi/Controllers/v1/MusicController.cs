using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Player.BusinessLogic.Features.Music;
using Player.BusinessLogic.Features.Songs;
using Player.Domain;
using Player.DTOs;

namespace Player.WebApi.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class MusicController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MusicController(IMediator mediator)
        {
            _mediator = mediator;
            //Конструктор принимает экземпляр IMediator и использует его для отправки запросов и команд через MediatR.
        }

        [HttpGet]
        [Authorize(Policy = Permission.ReadAllMusic, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        //Этот метод возвращает отфильтрованный список музыкальных треков на основе параметров в модели MusicFilterModel. Он требует, чтобы пользователь имел разрешение ReadAllMusic.
        public async Task<List.MusicFilterResult> Get([FromQuery] List.MusicFilterModel model, CancellationToken cancellationToken) => await _mediator.Send(new List.Query { Filter = model }, cancellationToken); //Описание: Получение списка музыкальных треков с фильтрацией. Возвращает: Player.BusinessLogic.Features.Music.List+MusicFilterResult, результаты фильтрации музыкальных треков, включая детали треков и жанры. Метод Get возвращает список музыкальных треков с фильтрацией по страницам и другим критериям, таким как жанр и подборка. Возвращаемая модель MusicFilterResult включает в себя детали каждого трека, такие как ID, имя, длина, путь к файлу и связанные жанры.

        [HttpGet("all")]
        [Authorize(Policy = Permission.ReadAllMusic, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ICollection<List.MusicModel>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new List.Query
            {
                Filter = new List.MusicFilterModel()
            }, cancellationToken);

            return result.Result;
            //Этот метод возвращает список всех музыкальных треков без фильтрации. Также требуется разрешение ReadAllMusic.
        }

        [HttpGet("download")]
        //TODO добавить авторизацию
        // [Authorize(Policy = Permission.DownloadMusic, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<FileStreamResult> Get([FromQuery] Download.Query model, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(model, cancellationToken);
            return File(response.TrackStream, $"audio/{response.TrackType}", response.TrackName);
            //Этот метод позволяет скачать музыкальный трек. В текущей реализации закомментировано требование авторизации, что указывается в комментарии TODO как необходимость добавления.
        }

        [HttpPost]
        [Authorize(Policy = Permission.CreateMusic, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Post([FromForm] AddMusicModel model, CancellationToken cancellationToken)
        { //Метод Post предназначен для загрузки и добавления музыкального трека в систему. Для загрузки используется мультипарт форма, что позволяет передавать файлы и данные в одном запросе. Проверка прав доступа пользователя на выполнение операции, используя его ID и проверку соответствующих разрешений.
          // Обработка загруженных данных и файла музыкального трека.
          // В случае успешной обработки, информация о треке добавляется в базу данных.
          //    Post: Этот метод принимает объект AddMusicModel и CancellationToken как параметры. AddMusicModel содержит данные, необходимые для добавления новой музыки, включая, возможно, метаданные и файл музыки. Основная задача этого метода — обработать загруженные данные и сохранить их в базе данных.
            var genre = JsonConvert.DeserializeObject<SimpleDto>(Request.Form["genre"]); //Десериализация данных жанра: Используя JsonConvert.DeserializeObject<SimpleDto>, код десериализует JSON, содержащийся в форме запроса под ключом "genre", в объект SimpleDto. Этот объект представляет жанр музыкальной дорожки и используется для последующей обработки.
            var musicTrackIdRaw = Request.Form["musicTrackId"]; //Извлечение и обработка идентификатора музыкальной дорожки: Код извлекает значение musicTrackId из формы запроса, которое представляет собой идентификатор музыкальной дорожки. Если в форме присутствует только одно значение для musicTrackId, код очищает его от кавычек и преобразует в Guid (глобальный уникальный идентификатор).

            Guid? musicTrackId = null; //Инициализация musicTrackId: Сначала объявляется переменная musicTrackId типа Guid?, что означает, что она может хранить значение Guid или быть null. Переменная инициализируется значением null.

            if (musicTrackIdRaw.Count == 1) //Проверка условия: Затем код проверяет, содержит ли musicTrackIdRaw ровно один элемент с помощью условия musicTrackIdRaw.Count == 1. Это условие гарантирует, что действия будут выполнены только если в musicTrackIdRaw находится точно один идентификатор.
            {
                musicTrackId = Guid.Parse(musicTrackIdRaw[0].Replace("\"", string.Empty));
                //Парсинг идентификатора: Если условие истинно, то код берет первый элемент массива (musicTrackIdRaw[0]), удаляет из него кавычки (что делает Replace("\"", string.Empty)), и затем преобразует полученную строку в Guid с помощью Guid.Parse(). Результат преобразования присваивается переменной musicTrackId.
            }

            var files = model.MusicFiles.Files.Select(musicFile => new MusicFileModel
            { //Обработка загруженных файлов: Затем код перебирает файлы, отправленные через форму, используя model.MusicFiles.Files. Для каждого файла он создает объект MusicFileModel, который включает:
                Name = musicFile.FileName,
                Stream = musicFile.OpenReadStream(),
                Genre = genre,
                MusicTrackId = musicTrackId,
                //                 Name: Имя файла.
                // Stream: Поток данных файла, который открывается для чтения.
                // Genre: Объект жанра, полученный ранее.
                // MusicTrackId: Идентификатор музыкальной дорожки, если он доступен.
            }).ToList(); //Все созданные объекты MusicFileModel собираются в список.

            await _mediator.Send(new CreateList.Command { SongStreams = files }, cancellationToken); //Отправка команды через Mediator: После создания списка файлов, этот список упаковывается в команду CreateList.Command, которая отправляется для обработки через шину команд Mediator. Это асинхронный вызов, который передает команду в систему для обработки, например, для сохранения файлов в базу данных или файловую систему.
            return Ok(); //Возвращение результата: По завершении обработки команды, метод возвращает HTTP статус 200 OK, что означает успешное выполнение запроса.
            //Этот метод принимает музыкальные файлы, жанр и идентификатор трека, и добавляет новые музыкальные треки. Используется для создания новых записей музыки в системе. Требует наличия разрешения CreateMusic.
        }
    }
}