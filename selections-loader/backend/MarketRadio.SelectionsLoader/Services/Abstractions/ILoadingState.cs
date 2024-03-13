using System;
using System.Collections.Generic;

namespace MarketRadio.SelectionsLoader.Services.Abstractions
{
    public interface ILoadingState
    {
        IObservable<LoadingProgress> ReactiveLoadingProgress { get; }
        List<LoadingProgress> LoadingProgress { get; }
        void AddProgress(LoadingProgress progress);
        LoadingProgress GetProgress(Guid id);
    }
}