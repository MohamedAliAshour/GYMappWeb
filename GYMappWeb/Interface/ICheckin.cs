using GYMappWeb.Helper;
using GYMappWeb.ViewModels.Checkin;
using GYMappWeb.ViewModels.InvitedUserRequest;
using GYMappWeb.ViewModels.TblUser;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GYMappWeb.Interface
{
    public interface ICheckin
    {
        Task<List<GetCheckinViewModel>> GetAllCheckinsAsync(int gymBranchId);
        Task<PagedResult<GetCheckinViewModel>> GetWithPaginations(UserParameters userParam, int gymBranchId);
        Task<bool> Add(SaveCheckinViewModel model, string createdById, int gymBranchId);
        Task<bool> Delete(int id, int gymBranchId);
        SaveCheckinViewModel GetDetailsById(int id, int gymBranchId);
        Task<GetCheckinViewModel> GetCheckinDetailsAsync(int id, int gymBranchId);
        Task<List<SelectListItem>> GetUsersSelectList(int gymBranchId);
        Task<List<SelectListItem>> GetGymBranchesSelectList();
        Task<bool> CheckPhoneExistsAsync(string phone, int gymBranchId);
        Task<bool> CreateCheckinWithInvitationsAsync(SaveCheckinViewModel model, List<InvitedUserRequest> invitedUsers, string createdById, int gymBranchId);
        Task<TblUserViewModel> SearchUserByCodeAsync(int code, int gymBranchId);
        Task<TblUserViewModel> SearchUserByPhoneAsync(string phone, int gymBranchId);
        Task<bool> IsUserCheckedInAsync(int userId, int gymBranchId);
    }
}