using AutoMapper;
using Player.Domain;
using Player.DTOs;

namespace Player.BusinessLogic.Features.ActivityTypes
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<ActivityType, SimpleDto>().ReverseMap();//TODO зачем этот маппинг
            //Эта строка создает сопоставление между типом ActivityType и SimpleDto. ReverseMap() позволяет выполнять сопоставление в обоих направлениях: от ActivityType к SimpleDto и обратно. Это может быть полезно для простых операций CRUD, когда вы хотите преобразовать DTO обратно в сущность домена для сохранения в базе данных.
            CreateMap<ActivityType, List.ActivityTypeModel>().ReverseMap();
        }
    }
}

//    ActivityType → ListActivityTypeModel: Это сопоставление используется для преобразования объекта ActivityType в ListActivityTypeModel. Это полезно, когда вы извлекаете данные из базы данных и хотите отправить их клиенту. В этом случае данные сущности ActivityType преобразуются в формат DTO ListActivityTypeModel, который затем может быть сериализован и отправлен как часть ответа API. Этот процесс часто используется для скрытия некоторых деталей сущности или для того, чтобы данные были представлены в формате, более подходящем для клиентского приложения.

// ListActivityTypeModel → ActivityType: Обратное сопоставление позволяет вам принимать DTO ListActivityTypeModel от клиента (например, из тела POST или PUT запроса) и преобразовывать его обратно в сущность ActivityType. Это полезно для операций обновления или создания, где клиент отправляет данные, которые нужно сохранить в базе данных. В этом случае DTO служит "посредником", который передает данные от клиента к вашей сущности домена.