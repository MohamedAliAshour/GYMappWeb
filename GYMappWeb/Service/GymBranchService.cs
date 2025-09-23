using GYMappWeb.Areas.Identity.Data;
using GYMappWeb.Helper;
using GYMappWeb.Helpers;
using GYMappWeb.Interface;
using GYMappWeb.Models;
using GYMappWeb.ViewModels.GymBranch;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace GYMappWeb.Service
{
    public class GymBranchService : IGymBranch
    {
        private readonly GYMappWebContext _context;

        public GymBranchService(GYMappWebContext context)
        {
            _context = context;
        }

        public async Task<List<GetGymBranchViewModel>> GetAllGymBranchesAsync()
        {
            var userNames = await _context.Users
                .Select(u => new { u.Id, u.UserName })
                .ToDictionaryAsync(u => u.Id, u => u.UserName);

            var gymBranches = await _context.GymBranches
                .OrderBy(g => g.GymName)
                .Select(g => new GetGymBranchViewModel
                {
                    GymBranchId = g.GymBranchId,
                    GymName = g.GymName,
                    Location = g.Location,
                    CreateDate = g.CreateDate,
                    CreatedBy = g.CreatedBy,
                    IsActive = g.IsActive,
                    CreatedByUserName = g.CreatedBy != null && userNames.ContainsKey(g.CreatedBy)
                        ? userNames[g.CreatedBy]
                        : "Unknown"
                })
                .ToListAsync();

            return gymBranches;
        }

        public async Task<PagedResult<GetGymBranchViewModel>> GetWithPaginations(UserParameters userParam)
        {
            var userNames = await _context.Users
                .Select(u => new { u.Id, u.UserName })
                .ToDictionaryAsync(u => u.Id, u => u.UserName);

            var query = _context.GymBranches.AsQueryable();

            // Apply filtering
            if (!string.IsNullOrEmpty(userParam.SearchTerm))
            {
                query = query.Where(g =>
                    g.GymName.Contains(userParam.SearchTerm) ||
                    g.Location.Contains(userParam.SearchTerm));
            }

            // Apply sorting
            switch (userParam.SortBy)
            {
                case "GymName":
                    query = userParam.SortDescending
                        ? query.OrderByDescending(g => g.GymName)
                        : query.OrderBy(g => g.GymName);
                    break;
                case "Location":
                    query = userParam.SortDescending
                        ? query.OrderByDescending(g => g.Location)
                        : query.OrderBy(g => g.Location);
                    break;
                case "CreateDate":
                    query = userParam.SortDescending
                        ? query.OrderByDescending(g => g.CreateDate)
                        : query.OrderBy(g => g.CreateDate);
                    break;
                default:
                    query = query.OrderBy(g => g.GymName);
                    break;
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var items = await query
                .Skip((userParam.PageNumber - 1) * userParam.PageSize)
                .Take(userParam.PageSize)
                .Select(g => new GetGymBranchViewModel
                {
                    GymBranchId = g.GymBranchId,
                    GymName = g.GymName,
                    Location = g.Location,
                    CreateDate = g.CreateDate,
                    CreatedBy = g.CreatedBy,
                    IsActive = g.IsActive,
                    CreatedByUserName = g.CreatedBy != null && userNames.ContainsKey(g.CreatedBy)
                        ? userNames[g.CreatedBy]
                        : "Unknown"
                })
                .ToListAsync();

            return new PagedResult<GetGymBranchViewModel>(
                items,
                totalCount,
                userParam.PageNumber,
                userParam.PageSize);
        }

        public async Task<bool> Add(SaveGymBranchViewModel model, string createdById)
        {
            // Check for duplicate gym name
            bool nameExists = await _context.GymBranches
                .AnyAsync(g => g.GymName.ToLower() == model.GymName.ToLower());

            if (nameExists)
            {
                throw new Exception("A gym branch with this name already exists.");
            }

            // Check for duplicate location
            bool locationExists = await _context.GymBranches
                .AnyAsync(g => g.Location.ToLower() == model.Location.ToLower());

            if (locationExists)
            {
                throw new Exception("This location is already registered.");
            }

            // Set CreatedBy and CreateDate
            model.CreatedBy = createdById;
            model.CreateDate = DateTime.Now;

            var entity = ObjectMapper.Mapper.Map<GymBranch>(model);
            _context.Add(entity);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> Update(SaveGymBranchViewModel model, int id, string updatedById)
        {
            if (id != model.GymBranchId)
            {
                throw new Exception("Gym Branch ID mismatch");
            }

            var existingGymBranch = await _context.GymBranches.FindAsync(id);
            if (existingGymBranch == null)
            {
                throw new Exception("Gym Branch not found");
            }

            // Update properties
            existingGymBranch.GymName = model.GymName;
            existingGymBranch.Location = model.Location;
            existingGymBranch.CreatedBy = updatedById;
            existingGymBranch.CreateDate = DateTime.Now;

            _context.Update(existingGymBranch);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> Delete(int id)
        {
            var gymBranch = await _context.GymBranches.FindAsync(id);
            if (gymBranch == null)
            {
                throw new Exception("Gym Branch not found");
            }

            // Check if gym branch has users
            bool hasUsers = await _context.TblUsers.AnyAsync(u => u.GymBranchId == id) ||
                           await _context.Users.AnyAsync(u => u.GymBranchId == id);

            if (hasUsers)
            {
                throw new Exception("Cannot delete gym branch. There are users associated with this branch.");
            }

            _context.GymBranches.Remove(gymBranch);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CheckNameExist(string name)
        {
            return await _context.GymBranches.AnyAsync(g => g.GymName.ToLower() == name.ToLower());
        }

        public async Task<bool> CheckLocationExist(string location)
        {
            return await _context.GymBranches.AnyAsync(g => g.Location.ToLower() == location.ToLower());
        }

        public SaveGymBranchViewModel GetDetailsById(int id)
        {
            var gymBranch = _context.GymBranches.Find(id);
            return ObjectMapper.Mapper.Map<SaveGymBranchViewModel>(gymBranch);
        }

        // In IGymBranch interface


        // In GymBranchService implementation
        public async Task<bool> ToggleActive(int id)
        {
            var gymBranch = await _context.GymBranches.FindAsync(id);
            if (gymBranch == null)
            {
                throw new Exception("Gym Branch not found");
            }

            // Toggle the IsActive status
            gymBranch.IsActive = !gymBranch.IsActive;

            _context.Update(gymBranch);
            await _context.SaveChangesAsync();

            return true;
        }

     

        public async Task<GetGymBranchViewModel> GetGymBranchDetailsAsync(int id)
        {
            var userNames = await _context.Users
                .Select(u => new { u.Id, u.UserName })
                .ToDictionaryAsync(u => u.Id, u => u.UserName);

            var gymBranch = await _context.GymBranches
                .Where(g => g.GymBranchId == id)
                .Select(g => new GetGymBranchViewModel
                {
                    GymBranchId = g.GymBranchId,
                    GymName = g.GymName,
                    Location = g.Location,
                    CreateDate = g.CreateDate,
                    CreatedBy = g.CreatedBy,
                    CreatedByUserName = g.CreatedBy != null && userNames.ContainsKey(g.CreatedBy)
                        ? userNames[g.CreatedBy]
                        : "Unknown"
                })
                .FirstOrDefaultAsync();

            return gymBranch;
        }
    }
}
