using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using MarketRadio.SelectionsLoader.Models;
using Refit;

namespace MarketRadio.SelectionsLoader.Api
{
    public interface ISelectionApi
    {
        [Get("/api/v1/selections/all")]
        Task<ICollection<SelectionDto>> GetSelections([Header("Authorization")] string token);

        [Get("/api/v1/selections/{id}")]
        Task<SelectionDto> GetSelection([AliasAs("id")] Guid id, [Header("Authorization")] string token);

        [Post("/api/v1/selections")]
        Task<HttpResponseMessage> CreateSelection([Body] UpdateSelectionModel model,
            [Header("Authorization")] string token);
    }
}