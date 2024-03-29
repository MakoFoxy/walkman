using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
// ReSharper disable InconsistentNaming

namespace MarketRadio.Player.Services
{
    public class App : IApp
    {
        public string Version => version;
        public string ProductName => productName;
        public string RunId => runId;
        public DateTime StartDate => startDate;

        private static readonly string version;
        private static readonly string productName;
        private static readonly string runId;
        private static readonly DateTime startDate;
        //         version: Строка, хранящая версию приложения.
        // productName: Строка, хранящая название продукта.
        // runId: Строка, уникальный идентификатор текущего запуска приложения, генерируемый как GUID.
        // startDate: DateTime, дата и время запуска приложения.
        static App()
        {//Статический конструктор выполняется автоматически до создания первого экземпляра класса или ссылки на какие-либо статические члены.

            var manifestJson = File.ReadAllText("electron.manifest.json"); // Внутри конструктора считывается файл electron.manifest.json, который, вероятно, содержит метаданные Electron-приложения.Это делается с помощью метода File.ReadAllText("electron.manifest.json").
            var jObject = JsonConvert.DeserializeObject<JObject>(manifestJson)!;  //Считанный JSON-файл десериализуется в объект JObject с помощью JsonConvert.DeserializeObject<JObject>(manifestJson). Здесь предполагается, что используется библиотека Json.NET для работы с JSON.
            version = jObject["build"]!["buildVersion"]!.Value<string>()!; //Из объекта JObject извлекаются значения для версии (version), имени продукта (productName) из секции build файла манифеста, а также генерируется уникальный идентификатор запуска (runId) с использованием Guid.NewGuid().ToString().
            productName = jObject["build"]!["productName"]!.Value<string>()!;
            startDate = DateTime.Now; //Устанавливается startDate в текущее время и дату с помощью DateTime.Now.

            runId = Guid.NewGuid().ToString();
        }
    }
}

// Свойства интерфейса IApp

// Класс предоставляет четыре свойства только для чтения, соответствующие его приватным переменным, что позволяет внешним компонентам получать информацию о приложении, не изменяя её:

//     Version: Возвращает версию приложения.
//     ProductName: Возвращает имя продукта.
//     RunId: Возвращает уникальный идентификатор запуска.
//     StartDate: Возвращает дату и время запуска приложения.

// Общий анализ

// Этот класс является хорошим примером использования статического конструктора для инициализации статических данных, которые будут общими для всех экземпляров класса и всего приложения. Использование статических переменных и свойств позволяет легко получить доступ к общим данным о приложении из любой части программы.

// Также стоит отметить, что класс App строго соответствует принципу инкапсуляции, предоставляя доступ к данным только через свойства только для чтения, что предотвращает их нежелательное изменение.