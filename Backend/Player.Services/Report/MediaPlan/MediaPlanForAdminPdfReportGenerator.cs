using System; // Подключает базовые системные классы.
using System.IO; // Позволяет работать с файлами и потоками данных.
using System.Linq; // Предоставляет классы и методы для запросов, работающих с объектами.
using System.Text; // Позволяет работать с текстом в кодировке Unicode.
using System.Threading.Tasks; // Обеспечивает поддержку асинхронных операций.
using Player.Services.Report.Abstractions; // Подключает абстракции служб отчетов.
using Wkhtmltopdf.NetCore; // Включает функциональность для конвертации HTML в PDF.

namespace Player.Services.Report.MediaPlan // Объявляет пространство имен.
{
    public class MediaPlanForAdminPdfReportGenerator : BaseReportGenerator<AdminMediaPlanPdfReportModel> // Объявляет класс генератора PDF-отчетов для администраторов, наследуемый от BaseReportGenerator.
    {
        private readonly IGeneratePdf _generatePdf; // Зависимость для генерации PDF из HTML.

        public override ReportType ReportType => ReportType.Pdf; // Переопределяет тип отчета как PDF.

        public MediaPlanForAdminPdfReportGenerator(IGeneratePdf generatePdf) // Конструктор класса.
        {
            _generatePdf = generatePdf; // Инициализация сервиса для генерации PDF.
        }

        protected override async Task<string> LoadTemplate(AdminMediaPlanPdfReportModel model) // Загружает HTML-шаблон отчета из файла.
        {
            using var reader = File.OpenText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "report-templates/admin-media-plan-template.html"));
            return await reader.ReadToEndAsync(); // Считывает шаблон полностью.
        }

        protected override Task<byte[]> GenerateAndInsertData(AdminMediaPlanPdfReportModel model) // Вставляет данные в шаблон и генерирует PDF.
        {
            var html = Content.Replace("[body]", BuildBody(model)); // Заменяет место для тела в HTML-шаблоне на сгенерированные данные.
            return Task.FromResult(_generatePdf.GetPDF(html)); // Генерирует PDF из HTML.
        }

        protected override Task<string> GetReportName(AdminMediaPlanPdfReportModel model) // Генерирует имя для отчета.
        {
            return Task.FromResult($"Админ {model.Object.Name} {DateTime.Today.AddDays(-1):ddMMyy}"); // Возвращает имя файла отчета.
        }

        protected override Task AfterGenerate(AdminMediaPlanPdfReportModel model, GeneratorResult result) // Метод для действий после генерации отчета.
        {
            return Task.CompletedTask; // В данном случае действий после генерации не предусмотрено.
        }

        private string BuildBody(AdminMediaPlanPdfReportModel model) // Построение тела отчета.
        {
            var rows = new StringBuilder(); // Создает новый экземпляр StringBuilder для создания строки HTML.
            var i = 1; // Начальный индекс для нумерации строк в отчете.

            foreach (var track in model.Tracks.All) // Проходит по всем трекам в модели отчета.
            {
                // Добавляет HTML-код для каждой строки трека в таблице отчета.
                rows.Append("<tr>" +
                            $"<td>{i++}</td>" +
                            $"<td>{model.Tracks.All.Take(i).Count(t => t.Id == track.Id)}</td>" +
                            $"<td>{track.BeginTime:hh\\:mm\\:ss}</td>" +
                            $"<td>{track.Name}</td>" +
                            $"<td>{Math.Round(track.Length.TotalSeconds)}</td>" +
                            "</tr>");
            }

            return rows.ToString(); // Возвращает сформированную строку HTML для вставки в тело отчета.
        }
    }
    //В этом коде формируется отчет для администратора в формате PDF. Отчет включает в себя информацию о треках: порядковый номер, количество воспроизведений, время начала, название трека и его продолжительность. Вся информация о треках собирается в таблицу HTML.

    // Код начинается с загрузки HTML-шаблона отчета из файла. Шаблон содержит место, где будет вставлено тело отчета ([body]). Затем происходит генерация данных и их вставка в шаблон. Данные генерируются в методе BuildBody, который создает HTML-строки для каждого трека в отчете. Эти строки добавляются в StringBuilder, который затем преобразуется в строку и вставляется в HTML-шаблон вместо [body].

    // Далее, сгенерированный HTML-код передается в сервис IGeneratePdf, который преобразует HTML в PDF. Имя файла отчета генерируется динамически на основе данных отчета, включая название объекта и дату.

    // После генерации отчета метод AfterGenerate может быть использован для выполнения дополнительных действий, хотя в данном случае он просто возвращает Task.CompletedTask, поскольку дополнительные действия не требуются.

    // Этот код показывает, как можно автоматизировать создание и распространение отчетов в формате PDF в административной части приложения или системы, обеспечивая удобный доступ к отчетности за определенные периоды.

}
