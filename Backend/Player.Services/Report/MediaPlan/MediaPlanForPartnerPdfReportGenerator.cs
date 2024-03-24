using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Player.Services.Report.Abstractions;
using Wkhtmltopdf.NetCore;

namespace Player.Services.Report.MediaPlan;
// Определение класса генератора отчетов для партнеров, наследующий базовый генератор отчетов
public class MediaPlanForPartnerPdfReportGenerator : BaseReportGenerator<PartnerMediaPlanPdfReportModel>
{
    private readonly IGeneratePdf _generatePdf; // Сервис для генерации PDF
    private string _printImgBase64; // Базовая строка для изображения подписи
    // Определение типа отчета
    public override ReportType ReportType => ReportType.Pdf;
    // Конструктор с инъекцией сервиса генерации PDF
    public MediaPlanForPartnerPdfReportGenerator(IGeneratePdf generatePdf)
    {
        _generatePdf = generatePdf;
    }
    // Загрузка HTML-шаблона отчета
    protected override async Task<string> LoadTemplate(PartnerMediaPlanPdfReportModel model)
    {
        var bytes = await File.ReadAllBytesAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, "report-templates/print.png"));
        _printImgBase64 = Convert.ToBase64String(bytes); // Преобразование изображения подписи в base64
        using var reader = File.OpenText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "report-templates/partner-media-plan-template.html"));
        return await reader.ReadToEndAsync(); // Чтение и возвращение HTML-шаблона
    }
    // Генерация и вставка данных в HTML-шаблон
    protected override Task<byte[]> GenerateAndInsertData(PartnerMediaPlanPdfReportModel model)
    {
        // Замена плейсхолдеров в контенте HTML-шаблона реальными данными
        Content = Content.Replace("{ObjectNames}", GetObjectNames(model))
            .Replace("{AdvertNames}", GetAdvertNames(model));

        // Замена тела отчета сгенерированным содержимым
        var html = Content.Replace("[body]", BuildBody(model));
        return Task.FromResult(_generatePdf.GetPDF(html)); // Генерация PDF из HTML
    }
    // Получение названий объектов для отчета
    private static string GetObjectNames(PartnerMediaPlanPdfReportModel model)
    {
        // Склеивание имен объектов с именами городов, разделенных новой строкой
        return string.Join(NewLine, model.ObjectHistoryModels.Select(o => $"{o.Object.City.Name} {o.Object.Name}").ToArray());
    }
    // Получение названий реклам для отчета
    private string GetAdvertNames(PartnerMediaPlanPdfReportModel model)
    {
        // Склеивание уникальных названий реклам, разделенных новой строкой
        return string.Join(NewLine,
            model.ObjectHistoryModels.SelectMany(o => o.History.Select(h => h.Advert))
                .DistinctBy(a => a.Id)
                .Select(a => a.Name)
                .ToArray());
    }
    // Генерация названия отчета
    protected override Task<string> GetReportName(PartnerMediaPlanPdfReportModel model)
    {
        // Формирование названия отчета для партнера с указанием даты
        return Task.FromResult($"Отчет для партнера {model.Client.Name} за {model.Date:dd.MM.yyyy}");
    }
    // Последующие действия после генерации отчета (в данном случае не выполняются)
    protected override Task AfterGenerate(PartnerMediaPlanPdfReportModel model, GeneratorResult result)
    {
        return Task.CompletedTask;
    }
    // Создание тела отчета
    private string BuildBody(PartnerMediaPlanPdfReportModel model)
    {
        var body = new StringBuilder(); // Инициализация строки для тела отчета

        // Перебор всех моделей истории объектов
        foreach (var historyModel in model.ObjectHistoryModels)
        {
            var rows = new StringBuilder(); // Строка для хранения строк таблицы
            var i = 1; // Счетчик для номера строки

            // Упорядочивание истории реклам по времени начала и формирование уникального списка истории реклам для текущего объекта
            var orderedHistory = historyModel.History.DistinctBy(hm => new { hm.AdvertId, hm.Start })
                .OrderBy(hm => hm.Start)
                .ToList();
            // Перебор упорядоченной истории реклам
            foreach (var adHistory in orderedHistory)
            {                // Подсчет количества повторов данной рекламы
                var repeatCount = orderedHistory.Take(i).Count(h => h.AdvertId == adHistory.AdvertId);

                // Добавление строки в таблицу с данными текущей рекламы
                rows.Append(string.Format(Row,
                    i,
                    repeatCount,
                    $"{adHistory.Start.TimeOfDay:hh\\:mm\\:ss} - {adHistory.End.TimeOfDay:hh\\:mm\\:ss}",
                    adHistory.Advert.Name));
                i++; // Увеличение счетчика номера строки
            }

            // Формирование таблицы для текущего объекта
            var table = string.Format(HtmlTable, rows, historyModel.History.Count(),
                string.Join(NewLine,
                    historyModel.History.DistinctBy(h => h.AdvertId).Select(h =>
                        $"{h.Advert.Name} {historyModel.History.Count(hi => hi.Advert.Id == h.Advert.Id)} раз")),
                $"{historyModel.Object.City.Name} {historyModel.Object.Name}");
            body.Append(table); // Добавление таблицы в тело отчета
        }

        // Добавление в тело отчета HTML-кода подписи с использованием base64 изображения
        var sign = string.Format(Sign, _printImgBase64);
        body.Append(sign);

        // Добавление в тело отчета заключительного текста
        body.Append(BottomText);

        // Возврат сформированного тела отчета в виде строки
        return body.ToString();
    }
    // Константы для форматирования HTML содержимого отчета
    private const string NewLine = "<br>";
    private const string Row = @"
                                <tr>
                                    <td>{0}</td>
                                    <td>{1}</td>
                                    <td>{2}</td>
                                    <td>{3}</td>
                                </tr>";

    private const string HtmlTable =
        @"<div class=""row page_break"">
                        <div class=""col-1-sm""></div>
                        <div class=""col-10-sm"">
                            <table class=""main_table"">
                                <caption>{3}</caption>
                                <thead>
                                <tr>
                                    <th class=""zero-width"">№</th>
                                    <th class=""zero-width"">Ко-во повторов</th>
                                    <th class=""zero-width"">Время выхода</th>
                                    <th>Название ролика</th>
                                </tr>
                                </thead>
                                <tbody>
                                    {0}
                                </tbody>
                                <tfoot>
                                <tr>
                                    <td colspan=""2"">Общее количество выходов в день {1}</td>
                                    <td colspan=""2"">
                                        {2}
                                    </td>
                                </tr>
                                </tfoot>
                            </table>
                        </div>
                        <div class=""col-1-sm""></div>
                    </div>";

    private const string Sign = @"
                <div class=""row"">
                    <div class=""col-1-sm""></div>
                    <div class=""col-5-sm right"" style=""margin-top: 60px;"">""Исполнитель: ТОО ""ESP-CENTER"" <br> Директор Бондарцев П.М.""</div>
                    <div class=""col-5-sm left""> <img src=""data:image/png;base64, {0}"" alt=""""></div>
                    <div class=""col-1-sm""></div>
                </div>
";

    private const string BottomText = @"
    <div class=""row"">    
        <div class=""col-1-sm""></div>
        <div class=""col-3-sm""><a href=""tel:+7707-401-0101"">+7 (707) 401-01-01</a><br><a href=""tel:+7747-919-1919"">+7 (747) 919-19-19</a></div>
        <div class=""col-1-sm""></div>
        <div class=""col-2-sm""><a href=""mailto:argpavel@mail.ru"">argpavel@mail.ru</a></div>
        <div class=""col-1-sm""></div>
        <div class=""col-2-sm""><a href=""http://www.marketradio.kz"">http://www.marketradio.kz</a></div>
        <div class=""col-1-sm""></div>
    </div>
";
}