using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarketRadio.Player.DataAccess.Domain;
using Player.ClientIntegration.Base;
using Refit;
using Object = MarketRadio.Player.DataAccess.Domain.Object;

namespace MarketRadio.Player.Services.Http
{
    public interface IObjectApi
    {
        [Get("/api/v1/current-user/objects")]
        Task<UserObjectsResponse> GetAll([Header("Authorization")]string token);

        [Get("/api/v1/object/{id}")]
        Task<ObjectInfo> GetFullInfo([AliasAs("id")]Guid objectId);

        [Get("/api/v1/client/{id}/settings")]
        Task<string> GetSettings([AliasAs("id")] Guid objectId);
    }

    public class UserObjectsResponse
    {
        public ICollection<SimpleModel> Objects { get; set; } = new List<SimpleModel>();
    }
}