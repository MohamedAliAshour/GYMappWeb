using GYMappWeb.Areas.Identity.Data;
using GYMappWeb.Helper;
using GYMappWeb.Helpers;
using GYMappWeb.Interface;
using GYMappWeb.Models;
using GYMappWeb.ViewModels.TblUser;
using Microsoft.EntityFrameworkCore;

namespace GYMappWeb.Services
{
    public class TblUserService : ITblUser
    {
      private readonly GYMappWebContext _context;

        public TblUserService(GYMappWebContext context)
        {
            _context = context;
        }

        public async Task<List<GetWithPaginationTblUserViewModel>> GetAllUsersAsync()
        {
            var userNames = await _context.Users
                .Select(u => new { u.Id, u.UserName })
                .ToDictionaryAsync(u => u.Id, u => u.UserName);

            var users = await _context.TblUsers
                .OrderBy(u => u.UserCode)
                .ThenBy(u => u.UserName)
                .Select(u => new GetWithPaginationTblUserViewModel
                {
                    UserId = u.UserId,
                    UserCode = u.UserCode,
                    UserName = u.UserName,
                    UserPhone = u.UserPhone,
                    IsActive = u.IsActive,
                    Notes = u.Notes,
                    CreatedDate = u.CreatedDate,
                    CreatedBy = u.CreatedBy,
                    CreatedByUserName = u.CreatedBy != null && userNames.ContainsKey(u.CreatedBy)
                        ? userNames[u.CreatedBy]
                        : "Unknown"
                })
                .ToListAsync();

            return users;
        }

        public async Task<bool> Add(SaveTblUserViewModel model, string createdById)
        {
            // Check for duplicate username
            bool usernameExists = await _context.TblUsers
                .AnyAsync(u => u.UserName.ToLower() == model.UserName.ToLower());

            if (usernameExists)
            {
                throw new Exception("A user with this name already exists.");
            }

            // Check for duplicate phone number
            bool phoneExists = await _context.TblUsers
                .AnyAsync(u => u.UserPhone == model.UserPhone);

            if (phoneExists)
            {
                throw new Exception("This phone number is already registered.");
            }

            // Set CreatedBy and CreatedDate
            model.CreatedBy = createdById;
            model.CreatedDate = DateTime.Today;

            var entity = ObjectMapper.Mapper.Map<TblUser>(model);
            _context.Add(entity);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> Update(SaveTblUserViewModel model, int id, string updatedById)
        {
            if (id != model.UserId)
            {
                throw new Exception("User ID mismatch");
            }

            var existingUser = await _context.TblUsers.FindAsync(id);
            if (existingUser == null)
            {
                throw new Exception("User not found");
            }

            // Update properties
            existingUser.UserName = model.UserName;
            existingUser.UserPhone = model.UserPhone;
            existingUser.IsActive = model.IsActive;
            existingUser.Notes = model.Notes;
            existingUser.CreatedBy = updatedById;
            existingUser.CreatedDate = DateTime.Today;

            _context.Update(existingUser);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> Delete(int id)
        {
            var user = await _context.TblUsers.FindAsync(id);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            _context.TblUsers.Remove(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteRelatedRecords(int id)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                // Delete all freezes related to this user's memberships
                var memberships = await _context.TblUserMemberShips
                    .Where(m => m.UserId == id)
                    .ToListAsync();

                foreach (var membership in memberships)
                {
                    var freezes = await _context.TblMemberShipFreezes
                        .Where(f => f.UserMemberShipId == membership.UserMemberShipId)
                        .ToListAsync();

                    _context.TblMemberShipFreezes.RemoveRange(freezes);
                }

                // Delete all memberships for this user
                _context.TblUserMemberShips.RemoveRange(memberships);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> CheckNameExist(string name)
        {
            return await _context.TblUsers.AnyAsync(u => u.UserName.ToLower() == name.ToLower());
        }

        public async Task<bool> CheckPhoneExist(string phone)
        {
            return await _context.TblUsers.AnyAsync(u => u.UserPhone == phone);
        }

        public async Task<int> GetNextUserCode()
        {
            var lastUser = await _context.TblUsers
                .OrderByDescending(u => u.UserCode)
                .FirstOrDefaultAsync();

            return lastUser != null ? lastUser.UserCode + 1 : 1;
        }

        public TblUser GetById(int id)
        {
            return _context.TblUsers.Find(id);
        }

        public SaveTblUserViewModel GetDetailsById(int id)
        {
            var tblUser = _context.TblUsers.Find(id);
            return ObjectMapper.Mapper.Map<SaveTblUserViewModel>(tblUser);
        }

        public async Task<GetWithPaginationTblUserViewModel> GetUserDetailsAsync(int id)
        {
            var userNames = await _context.Users
                .Select(u => new { u.Id, u.UserName })
                .ToDictionaryAsync(u => u.Id, u => u.UserName);

            var user = await _context.TblUsers
                .Where(u => u.UserId == id)
                .Select(u => new GetWithPaginationTblUserViewModel
                {
                    UserId = u.UserId,
                    UserCode = u.UserCode,
                    UserName = u.UserName,
                    UserPhone = u.UserPhone,
                    IsActive = u.IsActive,
                    Notes = u.Notes,
                    CreatedDate = u.CreatedDate,
                    CreatedBy = u.CreatedBy,
                    CreatedByUserName = u.CreatedBy != null && userNames.ContainsKey(u.CreatedBy)
                        ? userNames[u.CreatedBy]
                        : "Unknown"
                })
                .FirstOrDefaultAsync();

            return user;
        }

        public async Task<PagedResult<GetWithPaginationTblUserViewModel>> GetWithPaginations(UserParameters userParameters)
        {
            var userNames = await _context.Users
                .Select(u => new { u.Id, u.UserName })
                .ToDictionaryAsync(u => u.Id, u => u.UserName);

            var query = _context.TblUsers.AsQueryable();

            // Apply filtering
            if (!string.IsNullOrEmpty(userParameters.SearchTerm))
            {
                query = query.Where(u =>
                    u.UserName.Contains(userParameters.SearchTerm) ||
                    u.UserPhone.Contains(userParameters.SearchTerm) ||
                    u.UserCode.ToString().Contains(userParameters.SearchTerm));
            }

            if (userParameters.IsActive.HasValue)
            {
                query = query.Where(u => u.IsActive == userParameters.IsActive);
            }

            // Apply sorting
            switch (userParameters.SortBy)
            {
                case "UserCode":
                    query = userParameters.SortDescending
                        ? query.OrderByDescending(u => u.UserCode)
                        : query.OrderBy(u => u.UserCode);
                    break;
                case "UserName":
                    query = userParameters.SortDescending
                        ? query.OrderByDescending(u => u.UserName)
                        : query.OrderBy(u => u.UserName);
                    break;
                case "CreatedDate":
                    query = userParameters.SortDescending
                        ? query.OrderByDescending(u => u.CreatedDate)
                        : query.OrderBy(u => u.CreatedDate);
                    break;
                default:
                    query = query.OrderBy(u => u.UserCode);
                    break;
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var items = await query
                .Skip((userParameters.PageNumber - 1) * userParameters.PageSize)
                .Take(userParameters.PageSize)
                .Select(u => new GetWithPaginationTblUserViewModel
                {
                    UserId = u.UserId,
                    UserCode = u.UserCode,
                    UserName = u.UserName,
                    UserPhone = u.UserPhone,
                    IsActive = u.IsActive,
                    Notes = u.Notes,
                    CreatedDate = u.CreatedDate,
                    CreatedBy = u.CreatedBy,
                    CreatedByUserName = u.CreatedBy != null && userNames.ContainsKey(u.CreatedBy)
                        ? userNames[u.CreatedBy]
                        : "Unknown"
                })
                .ToListAsync();

            return new PagedResult<GetWithPaginationTblUserViewModel>(
                items,
                totalCount,
                userParameters.PageNumber,
                userParameters.PageSize);
        }
    }
}