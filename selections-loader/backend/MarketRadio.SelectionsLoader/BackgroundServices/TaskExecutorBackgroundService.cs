using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using MarketRadio.SelectionsLoader.Api;
using MarketRadio.SelectionsLoader.DataAccess;
using MarketRadio.SelectionsLoader.Domain;
using MarketRadio.SelectionsLoader.Extensions;
using MarketRadio.SelectionsLoader.Models;
using MarketRadio.SelectionsLoader.Services;
using MarketRadio.SelectionsLoader.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Refit;
using Task = System.Threading.Tasks.Task;
using TaskType = MarketRadio.SelectionsLoader.Domain.TaskType;

namespace MarketRadio.SelectionsLoader.BackgroundServices
{
    public class TaskExecutorBackgroundService : BackgroundService
    {
        public static bool StartService { get; set; }
        
        private readonly IServiceProvider _serviceProvider;
        private readonly ICurrentUserKeeper _currentUserKeeper;
        private readonly ILogger<TaskExecutorBackgroundService> _logger;

        public TaskExecutorBackgroundService(
            IServiceProvider serviceProvider,
            ICurrentUserKeeper currentUserKeeper,
            ILogger<TaskExecutorBackgroundService> logger
        )
        {
            _serviceProvider = serviceProvider;
            _currentUserKeeper = currentUserKeeper;
            _logger = logger;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (StartService)
                {
                    try
                    {
                        await DoWork(stoppingToken);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "");
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }

        private async Task DoWork(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            var trackApi = scope.ServiceProvider.GetRequiredService<ITrackApi>();
            var selectionApi = scope.ServiceProvider.GetRequiredService<ISelectionApi>();
            var errorCollector = scope.ServiceProvider.GetRequiredService<IErrorCollector>();
            var tracksLoadingState = scope.ServiceProvider.GetRequiredService<ILoadingState>();

            var token = _currentUserKeeper.Token;
            
            if (string.IsNullOrEmpty(token))
            {
                errorCollector.AddError(new Error
                {
                    Text = "Не пройдена авторизация, пожалуйста перезайдите в приложение",
                    CreateDate = DateTime.Now,
                });
                StartService = false;
                return;
            }
            
            var tasks = await context.Tasks
                .Where(t => !t.IsFinished)
                .OrderBy(t => t.Priority)
                .ThenBy(t => t.CreateDate)
                .ToListAsync(cancellationToken);

            if (!tasks.Any())
            {
                StartService = false;
            }

            foreach (var task in tasks)
            {
                if (!StartService)
                {
                    continue;
                }

                switch (task.TaskType)
                {
                    case TaskType.None:
                        throw new ArgumentOutOfRangeException();
                    case TaskType.Selection:
                    {
                        await UploadSelection(task);
                        break;
                    }
                    case TaskType.Track:
                    {
                        var track = await context.Tracks
                            .Include(t => t.Genres)
                            .Where(s => s.Id == task.TaskObjectId)
                            .SingleAsync(cancellationToken);
                        
                        await UploadTrack(track, null);
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                task.IsFinished = true;
                task.FinishDate = DateTime.Now;
                await context.SaveChangesAsync(cancellationToken);
            }
            
            async Task UploadSelection(Domain.Task task)
            {
                var selection = await context.Selections
                    .Include(s => s.Tracks)
                    .ThenInclude(t => t.Track)
                    .ThenInclude(t => t.Genres)
                    .Where(s => s.Id == task.TaskObjectId)
                    .SingleAsync(cancellationToken);
                
                foreach (var tracksForUpload in selection.Tracks.Where(t => !t.Track.Uploaded))
                {
                    if (!StartService)
                    {
                        continue;
                    }

                    var track = tracksForUpload.Track;
                    await UploadTrack(track, selection);
                }

                await CreateSelection(selection);
            }

            async Task CreateSelection(Selection selection)
            {
                var updateSelectionModel = new UpdateSelectionModel
                {
                    Id = selection.Id,
                    Name = selection.Name,
                    Tracks = selection.Tracks.Select(t => t.TrackId).ToList(),
                    DateBegin = selection.DateBegin,
                    DateEnd = selection.DateEnd,
                    IsPublic = selection.IsPublic,
                };
                var response = await selectionApi.CreateSelection(updateSelectionModel, token);

                response.EnsureSuccessStatusCode();
                selection.Created = true;

                await context.SaveChangesAsync();
            }

            async Task UploadTrack(Track track, Selection selection)
            {
                var fileBytes = await File.ReadAllBytesAsync(track.Path, cancellationToken);

                var sleep = false;
                
                await using var progressableStream = new ProgressableStream<Guid>(new MemoryStream(fileBytes), track.Id,
                    (uploaded, length, id) =>
                    {
                        double uploadedPercent;

                        if (sleep)
                        {
                            Thread.Sleep(10000);
                        }

                        if (selection != null)
                        {
                            var selectionUploadedTracksCount = selection.UploadedTracksCount;
                            uploadedPercent = 100.0 * (selectionUploadedTracksCount + uploaded) /
                                              selection.TracksLength;
                        }
                        else
                        {
                            uploadedPercent = 100.0 * uploaded / length;
                        }

                        uploadedPercent = Math.Round(uploadedPercent, 2, MidpointRounding.ToNegativeInfinity);

                        tracksLoadingState.AddProgress(new LoadingProgress(selection?.Id ?? id, uploadedPercent));
                    });

                const string contentType = "audio/mp3";
                var file = new StreamPart(progressableStream, Path.GetFileName(track.Path), contentType);

                //TODO Поправить
                var genre = track.Genres.First();

                var genreJson = JsonConvert.SerializeObject(new GenreDto
                {
                    Id = genre.Id,
                    Name = genre.Name
                });
                track.UploadInProgress = true;
                await context.SaveChangesAsync(cancellationToken);
                Console.WriteLine($"Id={track.Id}; Started");
                var response = await trackApi.SaveMusic(genreJson, track.Id, new[] {file}, token, cancellationToken);
                Console.WriteLine($"Id={track.Id}; Ended");
                track.UploadInProgress = false;
                await context.SaveChangesAsync(cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    track.Uploaded = true;
                    await context.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.BadRequest:
                        case HttpStatusCode.MethodNotAllowed:
                        case HttpStatusCode.NotFound:
                        case HttpStatusCode.RequestEntityTooLarge:
                        case HttpStatusCode.RequestUriTooLong:
                        case HttpStatusCode.UnsupportedMediaType:
                        case HttpStatusCode.InternalServerError:
                        case HttpStatusCode.NotImplemented:
                        {
                            var error = new Error
                            {
                                Text = "Вы столкнулись с багом обратитесь к администратору",
                                CreateDate = DateTime.Now,
                                Metadata = response.StatusCode,
                            };
                            _logger.LogError("Error {@Error}", error);
                            errorCollector.AddError(error);
                            break;
                        }
                        case HttpStatusCode.Unauthorized:
                        {
                            errorCollector.AddError(new Error
                            {
                                Text = "Не пройдена авторизация, пожалуйста перезайдите в приложение",
                                CreateDate = DateTime.Now,
                            });
                            StartService = false;
                            break;
                        }
                        case HttpStatusCode.Forbidden:
                        case HttpStatusCode.RequestTimeout:
                        case HttpStatusCode.TooManyRequests:
                        case HttpStatusCode.BadGateway:
                        case HttpStatusCode.ServiceUnavailable:
                        case HttpStatusCode.GatewayTimeout:
                        {
                            var error = new Error
                            {
                                Text = "У нас временные проблемы с сервером",
                                CreateDate = DateTime.Now,
                                Metadata = response.StatusCode,
                            };
                            _logger.LogError("Error {@Error}", error);
                            errorCollector.AddError(error);
                            StartService = false;
                            break;
                        }
                        default:
                        {
                            var error = new Error
                            {
                                Text = "Вы столкнулись с багом обратитесь к администратору",
                                CreateDate = DateTime.Now,
                                Metadata = response.StatusCode,
                            };
                            _logger.LogError("Error {@Error}", error);
                            errorCollector.AddError(error);
                            break;
                        }
                    }
                }
            }
        }
    }
}