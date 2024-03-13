using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Player.BusinessLogic.Features.AdvertTypes;
using Player.Domain;

namespace Player.WebApi.Controllers.v1.Dictionary
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AdvertTypesController : Controller
    {
        private readonly IMediator _mediator;

        public AdvertTypesController(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        [HttpGet]
        [Authorize(Policy = Permission.ReadAllAdvertTypes, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ICollection<List.AdvertTypeModel>> Get(CancellationToken cancellationToken) => await _mediator.Send(new List.Query(), cancellationToken);
    }
}