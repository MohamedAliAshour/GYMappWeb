using GYMappWeb.Helper;
using GYMappWeb.Models;
using GYMappWeb.ViewModels.TblOffer;

namespace GYMappWeb.Interface
{
    public interface ITblOffer
    {
        Task<PagedResult<GetWithPaginationTblOfferViewModel>> GetAllOffersAsync(UserParameters userParameters);
        Task<bool> AddOfferAsync(SaveTblOfferViewModel model, string createdById);
        Task<bool> UpdateOfferAsync(SaveTblOfferViewModel model, int id, string updatedById);
        Task<bool> DeleteOfferAsync(int id);
        Task<bool> HasRelatedMembershipsAsync(int offerId);
        Task<TblOffer> GetOfferByIdAsync(int id);
        Task<SaveTblOfferViewModel> GetOfferDetailsByIdAsync(int id);
    }
}
