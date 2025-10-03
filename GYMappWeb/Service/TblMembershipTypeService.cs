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
        private readonly ILogging _logger;

        public TblMembershipTypeService(GYMappWebContext context, ILogging logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PagedResult<GetWithPaginationTblMemberShipTypeViewModel>> GetAllMembershipTypesAsync(UserParameters userParameters, int gymBranchId)
        {
            try
            {
                var userNames = await _context.Users
                    .Select(u => new { u.Id, u.UserName })
                    .ToDictionaryAsync(u => u.Id, u => u.UserName);

                var query = _context.TblMembershipTypes
                    .Where(m => m.GymBranchId == gymBranchId)
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
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Failed to get all membership types. GymBranch: {gymBranchId}, Page: {userParameters.PageNumber}, Search: {userParameters.SearchTerm}",
                    ex,
                    nameof(TblMembershipTypeService),
                    nameof(GetAllMembershipTypesAsync)
                );
                throw;
            }
        }

        public async Task<bool> AddMembershipTypeAsync(SaveTblMemberShipTypeViewModel model, string createdById, int gymBranchId)
        {
            try
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
                model.GymBranchId = gymBranchId;

                var entity = ObjectMapper.Mapper.Map<TblMembershipType>(model);
                _context.Add(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Failed to add membership type. Name: {model.Name}, Price: {model.Price}, Duration: {model.MembershipDuration}, CreatedBy: {createdById}",
                    ex,
                    nameof(TblMembershipTypeService),
                    nameof(AddMembershipTypeAsync)
                );
                throw;
            }
        }

        public async Task<bool> UpdateMembershipTypeAsync(SaveTblMemberShipTypeViewModel model, int id, string updatedById, int gymBranchId)
        {
            try
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
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Failed to update membership type. ID: {id}, Name: {model.Name}, UpdatedBy: {updatedById}, GymBranch: {gymBranchId}",
                    ex,
                    nameof(TblMembershipTypeService),
                    nameof(UpdateMembershipTypeAsync)
                );
                throw;
            }
        }

        public async Task<bool> DeleteMembershipTypeAsync(int id, int gymBranchId)
        {
            try
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
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Failed to delete membership type. ID: {id}, GymBranch: {gymBranchId}",
                    ex,
                    nameof(TblMembershipTypeService),
                    nameof(DeleteMembershipTypeAsync)
                );
                throw;
            }
        }

        public async Task<bool> HasRelatedMembershipsAsync(int membershipTypeId, int gymBranchId)
        {
            try
            {
                return await _context.TblUserMemberShips
                    .Include(m => m.MemberShipTypes)
                    .AnyAsync(m => m.MemberShipTypesId == membershipTypeId &&
                                 m.MemberShipTypes.GymBranchId == gymBranchId);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Failed to check related memberships. MembershipTypeId: {membershipTypeId}, GymBranch: {gymBranchId}",
                    ex,
                    nameof(TblMembershipTypeService),
                    nameof(HasRelatedMembershipsAsync)
                );
                throw;
            }
        }

        public async Task<SaveTblMemberShipTypeViewModel> GetMembershipTypeDetailsAsync(int id, int gymBranchId)
        {
            try
            {
                var membershipType = await _context.TblMembershipTypes
                    .FirstOrDefaultAsync(m => m.MemberShipTypesId == id && m.GymBranchId == gymBranchId);

                if (membershipType == null)
                {
                    throw new Exception("Membership Type not found or doesn't belong to this gym branch");
                }

                return ObjectMapper.Mapper.Map<SaveTblMemberShipTypeViewModel>(membershipType);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Failed to get membership type details. ID: {id}, GymBranch: {gymBranchId}",
                    ex,
                    nameof(TblMembershipTypeService),
                    nameof(GetMembershipTypeDetailsAsync)
                );
                throw;
            }
        }

        public async Task<bool> ToggleMembershipTypeStatusAsync(int id, int gymBranchId)
        {
            try
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
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Failed to toggle membership type status. ID: {id}, GymBranch: {gymBranchId}",
                    ex,
                    nameof(TblMembershipTypeService),
                    nameof(ToggleMembershipTypeStatusAsync)
                );
                throw;
            }
        }
    }
}