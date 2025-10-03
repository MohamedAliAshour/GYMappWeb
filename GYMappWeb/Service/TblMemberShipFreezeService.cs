using GYMappWeb.Areas.Identity.Data;
using GYMappWeb.Helper;
using GYMappWeb.Helpers;
using GYMappWeb.Interface;
using GYMappWeb.Models;
using GYMappWeb.ViewModels.TblMemberShipFreeze;
using GYMappWeb.ViewModels.TblUser;
using GYMappWeb.ViewModels.TblUserMemberShip;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GYMappWeb.Service
{
    public class TblMemberShipFreezeService : ITblMemberShipFreeze
    {
        private readonly GYMappWebContext _context;
        private readonly ILogging _logger;

        public TblMemberShipFreezeService(GYMappWebContext context, ILogging logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PagedResult<GetWithPaginationTblMemberShipFreezeViewModel>> GetAllFreezesAsync(UserParameters userParameters, int gymBranchId)
        {
            try
            {
                var userNames = await _context.Users
                    .Select(u => new { u.Id, u.UserName })
                    .ToDictionaryAsync(u => u.Id, u => u.UserName);

                var query = _context.TblMemberShipFreezes
                    .Include(f => f.UserMemberShip)
                    .ThenInclude(um => um.User)
                    .Where(f => f.UserMemberShip.User.GymBranchId == gymBranchId)
                    .AsQueryable();

                // Apply filtering
                if (!string.IsNullOrEmpty(userParameters.SearchTerm))
                {
                    query = query.Where(f =>
                        f.UserMemberShip.User.UserName.Contains(userParameters.SearchTerm) ||
                        f.Reason.Contains(userParameters.SearchTerm));
                }

                // Apply sorting
                switch (userParameters.SortBy)
                {
                    case "User":
                        query = userParameters.SortDescending
                            ? query.OrderByDescending(f => f.UserMemberShip.User.UserName)
                            : query.OrderBy(f => f.UserMemberShip.User.UserName);
                        break;
                    case "StartDate":
                        query = userParameters.SortDescending
                            ? query.OrderByDescending(f => f.FreezeStartDate)
                            : query.OrderBy(f => f.FreezeStartDate);
                        break;
                    case "EndDate":
                        query = userParameters.SortDescending
                            ? query.OrderByDescending(f => f.FreezeEndDate)
                            : query.OrderBy(f => f.FreezeEndDate);
                        break;
                    default:
                        query = query.OrderBy(f => f.FreezeStartDate);
                        break;
                }

                // Get total count before pagination
                var totalCount = await query.CountAsync();

                // Apply pagination
                var items = await query
                    .Skip((userParameters.PageNumber - 1) * userParameters.PageSize)
                    .Take(userParameters.PageSize)
                    .Select(f => new GetWithPaginationTblMemberShipFreezeViewModel
                    {
                        MemberShipFreezeId = f.MemberShipFreezeId,
                        UserMemberShipId = f.UserMemberShipId,
                        FreezeStartDate = f.FreezeStartDate,
                        FreezeEndDate = f.FreezeEndDate,
                        Reason = f.Reason,
                        CreatedDate = f.CreatedDate,
                        CreatedBy = f.CreatedBy,
                        CreatedByUserName = f.CreatedBy != null && userNames.ContainsKey(f.CreatedBy)
                            ? userNames[f.CreatedBy]
                            : "Unknown",
                        UserMemberShip = new TblUserMemberShipViewModel
                        {
                            UserMemberShipId = f.UserMemberShip.UserMemberShipId,
                            User = new TblUserViewModel
                            {
                                UserName = f.UserMemberShip.User.UserName
                            }
                        }
                    })
                    .ToListAsync();

                return new PagedResult<GetWithPaginationTblMemberShipFreezeViewModel>(
                    items,
                    totalCount,
                    userParameters.PageNumber,
                    userParameters.PageSize);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Failed to get all freezes. GymBranch: {gymBranchId}, Page: {userParameters.PageNumber}, Search: {userParameters.SearchTerm}",
                    ex,
                    nameof(TblMemberShipFreezeService),
                    nameof(GetAllFreezesAsync)
                );
                throw;
            }
        }

        public async Task<List<object>> GetFreezeRecordsAsync(int userMembershipId, int gymBranchId)
        {
            try
            {
                return await _context.TblMemberShipFreezes
                    .Include(f => f.UserMemberShip)
                    .ThenInclude(um => um.User)
                    .Where(f => f.UserMemberShipId == userMembershipId &&
                              f.UserMemberShip.User.GymBranchId == gymBranchId)
                    .OrderByDescending(f => f.FreezeStartDate)
                    .Select(f => new {
                        freezeStartDate = f.FreezeStartDate.ToString("yyyy-MM-dd"),
                        freezeEndDate = f.FreezeEndDate.ToString("yyyy-MM-dd"),
                        reason = f.Reason
                    })
                    .ToListAsync<object>();
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Failed to get freeze records. UserMembershipId: {userMembershipId}, GymBranch: {gymBranchId}",
                    ex,
                    nameof(TblMemberShipFreezeService),
                    nameof(GetFreezeRecordsAsync)
                );
                throw;
            }
        }

        public async Task<object> GetMembershipFreezeDetailsAsync(int userMembershipId, int gymBranchId)
        {
            try
            {
                var membership = await _context.TblUserMemberShips
                    .Include(um => um.MemberShipTypes)
                    .Include(um => um.User)
                    .FirstOrDefaultAsync(um => um.UserMemberShipId == userMembershipId &&
                                             um.User.GymBranchId == gymBranchId);

                if (membership == null)
                {
                    return null;
                }

                var freezes = await _context.TblMemberShipFreezes
                    .Where(f => f.UserMemberShipId == userMembershipId)
                    .ToListAsync();

                // Calculate used freeze days
                int usedFreezeDays = 0;
                foreach (var freeze in freezes)
                {
                    var startDate = new DateTime(freeze.FreezeStartDate.Year, freeze.FreezeStartDate.Month, freeze.FreezeStartDate.Day);
                    var endDate = new DateTime(freeze.FreezeEndDate.Year, freeze.FreezeEndDate.Month, freeze.FreezeEndDate.Day);
                    usedFreezeDays += (endDate - startDate).Days + 1; // +1 to include both days
                }

                var totalAllowedFreezeDays = membership.MemberShipTypes?.TotalFreezeDays ?? 0;
                var remainingFreezeDays = Math.Max(0, totalAllowedFreezeDays - usedFreezeDays);

                var totalFreezeCount = membership.MemberShipTypes?.FreezeCount ?? 0;
                var remainingFreezeCount = Math.Max(0, totalFreezeCount - freezes.Count);

                return new
                {
                    freezeCount = freezes.Count,
                    remainingFreezeCount = remainingFreezeCount,
                    totalFreezeDays = totalAllowedFreezeDays,
                    remainingFreezeDays = remainingFreezeDays
                };
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Failed to get membership freeze details. UserMembershipId: {userMembershipId}, GymBranch: {gymBranchId}",
                    ex,
                    nameof(TblMemberShipFreezeService),
                    nameof(GetMembershipFreezeDetailsAsync)
                );
                throw;
            }
        }

        public async Task<bool> AddFreezeAsync(SaveTblMemberShipFreezeViewModel model, string createdById, int gymBranchId)
        {
            try
            {
                // Verify the membership belongs to the same gym branch
                var membership = await _context.TblUserMemberShips
                    .Include(um => um.User)
                    .FirstOrDefaultAsync(m => m.UserMemberShipId == model.UserMemberShipId &&
                                            m.User.GymBranchId == gymBranchId);

                if (membership == null)
                {
                    throw new Exception("Membership not found or doesn't belong to this gym branch");
                }

                // Convert DateOnly to DateTime for calculation
                var startDate = new DateTime(model.FreezeStartDate.Year, model.FreezeStartDate.Month, model.FreezeStartDate.Day);
                var endDate = new DateTime(model.FreezeEndDate.Year, model.FreezeEndDate.Month, model.FreezeEndDate.Day);
                var freezeDuration = (endDate - startDate).Days + 1; // Add 1 to include both days

                // Update the membership's total freeze days
                membership.TotalFreezedDays = (membership.TotalFreezedDays ?? 0) + freezeDuration;

                // Convert membership end date if it's DateOnly
                var membershipEndDate = new DateTime(membership.EndDate.Year, membership.EndDate.Month, membership.EndDate.Day);

                // Extend the membership end date by the freeze duration
                membership.EndDate = DateOnly.FromDateTime(membershipEndDate.AddDays(freezeDuration));

                _context.Update(membership);

                // Create the freeze record
                model.CreatedBy = createdById;
                model.CreatedDate = DateTime.Today;

                var entity = ObjectMapper.Mapper.Map<TblMemberShipFreeze>(model);
                _context.Add(entity);

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Failed to add freeze. UserMembershipId: {model.UserMemberShipId}, StartDate: {model.FreezeStartDate}, EndDate: {model.FreezeEndDate}, CreatedBy: {createdById}",
                    ex,
                    nameof(TblMemberShipFreezeService),
                    nameof(AddFreezeAsync)
                );
                throw;
            }
        }

        public async Task<bool> DeleteFreezeAsync(int id, int gymBranchId)
        {
            try
            {
                var freeze = await _context.TblMemberShipFreezes
                    .Include(f => f.UserMemberShip)
                    .ThenInclude(um => um.User)
                    .FirstOrDefaultAsync(f => f.MemberShipFreezeId == id &&
                                           f.UserMemberShip.User.GymBranchId == gymBranchId);

                if (freeze == null)
                {
                    throw new Exception("Freeze record not found or doesn't belong to this gym branch");
                }

                // Calculate the freeze duration in days
                var freezeDuration = (freeze.FreezeEndDate.DayNumber - freeze.FreezeStartDate.DayNumber) + 1;

                // Get the associated membership
                var membership = freeze.UserMemberShip;
                if (membership != null)
                {
                    // Reduce the total freeze days
                    membership.TotalFreezedDays = Math.Max(0, (membership.TotalFreezedDays ?? 0) - freezeDuration);

                    // Reduce the membership end date by the freeze duration
                    membership.EndDate = membership.EndDate.AddDays(-freezeDuration);

                    _context.Update(membership);
                }

                // Remove the freeze record
                _context.TblMemberShipFreezes.Remove(freeze);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Failed to delete freeze. FreezeId: {id}, GymBranch: {gymBranchId}",
                    ex,
                    nameof(TblMemberShipFreezeService),
                    nameof(DeleteFreezeAsync)
                );
                throw;
            }
        }

        public async Task<List<object>> GetActiveMembershipsForDropdownAsync(int gymBranchId)
        {
            try
            {
                return await _context.TblUserMemberShips
                    .Include(um => um.User)
                    .Include(um => um.MemberShipTypes)
                    .Where(um => um.IsActive == true && um.User.GymBranchId == gymBranchId)
                    .Select(um => new {
                        UserMemberShipId = um.UserMemberShipId,
                        UserName = $"{um.User.UserName} ({um.User.UserCode})",
                        UserCode = um.User.UserCode
                    })
                    .ToListAsync<object>();
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Failed to get active memberships for dropdown. GymBranch: {gymBranchId}",
                    ex,
                    nameof(TblMemberShipFreezeService),
                    nameof(GetActiveMembershipsForDropdownAsync)
                );
                throw;
            }
        }

        public async Task<bool> HasDateOverlapAsync(int userMembershipId, DateTime startDate, DateTime endDate, int gymBranchId)
        {
            try
            {
                // Verify the membership belongs to the same gym branch
                var membership = await _context.TblUserMemberShips
                    .Include(um => um.User)
                    .FirstOrDefaultAsync(um => um.UserMemberShipId == userMembershipId &&
                                             um.User.GymBranchId == gymBranchId);

                if (membership == null)
                {
                    throw new Exception("Membership not found or doesn't belong to this gym branch");
                }

                // Get all existing freezes for this membership
                var existingFreezes = await _context.TblMemberShipFreezes
                    .Where(f => f.UserMemberShipId == userMembershipId)
                    .ToListAsync();

                foreach (var freeze in existingFreezes)
                {
                    // Convert DateOnly to DateTime for comparison
                    DateTime freezeStart = freeze.FreezeStartDate.ToDateTime(TimeOnly.MinValue);
                    DateTime freezeEnd = freeze.FreezeEndDate.ToDateTime(TimeOnly.MinValue);

                    // Check if the new period overlaps with any existing period
                    if (startDate <= freezeEnd && endDate >= freezeStart)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Failed to check date overlap. UserMembershipId: {userMembershipId}, StartDate: {startDate}, EndDate: {endDate}, GymBranch: {gymBranchId}",
                    ex,
                    nameof(TblMemberShipFreezeService),
                    nameof(HasDateOverlapAsync)
                );
                throw;
            }
        }
    }
}