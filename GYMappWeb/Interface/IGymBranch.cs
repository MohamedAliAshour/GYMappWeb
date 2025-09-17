using GYMappWeb.Helper;
using GYMappWeb.ViewModels.GymBranch;

namespace GYMappWeb.Interface
{
    public interface IGymBranch
    {
        Task<List<GetGymBranchViewModel>> GetAllGymBranchesAsync();
        Task<PagedResult<GetGymBranchViewModel>> GetWithPaginations(UserParameters userParam);
        Task<bool> Add(SaveGymBranchViewModel model, string createdById);
        Task<bool> Update(SaveGymBranchViewModel model, int id, string updatedById);
        Task<bool> Delete(int id);
        Task<bool> CheckNameExist(string name);
        Task<bool> CheckLocationExist(string location);
        SaveGymBranchViewModel GetDetailsById(int id);
        Task<GetGymBranchViewModel> GetGymBranchDetailsAsync(int id);
    }
}
