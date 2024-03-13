using System;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Player.BusinessLogic.Features.Objects;
using Player.Domain;

namespace Player.WebApi.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReportController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Get(Guid objectId, DateTime date)
        {
            var response = await _mediator.Send(new PlaningMediaPlanReport.Query {ObjectId = objectId, Date = date});

            if (!response.IsSuccess)
            {
                return NotFound();
            }

            return File(response.File, "application/octet-stream", response.FileName);
        }

        [HttpGet("media-plan")]
        [Authorize(Policy = Permission.ReadMediaPlanReport, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        //TODO Разобраться где используется
        public async Task<IActionResult> Get([FromQuery] MediaPlanReport.Query query)
        {
            var response = await _mediator.Send(query);

            if (!response.File.Any())
            {
                return BadRequest();
            }

            return File(response.File, "application/octet-stream", $"{response.FileName}.{response.Type}");
        }
    }
}
