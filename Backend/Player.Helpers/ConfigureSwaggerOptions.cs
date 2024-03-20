using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Player.Helpers
{
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;
        private readonly string _apiName;

        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider, string apiName)
        {
            _provider = provider;
            _apiName = apiName;
        }

        public void Configure(SwaggerGenOptions options)
        {
            options.CustomSchemaIds(t => t.FullName);
            foreach (var description in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
            }
            //             Метод Configure вызывается при настройке опций SwaggerGen через DI контейнер.
            // В методе options.CustomSchemaIds(t => t.FullName); указывается, что для каждой схемы в Swagger должен использоваться полный путь класса (что помогает избежать конфликтов имен, если в разных пространствах имен есть классы с одинаковыми именами).
            // В цикле foreach (var description in _provider.ApiVersionDescriptions) происходит перебор всех версий API, предоставленных IApiVersionDescriptionProvider.
            // Для каждой версии API создается документация Swagger (SwaggerDoc), используя description.GroupName в качестве имени группы и созданный объект OpenApiInfo для описания документации.
        }

        private OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
        {
            var info = new OpenApiInfo
            {
                Title = _apiName,
                Version = description.ApiVersion.ToString(),
                Description = "",
            };

            if (description.IsDeprecated)
            {
                info.Description += " This API version has been deprecated.";
            }

            return info;
            //    Этот приватный метод создает и возвращает объект OpenApiInfo, который содержит информацию о версии API.
            // В поле Title устанавливается имя API, переданное в конструкторе класса.
            // Версия API берется из объекта description (description.ApiVersion.ToString()).
            // Если версия API помечена как устаревшая (description.IsDeprecated), к описанию добавляется предупреждение об устаревании.
        }
    }
}

// Применение:

// Чтобы использовать эту настройку в проекте ASP.NET Core, вы должны добавить его в сервисы в методе ConfigureServices файла Startup.cs следующим образом:

// csharp

// services.AddVersionedApiExplorer(options => /* настройки */);
// services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>(serviceProvider =>
// {
//     var provider = serviceProvider.GetRequiredService<IApiVersionDescriptionProvider>();
//     return new ConfigureSwaggerOptions(provider, "Название вашего API");
// });
// services.AddSwaggerGen();

// Этот код добавит поддержку версионирования и автоматическую генерацию Swagger документации для каждой версии API в вашем проекте.
// Цель использования:

// Использование этого кода в вашем ASP.NET Core проекте позволяет вам:

//     Автоматически генерировать и обновлять документацию Swagger для всех версий вашего API.
//     Обеспечивать точность и актуальность документации без необходимости ручного обновления.
//     Предоставлять пользователям API четкую и понятную документацию с возможностью переключения между версиями API.
//     Избегать раскрытия чувствительной информации в документации благодаря фильтрации параметров конфигурации.