using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace MarketRadio.Player.Workers
{
    public abstract class PlayerBackgroundServiceBase : BackgroundService
    {
        private readonly PlayerStateManager _stateManager;

        public PlayerBackgroundServiceBase(PlayerStateManager stateManager)
        {
            _stateManager = stateManager;
        }

        protected bool NowTheWorkingTime
        {
            get
            {
                if (_stateManager.Object == null)
                {
                    throw new ArgumentNullException($"Object in state manager is null, use {nameof(WaitForObject)} before access to object");
                }
                
                return _stateManager.Object.BeginTime <= DateTime.Now.TimeOfDay &&
                       _stateManager.Object.EndTime > DateTime.Now.TimeOfDay ||
                       _stateManager.Object.FreeDays.Contains(DateTime.Now.DayOfWeek);
            }
        }

        protected async Task WaitForObject(CancellationToken stoppingToken)
        {
            while (_stateManager.Object == null)
            {
                await Task.Delay(TimeSpan.FromSeconds(.5), stoppingToken);
            }
        }
        
        protected async Task WaitForPlaylist(CancellationToken stoppingToken)
        {
            while (_stateManager.Playlist == null)
            {
                await Task.Delay(TimeSpan.FromSeconds(.5), stoppingToken);
            }
        }
    }
}