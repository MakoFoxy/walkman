using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Player.BusinessLogic.Features.Users;
using Player.Services.Abstractions;

namespace Player.WebApi.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IUserManager _userManager;

        public AuthController(IMediator mediator, IUserManager userManager)
        {
            _mediator = mediator;
            _userManager = userManager;
        }

        [HttpGet("is-authenticated")]
        [Authorize]
        public IActionResult IsAuthenticated()
        {
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Authorize.Query query, CancellationToken token)
        {
            var authorizeResult = await _mediator.Send(query, token);

            if (authorizeResult.IsSuccess)
            {
                return Ok(authorizeResult.Token);
            }

            return Unauthorized();
        }

        [HttpGet("renew")]
        public async Task<IActionResult> Renew(CancellationToken token)
        {
            var user = await _userManager.GetCurrentUser(token);
            return await Post(new Authorize.Query
            {
                Email = user.Email,
                Password = user.Password,
            }, token);
        }
    }
}