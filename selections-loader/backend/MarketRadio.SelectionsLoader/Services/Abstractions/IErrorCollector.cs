using System;
using System.Collections.Generic;
using MarketRadio.SelectionsLoader.Models;

namespace MarketRadio.SelectionsLoader.Services.Abstractions
{
    public interface IErrorCollector
    {
        IReadOnlyCollection<Error> Errors { get; }
        void AddError(Error error);
        void RemoveError(Guid errorId);
        void RemoveError(Error error);
    }
}