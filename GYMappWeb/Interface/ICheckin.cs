using GYMappWeb.Helper;
using GYMappWeb.ViewModels.Checkin;
using GYMappWeb.ViewModels.InvitedUserRequest;
using GYMappWeb.ViewModels.TblUser;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GYMappWeb.Interface
{
    public interface ICheckin
    {
        Task<List<GetCheckinViewModel>> GetAllCheckinsAsync();
        Task<PagedResult<GetCheckinViewModel>> GetWithPaginations(UserParameters userParam);
        Task<bool> Add(SaveCheckinViewModel model, string createdById,int GymBranchId);
        Task<bool> Delete(int id);
        SaveCheckinViewModel GetDetailsById(int id);
        Task<GetCheckinViewModel> GetCheckinDetailsAsync(int id);
        Task<List<SelectListItem>> GetUsersSelectList();
        Task<List<SelectListItem>> GetGymBranchesSelectList();
        Task<bool> CheckPhoneExistsAsync(string phone);
        Task<bool> CreateCheckinWithInvitationsAsync(SaveCheckinViewModel model, List<InvitedUserRequest> invitedUsers, string createdById, int gymBranchId);
        Task<TblUserViewModel> SearchUserByCodeAsync(int code);

        Task<TblUserViewModel> SearchUserByPhoneAsync(string phone);

        Task<bool> IsUserCheckedInAsync(int userId);
    }
}
