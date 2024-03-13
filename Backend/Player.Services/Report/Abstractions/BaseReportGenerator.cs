using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Player.Services.Report.Abstractions
{
    public abstract class BaseReportGenerator<TModel> : IReportGenerator<TModel> where TModel : IReportModel
    {
        protected abstract Task<string> LoadTemplate(TModel model);
        protected abstract Task<byte[]> GenerateAndInsertData(TModel model);
        protected abstract Task<string> GetReportName(TModel model);
        protected abstract Task AfterGenerate(TModel model, GeneratorResult result);
        protected string Content = string.Empty;

        public abstract ReportType ReportType { get; }

        public virtual async Task<GeneratorResult> Generate(TModel model)
        {
            var generatorResult = new GeneratorResult();
            
            var template = await LoadTemplate(model);
            Content = FillTemplate(model, template);
            generatorResult.Report = await GenerateAndInsertData(model);
            await AfterGenerate(model, generatorResult);
            generatorResult.FileName = await GetReportName(model);
            generatorResult.FileType = ReportType.ToString().ToLower();
            return generatorResult;
        }

        private static string FormatField(object propertyValue, string format)
        {
            return propertyValue switch
            {
                DateTime date => date.ToString(format),
                TimeSpan time => time.ToString(format),
                _ => propertyValue.ToString()
            };
        }

        private string FillTemplate(object model, string template)
        {
            var regex = new Regex("\\[(.*?);");
            return regex.Replace(template, match =>
            {
                var propertyChain = match.Value;
                var isFormatted = false;
                var format = "";

                if (propertyChain.Contains(":"))
                {
                    var beginIndex = propertyChain.IndexOf(":", StringComparison.Ordinal) + 1;
                    format = propertyChain[beginIndex..^1];
                    propertyChain = propertyChain.Replace(format, "");
                    isFormatted = true;
                }

                propertyChain = propertyChain
                    .Replace(";", "")
                    .Replace(":", "")
                    .Replace("[", "")
                    .Replace("]", "");

                var properties = propertyChain.Split('.');

                var propertyValue = model;

                foreach (var property in properties)
                {
                    propertyValue = GetPropertyValue(propertyValue, property);

                    if (propertyValue == null)
                    {
                        throw new ArgumentNullException($"Property {property} is null in {propertyChain}");
                    }
                }

                var formattedData = FormatField(propertyValue, isFormatted ? format : "");

                return formattedData;
            });
        }

        private object GetPropertyValue(object model, string property)
        {
            var type = model.GetType();
            var propertyInfo = type.GetProperty(property);

            if (propertyInfo == null)
                throw new ArgumentException($"Property {property} not exists in type {type.FullName}");

            return propertyInfo.GetValue(model);
        }
    }
}