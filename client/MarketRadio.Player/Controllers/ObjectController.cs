using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketRadio.Player.DataAccess;
using MarketRadio.Player.DataAccess.Domain;
using MarketRadio.Player.Models;
using MarketRadio.Player.Services;
using MarketRadio.Player.Services.Http;
using MarketRadio.Player.Services.LiveConnection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Player.ClientIntegration.Base;
using Object = MarketRadio.Player.DataAccess.Domain.Object;

namespace MarketRadio.Player.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ObjectController : ControllerBase
    {
        private readonly IObjectApi _objectApi;
        private readonly PlayerContext _context;
        private readonly ILogger<ObjectController> _logger;
        private readonly ServerLiveConnection _serverLiveConnection;
        private readonly PlayerStateManager _stateManager;
        private readonly ObjectSettingsService _objectSettingsService;

        public ObjectController(IObjectApi objectApi, 
            PlayerContext context,
            ILogger<ObjectController> logger,
            ServerLiveConnection serverLiveConnection,
            PlayerStateManager stateManager,
            ObjectSettingsService objectSettingsService)
        {
            _objectApi = objectApi;
            _context = context;
            _logger = logger;
            _serverLiveConnection = serverLiveConnection;
            _stateManager = stateManager;
            _objectSettingsService = objectSettingsService;
        }
        
        [HttpGet("all")]
        public async Task<ICollection<SimpleModel>> GetAll()
        {
            var token = await _context.UserSettings.Where(us => us.Key == UserSetting.Token)
                .Select(us => us.Value)
                .SingleAsync();
            var objects = await _objectApi.GetAll($"Bearer {token}");
            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.Objects.ExecuteDeleteAsync();
                _context.Objects.AddRange(objects.Objects.Select(o => new Object
                {
                    Id = o.Id,
                    Name = o.Name
                }));
                await _context.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "");
                await tx.RollbackAsync();
            }
            
            return objects.Objects;
        }

        [HttpGet("all/local")]
        public async Task<List<Object>> GetAllLocal()
        {
            return await _context.Objects.AsNoTracking().ToListAsync();
        }

        [HttpPost("{objectId}")]
        public async Task<ObjectInfo> SaveCurrentObject([FromRoute]Guid objectId)
        {
            var objectInfo = await _objectApi.GetFullInfo(objectId);
            var settingsRaw = await _objectApi.GetSettings(objectId);

            if (string.IsNullOrWhiteSpace(settingsRaw))
            {
                objectInfo.Settings = _objectSettingsService.FillSettingsIfNeeded(objectInfo);
            }
            else
            {
                objectInfo.Settings = JsonConvert.DeserializeObject<Settings>(settingsRaw)!;
            }

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.ObjectInfos.ExecuteDeleteAsync();
                _context.ObjectInfos.Add(objectInfo);
                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                var overrideObject = _stateManager.Object != null;
                
                _stateManager.Object = new ObjectInfoDto
                {
                    Id = objectInfo.Id,
                    Bin = objectInfo.Bin,
                    Name = objectInfo.Name,
                    BeginTime = objectInfo.BeginTime,
                    EndTime = objectInfo.EndTime,
                    FreeDays = objectInfo.FreeDays,
                    Settings = new SettingsDto
                    {
                        MusicVolume = objectInfo.Settings!.MusicVolumeComputed,
                        SilentTime = objectInfo.Settings.SilentTime,
                        AdvertVolume = objectInfo.Settings.AdvertVolumeComputed,
                        IsOnTop = objectInfo.Settings.IsOnTop,
                    }
                };

                if (overrideObject)
                {
                    await _serverLiveConnection.ConnectToNewObject(objectId);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "");
                await tx.RollbackAsync();
            }

            return objectInfo;
        }

        [HttpPost("settings")]
        public async Task<Settings> SaveCurrentObjectSettings([FromBody]Settings settings)
        {
            var objectInfo = await _context.ObjectInfos.SingleAsync();

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                objectInfo.Settings = settings;
                await _context.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "");
                await tx.RollbackAsync();
            }

            return settings;
        }

        [HttpGet("current")]
        public async Task<ObjectInfo?> GetCurrentObject()
        {
            var objectInfo = await _context.ObjectInfos.SingleOrDefaultAsync();

            if (objectInfo == null)
            {
                return null;
            }
            
            _stateManager.Object = new ObjectInfoDto
            {
                Id = objectInfo.Id,
                Bin = objectInfo.Bin,
                Name = objectInfo.Name,
                BeginTime = objectInfo.BeginTime,
                EndTime = objectInfo.EndTime,
                FreeDays = objectInfo.FreeDays,
                Settings = new SettingsDto
                {
                    MusicVolume = objectInfo.Settings!.MusicVolumeComputed,
                    SilentTime = objectInfo.Settings.SilentTime,
                    AdvertVolume = objectInfo.Settings.AdvertVolumeComputed,
                    IsOnTop = objectInfo.Settings.IsOnTop,
                }
            };
            return objectInfo;
        }

        [HttpPost("{objectId}/connect")]
        public async Task<IActionResult> Connect([FromRoute]Guid objectId)
        {
            await _serverLiveConnection.Connect(objectId);
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveCurrentObject()
        {
            var objectInfo = await _context.ObjectInfos.SingleAsync();
            _context.Remove(objectInfo);
            await _context.SaveChangesAsync();
            _stateManager.Object = null;
            return Ok();
        }
    }
}