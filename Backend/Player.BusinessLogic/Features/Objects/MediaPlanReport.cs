using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;
using Player.Services.Report;
using Player.Services.Report.Abstractions;
using Player.Services.Report.MediaPlan;

namespace Player.BusinessLogic.Features.Objects
{
    public class MediaPlanReport
    {
        public class Handler : IRequestHandler<Query, Response>
        {
            private readonly PlayerContext _context;
            private readonly IReportGenerator<PlaningMediaPlanReportModel> _reportGenerator;

            public Handler(PlayerContext context, IReportGenerator<PlaningMediaPlanReportModel> reportGenerator)
            {
                _context = context;
                _reportGenerator = reportGenerator;
                // Конструктор принимает контекст данных и сервис генерации отчетов, которые инъектируются через механизм внедрения зависимостей.
            }

            public async Task<Response> Handle(Query query, CancellationToken cancellationToken)
            {
                var adHistoryQuery = _context.AdHistories
                                                .Include(ah => ah.Advert)
                                                .ThenInclude(a => a.Organization)
                                                .Include(ah => ah.Object)
                                                .Where(ah => ah.Advert.OrganizationId == query.ClientId)
                                                .Where(ah => ah.Start.Date >= query.FromDate && ah.End <= query.ToDate);

                if (query.AdvertId.HasValue)
                {
                    adHistoryQuery = adHistoryQuery.Where(ah => ah.AdvertId == query.AdvertId);
                }

                if (query.ObjectId.HasValue)
                {
                    adHistoryQuery = adHistoryQuery.Where(ah => ah.ObjectId == query.ObjectId);
                }

                var adHistories = await adHistoryQuery.ToListAsync(cancellationToken);

                if (!adHistories.Any())
                {
                    return new Response();
                }

                var mediaPlanReportModel = new PlaningMediaPlanReportModel
                {
                    Client = new Client
                    {
                        Name = adHistories.First().Advert.Organization.Name
                    },
                    ReportBegin = query.FromDate,
                    ReportEnd = query.ToDate,
                    ObjectHistoryModels = adHistories.GroupBy(ah => ah.Object, ah => ah, (oi, ah) => new ObjectHistoryModel { Object = oi, History = ah })
                };

                var result = await _reportGenerator.Generate(mediaPlanReportModel);

                return new Response
                {
                    FileName = result.FileName,
                    File = result.Report,
                    Type = result.FileType
                };
                // Метод Handle асинхронно обрабатывает запрос. Он извлекает историю рекламных объявлений из базы данных в соответствии с заданными критериями фильтрации (диапазон дат, ID клиента, ID рекламы и объекта). Если записи найдены, формируется модель отчета, которая включает информацию о клиенте, временном диапазоне отчета и сгруппированные данные по истории объектов. Затем сервис генерации отчетов создает отчет и возвращает его в ответе.
            }
        }

        public class Response
        {
            // Response содержит данные сгенерированного отчета: бинарное содержание файла (File), имя файла (FileName) и тип файла (Type).
            public byte[] File { get; set; }
            public string FileName { get; set; }
            public string Type { get; set; }
        }

        public class Query : IRequest<Response>
        {
            // Query представляет собой запрос с параметрами для создания отчета, включая диапазон дат, идентификаторы клиента и рекламного объявления, и тип отчета. Этот класс является входной точкой для операции генерации отчета.
            public Guid? ObjectId { get; set; }
            public DateTime FromDate { get; set; }
            public DateTime ToDate { get; set; }
            public Guid ClientId { get; set; }
            public Guid? AdvertId { get; set; }

            public ReportType ReportType { get; set; }
        }
    }
    // Код обеспечивает возможность генерации и получения отчетов о размещении рекламы для конкретных клиентов и объектов за определенный период времени. Это важно для анализа эффективности рекламных кампаний и планирования будущих медиапланов.
}
