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
        private readonly ILogging _logger;

        public GymBranchService(GYMappWebContext context, ILogging logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<GetGymBranchViewModel>> GetAllGymBranchesAsync()
        {
            try
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
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    "Failed to get all gym branches",
                    ex,
                    nameof(GymBranchService),
                    nameof(GetAllGymBranchesAsync)
                );
                throw;
            }
        }

        public async Task<PagedResult<GetGymBranchViewModel>> GetWithPaginations(UserParameters userParam)
        {
            try
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
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Failed to get paginated gym branches. Page: {userParam.PageNumber}, Size: {userParam.PageSize}, Search: {userParam.SearchTerm}",
                    ex,
                    nameof(GymBranchService),
                    nameof(GetWithPaginations)
                );
                throw;
            }
        }

        public async Task<bool> Add(SaveGymBranchViewModel model, string createdById)
        {
            try
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
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Failed to add gym branch. Name: {model.GymName}, Location: {model.Location}, CreatedBy: {createdById}",
                    ex,
                    nameof(GymBranchService),
                    nameof(Add)
                );
                throw;
            }
        }

        public async Task<bool> Update(SaveGymBranchViewModel model, int id, string updatedById)
        {
            try
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
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Failed to update gym branch. ID: {id}, Name: {model.GymName}, UpdatedBy: {updatedById}",
                    ex,
                    nameof(GymBranchService),
                    nameof(Update)
                );
                throw;
            }
        }

        public async Task<bool> Delete(int id)
        {
            try
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
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Failed to delete gym branch. ID: {id}",
                    ex,
                    nameof(GymBranchService),
                    nameof(Delete)
                );
                throw;
            }
        }

        public async Task<bool> CheckNameExist(string name)
        {
            try
            {
                return await _context.GymBranches.AnyAsync(g => g.GymName.ToLower() == name.ToLower());
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Failed to check gym name existence. Name: {name}",
                    ex,
                    nameof(GymBranchService),
                    nameof(CheckNameExist)
                );
                throw;
            }
        }

        public async Task<bool> CheckLocationExist(string location)
        {
            try
            {
                return await _context.GymBranches.AnyAsync(g => g.Location.ToLower() == location.ToLower());
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Failed to check location existence. Location: {location}",
                    ex,
                    nameof(GymBranchService),
                    nameof(CheckLocationExist)
                );
                throw;
            }
        }

        public SaveGymBranchViewModel GetDetailsById(int id)
        {
            try
            {
                var gymBranch = _context.GymBranches.Find(id);
                return ObjectMapper.Mapper.Map<SaveGymBranchViewModel>(gymBranch);
            }
            catch (Exception ex)
            {
                _logger.LogErrorAsync(
                    $"Failed to get gym branch details by ID. ID: {id}",
                    ex,
                    nameof(GymBranchService),
                    nameof(GetDetailsById)
                ).Wait();
                throw;
            }
        }

        public async Task<bool> ToggleActive(int id)
        {
            try
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
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Failed to toggle active status for gym branch. ID: {id}",
                    ex,
                    nameof(GymBranchService),
                    nameof(ToggleActive)
                );
                throw;
            }
        }

        public async Task<GetGymBranchViewModel> GetGymBranchDetailsAsync(int id)
        {
            try
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
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Failed to get gym branch details. ID: {id}",
                    ex,
                    nameof(GymBranchService),
                    nameof(GetGymBranchDetailsAsync)
                );
                throw;
            }
        }
    }
}