using System.Linq;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Player.Helpers
{
    public class SwaggerDefaultValues : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {

            var apiDescription = context.ApiDescription;

            operation.Deprecated = apiDescription.IsDeprecated();

            if (operation.Parameters == null)
            {
                return;
            }

            // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/412
            // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/pull/413
            foreach (var parameter in operation.Parameters)
            {
                var description = context.ApiDescription.ParameterDescriptions.First(p => p.Name == parameter.Name);
                var routeInfo = description.RouteInfo;

                if (string.IsNullOrEmpty(parameter.Name))
                {
                    parameter.Name = description.ModelMetadata?.Name;
                }

                if (parameter.Description == null)
                {
                    parameter.Description = description.ModelMetadata?.Description;
                }

                if (routeInfo == null)
                {
                    continue;
                }

                parameter.Required |= !routeInfo.IsOptional;
            }
        }
    }
}

// Apply

// Метод Apply вызывается для каждой операции (например, GET, POST и т.д.) в вашем API.

//     Проверка устаревания API:
//         operation.Deprecated = apiDescription.IsDeprecated(); устанавливает свойство Deprecated операции на основании того, помечена ли версия API как устаревшая. Это информирует пользователей Swagger UI о том, что конкретная операция или версия API больше не рекомендуется к использованию.

//     Обработка параметров:
//         Проходит по всем параметрам операции. Для каждого параметра он пытается найти соответствующее описание параметра в контексте текущего API.
//         Обновляет название параметра и описание на основе метаданных модели, если они не были автоматически установлены Swagger.

//     Требования к параметрам:
//         Устанавливает параметр как обязательный, если он не помечен как опциональный в маршруте (routeInfo.IsOptional). Это помогает обеспечить точность документации API, указывая, какие параметры являются обязательными для каждой операции.

// Комментарии REF

// В коде есть ссылки на GitHub issues и pull requests (REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/412), которые, вероятно, были источником вдохновения или решения конкретной проблемы при создании этого фильтра. Эти ссылки указывают на обсуждения, связанные с проблемами в документации Swashbuckle, которые этот фильтр стремится решить.
// Общее впечатление

// Этот фильтр помогает улучшить качество и точность сгенерированной документации Swagger, обеспечивая, чтобы все параметры были правильно документированы и помечены в соответствии с их требованиями и метаданными. Использование такого фильтра полезно для обеспечения полноты и точности документации вашего API, что делает её более полезной для разработчиков, использующих ваш API.