using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;
using Player.DataAccess.Extensions;
using Player.Domain;
using Player.DTOs;
using Player.Services;
using Player.Services.Abstractions;

namespace Player.BusinessLogic.Features.Selections
{
    public class List
    {
        public class Handler : IRequestHandler<Query, SelectionFilterResult>
        {
            private readonly PlayerContext _context;
            private readonly IMapper _mapper;
            private readonly IUserManager _userManager;

            public Handler(PlayerContext context, 
                IMapper mapper,
                IUserManager userManager)
            {
                _context = context;
                _mapper = mapper;
                _userManager = userManager;
            }

            public async Task<SelectionFilterResult> Handle(Query request,
                CancellationToken cancellationToken = default)
            {
                var permissions = await _userManager.GetCurrentUserPermissions(cancellationToken);
                var query = _context.Selections.AsQueryable();

                if (permissions.Any(p => p.Code == Permission.PartnerAccessToObject))
                {
                    var organization = await _userManager.GetUserOrganization(cancellationToken);
                    query = query.Where(s => s.Organization == organization || s.IsPublic);
                }
                
                var result = new SelectionFilterResult
                {
                    Page = request.Filter?.Page,
                    ItemsPerPage = request.Filter?.ItemsPerPage
                };

                if (request.Filter?.Actual != null)
                {
                    if (request.Filter.Actual.Value)
                    {
                        var now = DateTimeOffset.Now;
                        query = query.Where(s => s.DateBegin <=  now && (s.DateEnd == null || s.DateEnd >= now));
                    }
                }

                if (!string.IsNullOrWhiteSpace(request.Filter?.SearchText))
                    query = query.Where(s => s.Name.Contains(request.Filter.SearchText));

                if (request.Filter?.ObjectId != null)
                {
                    query = query.Where(s =>
                        s.Objects.Select(so => so).Any(o => o.ObjectId == request.Filter.ObjectId));
                }

                result.Result = await query
                    .GetPagedQuery(result.Page, result.ItemsPerPage)
                    .ProjectTo<SelectionListModel>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                result.TotalItems = await query.CountAsync(cancellationToken);

                return result;
            }
        }

        public class SelectionListModel : SimpleDto
        {
        }

        public class SelectionFilterModel : BaseFilterModel
        {
            public Guid? ObjectId { get; set; }
            public string SearchText { get; set; }
            public bool? Actual { get; set; }
        }

        public class Query : IRequest<SelectionFilterResult>
        {
            public SelectionFilterModel Filter { get; set; }
        }

        public class SelectionFilterResult : BaseFilterResult<SelectionListModel>
        {
        }
    }
}
