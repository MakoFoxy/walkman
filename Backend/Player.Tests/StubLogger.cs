using System;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Player.Tests
{
    public class StubLogger<T> : ILogger<T>
    {
        //Этот класс StubLogger<T> представляет собой реализацию интерфейса ILogger<T> из Microsoft.Extensions.Logging, предназначенную для использования в тестовых сценариях. Давайте рассмотрим, как он устроен и что делает:
        private readonly ITestOutputHelper _output;

        public StubLogger(ITestOutputHelper output)
        {
            _output = output;
            //Конструктор принимает ITestOutputHelper, что является частью xUnit для записи вывода теста. Этот хелпер используется в методе Log для вывода сообщений лога.
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
            //В текущей реализации этот метод не реализован и при вызове будет выброшено исключение NotImplementedException. В реальных сценариях использования логгера, метод BeginScope используется для создания области логирования, где можно установить некоторые контекстные данные для всех логов, отправленных в рамках этой области. В контексте тестирования этот метод часто не требуется.
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            throw new NotImplementedException();//Этот метод предназначен для проверки, активен ли данный уровень логирования. В данной заглушке он также не реализован, так как для тестовой цели этот функционал может быть не критичен.
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            _output.WriteLine(state.ToString());
            //Это основной метод для записи логов. В этой реализации он записывает строковое представление state в тестовый вывод (_output). Таким образом, когда вы используете этот логгер в тестах, все сообщения лога будут напрямую выводиться в лог теста. Это удобно для отладки и проверки состояния во время выполнения тестов.
        }
    }
    //В реальных сценариях, вам может понадобиться расширить реализацию StubLogger, чтобы он мог обрабатывать различные уровни логирования или поддерживать области логирования, но для многих тестовых случаев текущей реализации может быть достаточно.
}