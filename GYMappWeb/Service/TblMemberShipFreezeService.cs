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

        public TblMemberShipFreezeService(GYMappWebContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<GetWithPaginationTblMemberShipFreezeViewModel>> GetAllFreezesAsync(UserParameters userParameters)
        {
            var userNames = await _context.Users
                .Select(u => new { u.Id, u.UserName })
                .ToDictionaryAsync(u => u.Id, u => u.UserName);

            var query = _context.TblMemberShipFreezes
                .Include(f => f.UserMemberShip)
                .ThenInclude(um => um.User)
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

        public async Task<List<object>> GetFreezeRecordsAsync(int userMembershipId)
        {
            return await _context.TblMemberShipFreezes
                .Where(f => f.UserMemberShipId == userMembershipId)
                .OrderByDescending(f => f.FreezeStartDate)
                .Select(f => new {
                    freezeStartDate = f.FreezeStartDate.ToString("yyyy-MM-dd"),
                    freezeEndDate = f.FreezeEndDate.ToString("yyyy-MM-dd"),
                    reason = f.Reason
                })
                .ToListAsync<object>();
        }

        public async Task<object> GetMembershipFreezeDetailsAsync(int userMembershipId)
        {
            var membership = await _context.TblUserMemberShips
                .Include(um => um.MemberShipTypes)
                .FirstOrDefaultAsync(um => um.UserMemberShipId == userMembershipId);

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

        public async Task<bool> AddFreezeAsync(SaveTblMemberShipFreezeViewModel model, string createdById)
        {
            // Convert DateOnly to DateTime for calculation
            var startDate = new DateTime(model.FreezeStartDate.Year, model.FreezeStartDate.Month, model.FreezeStartDate.Day);
            var endDate = new DateTime(model.FreezeEndDate.Year, model.FreezeEndDate.Month, model.FreezeEndDate.Day);
            var freezeDuration = (endDate - startDate).Days + 1; // Add 1 to include both days

            // Get the associated membership
            var membership = await _context.TblUserMemberShips
                .FirstOrDefaultAsync(m => m.UserMemberShipId == model.UserMemberShipId);

            if (membership != null)
            {
                // Convert membership end date if it's DateOnly
                var membershipEndDate = new DateTime(membership.EndDate.Year, membership.EndDate.Month, membership.EndDate.Day);

                // Update the membership's total freeze days
                membership.TotalFreezedDays = (membership.TotalFreezedDays ?? 0) + freezeDuration;

                // Extend the membership end date by the freeze duration
                membership.EndDate = DateOnly.FromDateTime(membershipEndDate.AddDays(freezeDuration));

                _context.Update(membership);
            }

            // Create the freeze record
            model.CreatedBy = createdById;
            model.CreatedDate = DateTime.Today;

            var entity = ObjectMapper.Mapper.Map<TblMemberShipFreeze>(model);
            _context.Add(entity);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteFreezeAsync(int id)
        {
            var freeze = await _context.TblMemberShipFreezes.FindAsync(id);
            if (freeze == null)
            {
                throw new Exception("Freeze record not found");
            }

            _context.TblMemberShipFreezes.Remove(freeze);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<object>> GetActiveMembershipsForDropdownAsync()
        {
            return await _context.TblUserMemberShips
                .Include(um => um.User)
                .Include(um => um.MemberShipTypes)
                .Where(um => um.IsActive == true)
                .Select(um => new {
                    UserMemberShipId = um.UserMemberShipId,
                    UserName = $"{um.User.UserName} ({um.MemberShipTypes.Name})"
                })
                .ToListAsync<object>();
        }
    }
}