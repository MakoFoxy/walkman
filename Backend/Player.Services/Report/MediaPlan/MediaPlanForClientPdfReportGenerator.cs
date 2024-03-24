using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Player.Services.Report.Abstractions;
using Wkhtmltopdf.NetCore;

namespace Player.Services.Report.MediaPlan
{
    public class MediaPlanForClientPdfReportGenerator : BaseReportGenerator<ClientMediaPlanPdfReportModel>
    {
        private readonly IGeneratePdf _generatePdf; // Сервис для создания PDF.
        private string _printImgBase64; // Базовое представление изображения для печати в base64.

        public override ReportType ReportType => ReportType.Pdf; // Тип отчета (PDF).

        public MediaPlanForClientPdfReportGenerator(IGeneratePdf generatePdf) // Конструктор, получающий сервис генерации PDF.
        {
            _generatePdf = generatePdf;
        }

        protected override async Task<string> LoadTemplate(ClientMediaPlanPdfReportModel model) // Загрузка HTML-шаблона отчета.
        {
            var bytes = await File.ReadAllBytesAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, "report-templates/print.png")); // Чтение файла изображения.
            _printImgBase64 = Convert.ToBase64String(bytes); // Преобразование изображения в строку base64.
            using var reader = File.OpenText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "report-templates/client-media-plan-template.html")); // Чтение HTML-шаблона.
            return await reader.ReadToEndAsync(); // Возврат содержимого файла.
        }


        protected override Task<byte[]> GenerateAndInsertData(ClientMediaPlanPdfReportModel model) // Генерация данных и вставка их в шаблон.
        {
            Content = Content.Replace("{ObjectNames}", GetObjectNames(model))
                             .Replace("{AdvertNames}", GetAdvertNames(model))
                             .Replace("{CompanyPeriod}", GetCompanyPeriod(model)); // Замена плейсхолдеров на реальные данные.

            var html = Content.Replace("[body]", BuildBody(model)); // Вставка тела отчета в HTML.
            return Task.FromResult(_generatePdf.GetPDF(html)); // Возврат сгенерированного PDF.
        }

        // Методы получения различных частей отчета (названия объектов, названия реклам, период проведения кампании и т.д.).
        // Генерация имени отчета.
        // Дополнительные действия после генерации отчета (здесь ничего не делается, возвращает Task.CompletedTask).


        private static string GetObjectNames(ClientMediaPlanPdfReportModel model)
        {
            // Возвращает строку, содержащую названия объектов, разделенные переносами строк.
            return string.Join(NewLine, model.ObjectHistoryModels.Select(o => $"{o.Object.City.Name} {o.Object.Name}").ToArray());
        }

        private string GetAdvertNames(ClientMediaPlanPdfReportModel model)
        {
            // Возвращает строку, содержащую названия рекламных объявлений, разделенные переносами строк, уникальные для каждого объекта.
            return string.Join(NewLine,
                model.ObjectHistoryModels.SelectMany(o => o.History.Select(h => h.Advert))
                    .DistinctBy(a => a.Id)
                    .Select(a => a.Name)
                    .ToArray());
        }

        private string GetCompanyPeriod(ClientMediaPlanPdfReportModel model)
        {
            // Возвращает периоды действия реклам для каждого объявления или сообщение, если объявление вне периода.
            var lifeTimes = model.ObjectHistoryModels.SelectMany(o => o.History.Select(h => h.Advert))
                .DistinctBy(a => a.Id)
                .Select(a =>
                {
                    var adLifetime = a.AdLifetimes.SingleOrDefault(l => l.DateBegin < model.Date && l.DateEnd >= model.Date);
                    return adLifetime == null ?
                        "Реклама вне периода" :
                        $"{adLifetime.DateBegin:dd.MM.yyyy} - {adLifetime.DateEnd:dd.MM.yyyy}";
                })
                .ToArray();
            return string.Join(NewLine, lifeTimes);
        }

        protected override Task<string> GetReportName(ClientMediaPlanPdfReportModel model)
        {
            // Возвращает имя отчета, основанное на имени клиента и дате.
            return Task.FromResult($"Отчет для {model.Client.Name} за {model.Date:dd.MM.yyyy}");
        }

        protected override Task AfterGenerate(ClientMediaPlanPdfReportModel model, GeneratorResult result)
        {
            // Пустой метод, вызываемый после генерации отчета. Можно использовать для выполнения дополнительных действий.
            return Task.CompletedTask;
        }

        private string BuildBody(ClientMediaPlanPdfReportModel model)
        {
            // Строит HTML-тело отчета, создавая таблицы для каждого объекта с историей рекламных показов.
            var body = new StringBuilder();

            foreach (var historyModel in model.ObjectHistoryModels)
            {
                var rows = new StringBuilder(); //Для каждого объекта (historyModel) в модели отчета создается новый набор строк (rows).
                var i = 1;

                var orderedHistory = historyModel.History.DistinctBy(hm => new { hm.AdvertId, hm.Start })
                    .OrderBy(hm => hm.Start)
                    .ToList();

                foreach (var adHistory in orderedHistory) //История рекламных показов упорядочивается по времени начала и делается уникальной по ID рекламы и времени начала.
                {
                    var repeatCount = orderedHistory.Take(i).Count(h => h.AdvertId == adHistory.AdvertId);

                    rows.Append(string.Format(Row,
                        i,
                        repeatCount,
                        $"{adHistory.Start.TimeOfDay:hh\\:mm\\:ss} - {adHistory.End.TimeOfDay:hh\\:mm\\:ss}",
                        adHistory.Advert.Name));
                    i++;
                    //                 Генерация строк таблицы:

                    // Для каждой уникальной рекламной истории (adHistory) в упорядоченном списке формируется строка таблицы.
                    // В строке указываются порядковый номер, количество повторений рекламы, время начала и окончания, а также название рекламы.
                    // После добавления каждой строки инкрементируется счетчик i.
                }

                var table = string.Format(HtmlTable, rows, historyModel.History.Count(),
                    string.Join(NewLine,
                        historyModel.History.DistinctBy(h => h.AdvertId).Select(h =>
                            $"{h.Advert.Name} {historyModel.History.Count(hi => hi.Advert.Id == h.Advert.Id)} раз")),
                    $"{historyModel.Object.City.Name} {historyModel.Object.Name}");
                body.Append(table);
                //Создается HTML-таблица из сгенерированных строк, общего количества показов истории и уникальных названий реклам.
                // Таблица также включает название и город объекта.
            }

            var sign = string.Format(Sign, _printImgBase64); // Добавляет изображение подписи в отчет.
            body.Append(sign);
            body.Append(BottomText); // Добавляет нижний текст отчета.

            return body.ToString(); //    Вся собранная информация объединяется в одну строку HTML и возвращается как тело отчета.

            //    Этот код отвечает за создание детализированного отчета по показам рекламы для разных объектов, включая подсчет количества показов и периодов показа, а также форматирование данных в удобочитаемую форму.
        }

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
}