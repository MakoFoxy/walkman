using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Player.Services.Report.Abstractions
{
    public abstract class BaseReportGenerator<TModel> : IReportGenerator<TModel> where TModel : IReportModel
    {
        protected abstract Task<string> LoadTemplate(TModel model); //Абстрактный метод для генерации отчета и вставки данных в шаблон, возвращая массив байтов отчета.
        protected abstract Task<byte[]> GenerateAndInsertData(TModel model); //Абстрактный метод для получения имени файла отчета.
        protected abstract Task<string> GetReportName(TModel model); //Абстрактный метод для получения имени файла отчета.
        protected abstract Task AfterGenerate(TModel model, GeneratorResult result); //Абстрактный метод, который вызывается после генерации отчета. Может использоваться для выполнения дополнительных действий после создания отчета.
        protected string Content = string.Empty;

        public abstract ReportType ReportType { get; } //Это абстрактное свойство должно быть реализовано в производных классах и возвращает тип отчета, например PDF или Excel.

        public virtual async Task<GeneratorResult> Generate(TModel model) //Виртуальный метод для генерации отчета. Загружает шаблон, заполняет его, генерирует отчет, выполняет дополнительные действия после генерации и возвращает результат.
        {
            var generatorResult = new GeneratorResult(); // Создается новый экземпляр класса 

            var template = await LoadTemplate(model); // Асинхронно загружается шаблон отчета, который потом будет заполнен данными. Метод LoadTemplate должен быть реализован в производных классах.
            Content = FillTemplate(model, template); //Заполняется шаблон данными из модели model. Метод FillTemplate заполняет шаблон, используя данные из модели, и возвращает строку с готовым содержанием отчета.
            generatorResult.Report = await GenerateAndInsertData(model); //Асинхронно генерируется конечный отчет и вставляются данные в шаблон. Этот метод также должен быть реализован в производных классах.
            await AfterGenerate(model, generatorResult); //Выполняются дополнительные действия после создания отчета, если это необходимо. Этот метод предназначен для переопределения в производных классах.
            generatorResult.FileName = await GetReportName(model); //Получается имя файла отчета. Метод GetReportName должен быть реализован в производных классах и возвращает строку с именем файла.
            generatorResult.FileType = ReportType.ToString().ToLower(); //Устанавливается тип файла отчета. Это значение получается из абстрактного свойства ReportType, реализованного в производных классах.
            return generatorResult; //Возвращается объект GeneratorResult, содержащий всю информацию о сгенерированном отчете, включая сам отчет (в виде массива байтов), имя файла и тип файла.
            //В итоге, этот метод обеспечивает стандартный процесс создания отчета, который включает загрузку шаблона, заполнение его данными, генерацию окончательного отчета и выполнение дополнительных действий после генерации.
        }

        // Определяет метод для форматирования поля в зависимости от его типа и формата.
        private static string FormatField(object propertyValue, string format)
        {
            // Использует оператор switch для обработки разных типов значений.
            return propertyValue switch
            {
                // Если значение - дата, форматирует её в соответствии с указанным форматом.
                DateTime date => date.ToString(format),
                // Если значение - временной интервал, форматирует его в соответствии с указанным форматом.
                TimeSpan time => time.ToString(format),
                // Во всех остальных случаях просто преобразует значение в строку.
                _ => propertyValue.ToString()
            };
        }

        // Заполняет шаблон данными из модели.
        private string FillTemplate(object model, string template)
        {
            // Создаёт регулярное выражение для поиска мест замены в шаблоне.
            var regex = new Regex("\\[(.*?);");
            // Заменяет каждое совпадение в шаблоне соответствующим значением из модели.
            return regex.Replace(template, match =>
            {
                // Извлекает цепочку свойств из совпадения.
                var propertyChain = match.Value;
                // Флаг, указывающий на наличие формата у значения.
                var isFormatted = false;
                // Строка для хранения формата значения.
                var format = "";

                // Проверяет, содержит ли цепочка свойств формат.
                if (propertyChain.Contains(":"))
                {
                    // Извлекает формат из цепочки.
                    var beginIndex = propertyChain.IndexOf(":", StringComparison.Ordinal) + 1;
                    format = propertyChain[beginIndex..^1];
                    // Удаляет формат из цепочки.
                    propertyChain = propertyChain.Replace(format, "");
                    // Устанавливает флаг форматирования.
                    isFormatted = true;
                }

                // Очищает цепочку от специальных символов.
                propertyChain = propertyChain
                    .Replace(";", "")
                    .Replace(":", "")
                    .Replace("[", "")
                    .Replace("]", "");

                // Разбивает цепочку на отдельные свойства.
                var properties = propertyChain.Split('.');

                // Начинает с самого объекта модели.
                var propertyValue = model;

                // Перебирает свойства и извлекает их значения.
                foreach (var property in properties)
                {
                    // Получает значение текущего свойства.
                    propertyValue = GetPropertyValue(propertyValue, property);

                    // Проверяет значение на null.
                    if (propertyValue == null)
                    {
                        // Выбрасывает исключение, если свойство не найдено.
                        throw new ArgumentNullException($"Property {property} is null in {propertyChain}");
                    }
                }

                // Возвращает отформатированные данные.
                var formattedData = FormatField(propertyValue, isFormatted ? format : "");

                return formattedData;
            });
        }

        // Получает значение свойства по имени.
        private object GetPropertyValue(object model, string property)
        {
            // Получает тип объекта модели.
            var type = model.GetType();
            // Ищет свойство по имени.
            var propertyInfo = type.GetProperty(property);

            // Проверяет, существует ли свойство.
            if (propertyInfo == null)
                // Выбрасывает исключение, если свойство не найдено.
                throw new ArgumentException($"Property {property} not exists in type {type.FullName}");

            // Возвращает значение свойства.
            return propertyInfo.GetValue(model);
        }
    }
}