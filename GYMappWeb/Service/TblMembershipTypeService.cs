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

        public async Task<PagedResult<GetWithPaginationTblMemberShipTypeViewModel>> GetAllMembershipTypesAsync(UserParameters userParameters)
        {
            var userNames = await _context.Users
                .Select(u => new { u.Id, u.UserName })
                .ToDictionaryAsync(u => u.Id, u => u.UserName);

            var query = _context.TblMembershipTypes.AsQueryable();

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
                        : "Unknown"
                })
                .ToListAsync();

            return new PagedResult<GetWithPaginationTblMemberShipTypeViewModel>(
                items,
                totalCount,
                userParameters.PageNumber,
                userParameters.PageSize);
        }

        public async Task<bool> AddMembershipTypeAsync(SaveTblMemberShipTypeViewModel model, string createdById)
        {
            model.CreatedBy = createdById;
            model.CreatedDate = DateTime.Today;

            var entity = ObjectMapper.Mapper.Map<TblMembershipType>(model);
            _context.Add(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateMembershipTypeAsync(SaveTblMemberShipTypeViewModel model, int id, string updatedById)
        {
            if (id != model.MemberShipTypesId)
            {
                throw new Exception("Membership Type ID mismatch");
            }

            var existingMembershipType = await _context.TblMembershipTypes.FindAsync(id);
            if (existingMembershipType == null)
            {
                throw new Exception("Membership Type not found");
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

            _context.Update(existingMembershipType);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteMembershipTypeAsync(int id)
        {
            var membershipType = await _context.TblMembershipTypes.FindAsync(id);
            if (membershipType == null)
            {
                throw new Exception("Membership Type not found");
            }

            _context.TblMembershipTypes.Remove(membershipType);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HasRelatedMembershipsAsync(int membershipTypeId)
        {
            return await _context.TblUserMemberShips
                .AnyAsync(m => m.MemberShipTypesId == membershipTypeId);
        }

        public async Task<SaveTblMemberShipTypeViewModel> GetMembershipTypeDetailsAsync(int id)
        {
            var membershipType = await _context.TblMembershipTypes.FindAsync(id);
            return ObjectMapper.Mapper.Map<SaveTblMemberShipTypeViewModel>(membershipType);
        }
    }
}