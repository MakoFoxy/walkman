using System;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Player.Tests
{
    public class StubLogger<T> : ILogger<T>
    {
        private readonly ITestOutputHelper _output;

        public StubLogger(ITestOutputHelper output)
        {
            _output = output;
        }
        
        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            throw new NotImplementedException();
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            _output.WriteLine(state.ToString());
        }
    }
}