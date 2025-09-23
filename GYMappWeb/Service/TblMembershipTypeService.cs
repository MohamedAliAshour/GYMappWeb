using GYMappWeb.Areas.Identity.Data;
using GYMappWeb.Helper;
using GYMappWeb.Helpers;
using GYMappWeb.Interface;
using GYMappWeb.Models;
using GYMappWeb.ViewModels.TblMemberShipType;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GYMappWeb.Service
{
    public class TblMembershipTypeService : ITblMembershipType
    {
        private readonly GYMappWebContext _context;

        public TblMembershipTypeService(GYMappWebContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<GetWithPaginationTblMemberShipTypeViewModel>> GetAllMembershipTypesAsync(UserParameters userParameters, int gymBranchId)
        {
            var userNames = await _context.Users
                .Select(u => new { u.Id, u.UserName })
                .ToDictionaryAsync(u => u.Id, u => u.UserName);

            var query = _context.TblMembershipTypes
                .Where(m => m.GymBranchId == gymBranchId) // Filter by gym branch
                .AsQueryable();

            // Apply filtering
            if (!string.IsNullOrEmpty(userParameters.SearchTerm))
            {
                query = query.Where(m =>
                    m.Name.Contains(userParameters.SearchTerm) ||
                    m.Description.Contains(userParameters.SearchTerm) ||
                    m.Price.ToString().Contains(userParameters.SearchTerm));
            }

            // Apply sorting
            switch (userParameters.SortBy)
            {
                case "Name":
                    query = userParameters.SortDescending
                        ? query.OrderByDescending(m => m.Name)
                        : query.OrderBy(m => m.Name);
                    break;
                case "Price":
                    query = userParameters.SortDescending
                        ? query.OrderByDescending(m => m.Price)
                        : query.OrderBy(m => m.Price);
                    break;
                case "Duration":
                    query = userParameters.SortDescending
                        ? query.OrderByDescending(m => m.MembershipDuration)
                        : query.OrderBy(m => m.MembershipDuration);
                    break;
                default:
                    query = query.OrderBy(m => m.Name);
                    break;
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var items = await query
                .Skip((userParameters.PageNumber - 1) * userParameters.PageSize)
                .Take(userParameters.PageSize)
                .Select(m => new GetWithPaginationTblMemberShipTypeViewModel
                {
                    MemberShipTypesId = m.MemberShipTypesId,
                    Name = m.Name,
                    MembershipDuration = m.MembershipDuration,
                    Price = m.Price,
                    invitationCount = m.invitationCount,
                    Description = m.Description,
                    FreezeCount = m.FreezeCount,
                    TotalFreezeDays = m.TotalFreezeDays,
                    CreatedDate = m.CreatedDate,
                    CreatedBy = m.CreatedBy,
                    CreatedByUserName = m.CreatedBy != null && userNames.ContainsKey(m.CreatedBy)
                        ? userNames[m.CreatedBy]
                        : "Unknown",
                    IsActive = m.IsActive
                })
                .ToListAsync();

            return new PagedResult<GetWithPaginationTblMemberShipTypeViewModel>(
                items,
                totalCount,
                userParameters.PageNumber,
                userParameters.PageSize);
        }

        public async Task<bool> AddMembershipTypeAsync(SaveTblMemberShipTypeViewModel model, string createdById, int gymBranchId)
        {
            // Check for duplicate membership type name in the same gym branch
            bool nameExists = await _context.TblMembershipTypes
                .AnyAsync(m => m.Name.ToLower() == model.Name.ToLower() && m.GymBranchId == gymBranchId);

            if (nameExists)
            {
                throw new Exception("A membership type with this name already exists in this gym branch.");
            }

            model.CreatedBy = createdById;
            model.CreatedDate = DateTime.Today;
            model.IsActive = true;
            model.GymBranchId = gymBranchId; // Set gym branch ID

            var entity = ObjectMapper.Mapper.Map<TblMembershipType>(model);
            _context.Add(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateMembershipTypeAsync(SaveTblMemberShipTypeViewModel model, int id, string updatedById, int gymBranchId)
        {
            if (id != model.MemberShipTypesId)
            {
                throw new Exception("Membership Type ID mismatch");
            }

            var existingMembershipType = await _context.TblMembershipTypes
                .FirstOrDefaultAsync(m => m.MemberShipTypesId == id && m.GymBranchId == gymBranchId);

            if (existingMembershipType == null)
            {
                throw new Exception("Membership Type not found or doesn't belong to this gym branch");
            }

            // Check for duplicate name (excluding current record)
            bool nameExists = await _context.TblMembershipTypes
                .AnyAsync(m => m.Name.ToLower() == model.Name.ToLower() &&
                             m.MemberShipTypesId != id &&
                             m.GymBranchId == gymBranchId);

            if (nameExists)
            {
                throw new Exception("A membership type with this name already exists in this gym branch.");
            }

            existingMembershipType.Name = model.Name;
            existingMembershipType.MembershipDuration = model.MembershipDuration;
            existingMembershipType.Price = model.Price;
            existingMembershipType.invitationCount = model.invitationCount;
            existingMembershipType.Description = model.Description;
            existingMembershipType.FreezeCount = model.FreezeCount;
            existingMembershipType.TotalFreezeDays = model.TotalFreezeDays;
            existingMembershipType.CreatedBy = updatedById;
            existingMembershipType.CreatedDate = DateTime.Today;
            existingMembershipType.IsActive = model.IsActive;

            _context.Update(existingMembershipType);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteMembershipTypeAsync(int id, int gymBranchId)
        {
            var membershipType = await _context.TblMembershipTypes
                .FirstOrDefaultAsync(m => m.MemberShipTypesId == id && m.GymBranchId == gymBranchId);

            if (membershipType == null)
            {
                throw new Exception("Membership Type not found or doesn't belong to this gym branch");
            }

            _context.TblMembershipTypes.Remove(membershipType);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HasRelatedMembershipsAsync(int membershipTypeId, int gymBranchId)
        {
            return await _context.TblUserMemberShips
                .Include(m => m.MemberShipTypes)
                .AnyAsync(m => m.MemberShipTypesId == membershipTypeId &&
                             m.MemberShipTypes.GymBranchId == gymBranchId);
        }

        public async Task<SaveTblMemberShipTypeViewModel> GetMembershipTypeDetailsAsync(int id, int gymBranchId)
        {
            var membershipType = await _context.TblMembershipTypes
                .FirstOrDefaultAsync(m => m.MemberShipTypesId == id && m.GymBranchId == gymBranchId);

            if (membershipType == null)
            {
                throw new Exception("Membership Type not found or doesn't belong to this gym branch");
            }

            return ObjectMapper.Mapper.Map<SaveTblMemberShipTypeViewModel>(membershipType);
        }

        public async Task<bool> ToggleMembershipTypeStatusAsync(int id, int gymBranchId)
        {
            var membershipType = await _context.TblMembershipTypes
                .FirstOrDefaultAsync(m => m.MemberShipTypesId == id && m.GymBranchId == gymBranchId);

            if (membershipType == null)
            {
                throw new Exception("Membership Type not found or doesn't belong to this gym branch");
            }

            // Toggle the IsActive status
            membershipType.IsActive = !membershipType.IsActive;

            _context.Update(membershipType);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}