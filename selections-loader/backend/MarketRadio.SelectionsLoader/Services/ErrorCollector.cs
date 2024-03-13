using System;
using System.Collections.Generic;
using System.Linq;
using MarketRadio.SelectionsLoader.Models;
using MarketRadio.SelectionsLoader.Services.Abstractions;

namespace MarketRadio.SelectionsLoader.Services
{
    public class ErrorCollector : IErrorCollector
    {
        private readonly List<Error> _errors = new();

        public IReadOnlyCollection<Error> Errors
        {
            get => _errors.AsReadOnly();
        }

        public void AddError(Error error)
        {
            _errors.Add(error);
        }

        public void RemoveError(Guid errorId)
        {
            var error = _errors.Single(e => e.Id == errorId);
            RemoveError(error);
        }
        
        public void RemoveError(Error error)
        {
            _errors.Remove(error);
        }
    }
}