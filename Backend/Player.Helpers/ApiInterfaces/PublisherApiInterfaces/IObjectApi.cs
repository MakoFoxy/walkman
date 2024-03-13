using System;
using System.Threading.Tasks;
using Player.DTOs;
using Refit;

namespace Player.Helpers.ApiInterfaces.PublisherApiInterfaces
{
    public interface IObjectApi
    {
        [Post("/api/v1/object/object-info-changed/{id}")]
        Task ObjectInfoChanged([AliasAs("id")] Guid id, [Header("Authorization")] string bearerToken);
        
        [Post("/api/v1/object/object-volume-changed/{id}")]
        Task ObjectVolumeChanged([AliasAs("id")] Guid id, [Body] ObjectVolumeChangedDto volumeData, [Header("Authorization")] string bearerToken);
    }
}