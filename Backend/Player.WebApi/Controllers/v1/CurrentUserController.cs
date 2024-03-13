using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Player.BusinessLogic.Features.Users;

namespace Player.WebApi.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class CurrentUserController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CurrentUserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize]
        [HttpGet("permissions")]
        public async Task<PermissionList.Response> GetPermissions(CancellationToken cancellationToken) => await _mediator.Send(new PermissionList.Query(), cancellationToken);
        
        [Authorize]
        [HttpGet("objects")]
        public async Task<ObjectList.Response> GetObjects(CancellationToken cancellationToken) => await _mediator.Send(new ObjectList.Query(), cancellationToken);

        [Authorize]
        [HttpGet("info")]
        public async Task<GetCurrentUserInfo.Response> GetInfo(CancellationToken cancellationToken) =>  await _mediator.Send(new GetCurrentUserInfo.Query(), cancellationToken);
    }
}