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
        public async Task<List.MusicFilterResult> Get([FromQuery]List.MusicFilterModel model, CancellationToken cancellationToken) => await _mediator.Send(new List.Query{Filter = model}, cancellationToken);

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
        public async Task<FileStreamResult> Get([FromQuery]Download.Query model, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(model, cancellationToken);
            return File(response.TrackStream, $"audio/{response.TrackType}", response.TrackName);
            //Этот метод позволяет скачать музыкальный трек. В текущей реализации закомментировано требование авторизации, что указывается в комментарии TODO как необходимость добавления.
        }

        [HttpPost]
        [Authorize(Policy = Permission.CreateMusic, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Post([FromForm]AddMusicModel model, CancellationToken cancellationToken)
        {
            var genre = JsonConvert.DeserializeObject<SimpleDto>(Request.Form["genre"]);
            var musicTrackIdRaw = Request.Form["musicTrackId"];

            Guid? musicTrackId = null;

            if (musicTrackIdRaw.Count == 1)
            {
                musicTrackId = Guid.Parse(musicTrackIdRaw[0].Replace("\"", string.Empty));
            }
            
            var files = model.MusicFiles.Files.Select(musicFile => new MusicFileModel
            {
                Name = musicFile.FileName,
                Stream = musicFile.OpenReadStream(),
                Genre = genre,
                MusicTrackId = musicTrackId,
            }).ToList();

            await _mediator.Send(new CreateList.Command {SongStreams = files}, cancellationToken);
            return Ok();
            //Этот метод принимает музыкальные файлы, жанр и идентификатор трека, и добавляет новые музыкальные треки. Используется для создания новых записей музыки в системе. Требует наличия разрешения CreateMusic.
        }
    }
}