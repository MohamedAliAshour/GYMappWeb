using GYMappWeb.Areas.Identity.Data;
using GYMappWeb.Helper;
using GYMappWeb.Helpers;
using GYMappWeb.Interface;
using GYMappWeb.Models;
using GYMappWeb.ViewModels.TblMemberShipType;
using GYMappWeb.ViewModels.TblOffer;
using GYMappWeb.ViewModels.TblUser;
using GYMappWeb.ViewModels.TblUserMemberShip;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GYMappWeb.Service
{
    public class TblUserMemberShipService : ITblUserMemberShip
    {
        private readonly GYMappWebContext _context;

        public TblUserMemberShipService(GYMappWebContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<GetWithPaginationTblUserMemberShipViewModel>> GetAllUserMembershipsAsync(UserParameters userParameters)
        {
            var userNames = await _context.Users
                .Select(u => new { u.Id, u.UserName })
                .ToDictionaryAsync(u => u.Id, u => u.UserName);

            var query = _context.TblUserMemberShips
                .Include(m => m.User)
                .Include(m => m.MemberShipTypes)
                .Include(m => m.Off)
                .AsQueryable();

            // Apply filtering
            if (!string.IsNullOrEmpty(userParameters.SearchTerm))
            {
                query = query.Where(m =>
                    m.User.UserName.Contains(userParameters.SearchTerm) ||
                    m.User.UserCode.ToString().Contains(userParameters.SearchTerm) ||
                    m.MemberShipTypes.Name.Contains(userParameters.SearchTerm));
            }

            if (userParameters.IsActive.HasValue)
            {
                query = query.Where(m => m.IsActive == userParameters.IsActive);
            }

            // Apply sorting
            switch (userParameters.SortBy)
            {
                case "UserName":
                    query = userParameters.SortDescending
                        ? query.OrderByDescending(m => m.User.UserName)
                        : query.OrderBy(m => m.User.UserName);
                    break;
                case "Membership":
                    query = userParameters.SortDescending
                        ? query.OrderByDescending(m => m.MemberShipTypes.Name)
                        : query.OrderBy(m => m.MemberShipTypes.Name);
                    break;
                case "EndDate":
                    query = userParameters.SortDescending
                        ? query.OrderByDescending(m => m.EndDate)
                        : query.OrderBy(m => m.EndDate);
                    break;
                default:
                    query = query.OrderBy(m => m.User.UserCode);
                    break;
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var items = await query
                .Skip((userParameters.PageNumber - 1) * userParameters.PageSize)
                .Take(userParameters.PageSize)
                .Select(m => new GetWithPaginationTblUserMemberShipViewModel
                {
                    UserMemberShipId = m.UserMemberShipId,
                    UserId = m.UserId,
                    User = new TblUserViewModel
                    {
                        UserId = m.User.UserId,
                        UserCode = m.User.UserCode,
                        UserName = m.User.UserName
                    },
                    MemberShipTypesId = m.MemberShipTypesId,
                    MemberShipTypes = new TblMemberShipTypeViewModel
                    {
                        MemberShipTypesId = m.MemberShipTypes.MemberShipTypesId,
                        Name = m.MemberShipTypes.Name
                    },
                    StartDate = m.StartDate,
                    EndDate = m.EndDate,
                    TotalFreezedDays = m.TotalFreezedDays,
                    invitationUsed = m.invitationUsed,
                    IsActive = m.IsActive,
                    CreatedDate = m.CreatedDate,
                    CreatedBy = m.CreatedBy,
                    CreatedByUserName = m.CreatedBy != null && userNames.ContainsKey(m.CreatedBy)
                        ? userNames[m.CreatedBy]
                        : "Unknown",
                    Off = m.Off != null ? new TblOfferViewModel
                    {
                        OffId = m.Off.OffId,
                        OfferName = m.Off.OfferName
                    } : null
                })
                .ToListAsync();

            return new PagedResult<GetWithPaginationTblUserMemberShipViewModel>(
                items,
                totalCount,
                userParameters.PageNumber,
                userParameters.PageSize);
        }

        public async Task UpdateExpiredMembershipsAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var expiredMemberships = await _context.TblUserMemberShips
                .Include(m => m.User)
                .Where(m => m.EndDate <= today && m.IsActive)
                .ToListAsync();

            foreach (var membership in expiredMemberships)
            {
                membership.IsActive = false;

                var hasOtherActive = await _context.TblUserMemberShips
                    .AnyAsync(m => m.UserId == membership.UserId &&
                                 m.UserMemberShipId != membership.UserMemberShipId &&
                                 m.IsActive);

                if (!hasOtherActive && membership.User != null)
                {
                    membership.User.IsActive = false;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> AddMembershipAsync(SaveTblUserMemberShipViewModel model, string createdById)
        {
            if (await _context.TblUserMemberShips
                .AnyAsync(m => m.UserId == model.UserId && m.IsActive))
            {
                throw new Exception("User already has an active membership");
            }

            var user = await _context.TblUsers.FindAsync(model.UserId);
            if (user == null) throw new Exception("User not found");

            user.IsActive = true;
            _context.Update(user);

            var entity = ObjectMapper.Mapper.Map<TblUserMemberShip>(model);
            entity.CreatedBy = createdById;
            entity.CreatedDate = DateTime.Today;
            entity.IsActive = true;

            _context.Add(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteMembershipAsync(int id)
        {
            var membership = await _context.TblUserMemberShips.FindAsync(id);
            if (membership == null) return false;

            _context.Remove(membership);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteFreezesAsync(int userMemberShipId)
        {
            var freezes = await _context.TblMemberShipFreezes
                .Where(f => f.UserMemberShipId == userMemberShipId)
                .ToListAsync();

            _context.RemoveRange(freezes);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HasActiveMembershipAsync(int userId)
        {
            return await _context.TblUserMemberShips
                .AnyAsync(m => m.UserId == userId && m.IsActive);
        }

        public async Task<TblUserMemberShip> GetMembershipByIdAsync(int id)
        {
            return await _context.TblUserMemberShips.FindAsync(id);
        }

    }
}