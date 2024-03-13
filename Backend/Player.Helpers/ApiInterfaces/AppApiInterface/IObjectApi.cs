using System;
using System.Threading.Tasks;
using Player.DTOs;
using Refit;

namespace Player.Helpers.ApiInterfaces.AppApiInterface
{
    public interface IObjectApi
    {
        [Get("/api/v1/object/{id}")]
        Task<ObjectDto> GetObject([AliasAs("id")] Guid id, [Header("Authorization")] string bearerToken);
    }
}