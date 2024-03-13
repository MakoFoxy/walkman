using System;
using System.Threading.Tasks;
using MarketRadio.SelectionsLoader.Api;
using MarketRadio.SelectionsLoader.DataAccess;
using MarketRadio.SelectionsLoader.Domain;
using MarketRadio.SelectionsLoader.Models;
using MarketRadio.SelectionsLoader.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketRadio.SelectionsLoader.Controllers
{
    [ApiController]
    [Route("/api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly IUserApi _userApi;
        private readonly ICurrentUserKeeper _currentUserKeeper;

        public AuthController(
            DatabaseContext context, 
            IUserApi userApi,
            ICurrentUserKeeper currentUserKeeper)
        {
            _context = context;
            _userApi = userApi;
            _currentUserKeeper = currentUserKeeper;
        }

        [HttpPost]
        public async Task<IActionResult> Auth(AuthModel authModel)
        {
            var response = await _userApi.Auth(authModel);
            response.EnsureSuccessStatusCode();
            var token = await response.Content.ReadAsStringAsync();
            var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    $"delete from {_context.GetTableName<Settings>()} where \"Key\" = '{Settings.Token}'");
                _context.Settings.Add(new Settings
                {
                    Key = Settings.Token,
                    Value = token,
                });
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                _currentUserKeeper.SetToken(token);
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                return BadRequest();
            }

            return Ok();
        }

        [HttpGet("is-authenticated")]
        public async Task<IActionResult> IsAuthenticated()
        {
            var token = _currentUserKeeper.Token;

            if (string.IsNullOrEmpty(token))
            {
                return StatusCode(401);
            }
            
            var response = await _userApi.IsAuthenticated(token);

            return !response.IsSuccessStatusCode ? StatusCode(401) : Ok();
        }
    }
}