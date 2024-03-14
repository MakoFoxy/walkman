using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace Player.BusinessLogic.Features.SoftwareClient
{
    public class LatestClientExe
    {
        public class Handler : IRequestHandler<Query, Response>
        //Этот класс обрабатывает запросы типа Query и возвращает ответ типа Response. В конструкторе класса Handler инжектируется IConfiguration, который предоставляет доступ к настройкам приложения, в данном случае к пути директории с клиентскими исполняемыми файлами.
        {
            private readonly IConfiguration _configuration;

            public Handler(IConfiguration configuration)
            {
                _configuration = configuration;
            }

            public async Task<Response> Handle(Query query, CancellationToken cancellationToken)
            {
                var clientDirectory = _configuration.GetValue<string>("Player:ClientDirectory");

                var files = Directory.GetFiles(clientDirectory, "*.exe");

                return files
                    .Where(f => Regex.IsMatch(f, "\\d+\\.\\d+\\.\\d+"))
                    .Select(f => new Response
                    {
                        Version = Version.Parse(Regex.Match(f, "\\d+\\.\\d+\\.\\d+").Value),
                        File = Path.GetFileName(f),
                    })
                    .OrderByDescending(v => v.Version)
                    .First();
                //В этом асинхронном методе извлекается путь к директории с клиентскими версиями из конфигурации, а затем ищутся все исполняемые файлы (*.exe) в этой директории. Среди найденных файлов выбираются те, имена которых содержат версию в формате Major.Minor.Build (по шаблону \\d+\\.\\d+\\.\\d+). Затем выбирается файл с самой последней версией.
            }
        }

        public class Query : IRequest<Response>
        {
            //Query — это пустой класс запроса, который используется для активации операции получения последнего исполняемого файла клиента. Поскольку он не содержит входных данных, он просто активирует процесс поиска.
        }

        public class Response
        {
            public Version Version { get; set; }
            public string File { get; set; }
            //Response — это класс ответа, который содержит информацию о версии найденного файла (Version) и его имени (File).
        }
    }
    //     Применение и функциональность:

    //     Этот код может быть использован в сценариях, где клиентское программное обеспечение регулярно обновляется, и требуется автоматически распространять последние версии среди пользователей или систем.
    //     Используя этот код, система может автоматически предоставлять клиентам ссылки для скачивания последних версий программного обеспечения или исполняемых файлов.

    // Это решение упрощает процесс обновления клиентского программного обеспечения, автоматизируя определение последней доступной версии и предоставляя актуальную информацию пользователям или системам.
}