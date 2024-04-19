using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;
using Player.DTOs;

namespace Player.BusinessLogic.Features.Genres
{
    public class List
    {
        public class Handler : IRequestHandler<Query, List<GenreModel>>
        {
            private readonly PlayerContext _context;
            private readonly IMapper _mapper;

            public Handler(PlayerContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public Task<List<GenreModel>> Handle(Query request, CancellationToken cancellationToken = default)
            {
                return _context.Genres.ProjectTo<GenreModel>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken);
                //Этот метод асинхронно обрабатывает запрос на получение списка жанров, используя контекст базы данных. ProjectTo<GenreModel> используется для проецирования каждой сущности жанра из базы данных непосредственно в объект GenreModel, что упрощает преобразование и избавляет от необходимости писать много кода для маппинга. Метод ToListAsync асинхронно преобразует результаты запроса в список.
            }
        }

        public class GenreModel : SimpleDto
        {
            //Содержание модели GenreModel:
            // Id (GUID): Уникальный идентификатор жанра.
            // Name (string): Название жанра.
        }

        public class Query : IRequest<List<GenreModel>>
        {
            //Query является классом, который инкапсулирует параметры запроса. В этом случае он пуст, так как для получения списка жанров дополнительные параметры не требуются. Этот класс используется в качестве сигнала для MediatR о том, что необходимо выполнить операцию получения списка жанров.
        }
        //В целом, этот код позволяет получать список жанров из базы данных, автоматически преобразовывая записи из таблицы жанров в объекты DTO (Data Transfer Object), что упрощает передачу данных между слоями приложения.
    }
}
