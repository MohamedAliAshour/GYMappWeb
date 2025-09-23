using GYMappWeb.Helper;
using GYMappWeb.Models;
using GYMappWeb.ViewModels.TblOffer;

namespace GYMappWeb.Interface
{
    public interface ITblOffer
    {
        Task<PagedResult<GetWithPaginationTblOfferViewModel>> GetAllOffersAsync(UserParameters userParameters, int gymBranchId);
        Task<bool> AddOfferAsync(SaveTblOfferViewModel model, string createdById, int gymBranchId);
        Task<bool> UpdateOfferAsync(SaveTblOfferViewModel model, int id, string updatedById, int gymBranchId);
        Task<bool> ToggleOfferStatusAsync(int id, int gymBranchId);
        Task<bool> DeleteOfferAsync(int id, int gymBranchId);
        Task<bool> HasRelatedMembershipsAsync(int offerId, int gymBranchId);
        Task<TblOffer> GetOfferByIdAsync(int id, int gymBranchId);
        Task<SaveTblOfferViewModel> GetOfferDetailsByIdAsync(int id, int gymBranchId);
    }
}