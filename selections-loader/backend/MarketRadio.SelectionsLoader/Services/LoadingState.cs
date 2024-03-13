using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using MarketRadio.SelectionsLoader.Services.Abstractions;

namespace MarketRadio.SelectionsLoader.Services
{
    public class LoadingProgress
    {
        public Guid Id { get; set; }
        public double Percentage { get; set; }

        public LoadingProgress()
        {
        }

        public LoadingProgress(Guid id, double percentage)
        {
            Id = id;
            Percentage = percentage;
        }
    }

    public class LoadingState : ILoadingState
    {
        private readonly BlockingCollection<LoadingProgress> _loadingProgress = new();

        public IObservable<LoadingProgress> ReactiveLoadingProgress => _loadingProgress
                                                                        .GetConsumingEnumerable()
                                                                        .ToObservable(TaskPoolScheduler.Default);

        public List<LoadingProgress> LoadingProgress { get; } = new();

        public void AddProgress(LoadingProgress progress)
        {
            // _loadingProgress.Add(progress);

            var loadingProgress = GetProgress(progress.Id);

            if (loadingProgress != null)
            {
                loadingProgress.Percentage = progress.Percentage;
            }
            else
            {
                LoadingProgress.Add(progress);
            }
        }

        public LoadingProgress GetProgress(Guid id)
        {
            return LoadingProgress.FirstOrDefault(lp => lp.Id == id);
        }
    }
}