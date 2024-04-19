using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Player.BusinessLogic.Features.Genres;
using Player.Domain;
using Player.DTOs;

namespace Player.WebApi.Controllers.v1.Dictionary
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class GenresController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GenresController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize(Policy = Permission.ReadAllGenres, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ICollection<List.GenreModel>> Get(CancellationToken cancellationToken)
            => await _mediator.Send(new List.Query(), cancellationToken); //Метод Get предназначен для получения списка музыкальных жанров. Он поддерживает пагинацию, что позволяет запросить определенное количество элементов (в данном случае до 10000 элементов на страницу). Проверка прав пользователя на доступ к данным, используя его ID и проверку соответствующих разрешений (RolePermissions и Permissions).Этот метод полезен для получения обзора всех доступных музыкальных жанров, которые могут быть использованы для фильтрации музыкальных треков по жанру в пользовательском интерфейсе или для аналитических целей.
        // Выполнение SQL-запроса для получения данных о жанрах из таблицы Genres.     Get: Этот метод не принимает параметров непосредственно в сигнатуре метода, но использует CancellationToken для обработки отмены запроса. Он задействован для извлечения данных о жанрах из базы данных и возвращает их в виде коллекции.
        // Возвращение данных в формате JSON.

        [HttpPost]
        [Authorize(Policy = Permission.CreateGenre, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Create(SimpleDto genre, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new Create.Command
            {
                Name = genre.Name,
            }, cancellationToken);

            return Ok(result);
        }
    }
}