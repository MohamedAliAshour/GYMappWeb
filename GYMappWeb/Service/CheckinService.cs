using GYMappWeb.Areas.Identity.Data;
using GYMappWeb.Helper;
using GYMappWeb.Interface;
using GYMappWeb.Models;
using GYMappWeb.ViewModels.Checkin;
using GYMappWeb.ViewModels.GymBranch;
using GYMappWeb.ViewModels.InvitedUserRequest;
using GYMappWeb.ViewModels.TblUser;
using GYMappWeb.ViewModels.TblUserMemberShip;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GYMappWeb.Service
{
    public class CheckinService : ICheckin
    {
        private readonly GYMappWebContext _context;
        private readonly ILogging _logger;

        public CheckinService(GYMappWebContext context, ILogging logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<GetCheckinViewModel>> GetAllCheckinsAsync(int gymBranchId)
        {
            try
            {
                var userNames = await _context.Users
                    .Select(u => new { u.Id, u.UserName })
                    .ToDictionaryAsync(u => u.Id, u => u.UserName);

                var checkins = await _context.Checkins
                    .Include(c => c.User)
                    .Include(c => c.GymBranch)
                    .Where(c => c.GymBranchId == gymBranchId)
                    .OrderByDescending(c => c.CheckinDate)
                    .Select(c => new GetCheckinViewModel
                    {
                        CheckinId = c.CheckinId,
                        CheckinDate = c.CheckinDate,
                        UserId = c.UserId,
                        UserName = c.User.UserName,
                        GymBranchId = c.GymBranchId,
                        GymBranchName = c.GymBranch.GymName,
                        CreatedBy = c.CreatedBy,
                        CreatedByUserName = c.CreatedBy != null && userNames.ContainsKey(c.CreatedBy)
                            ? userNames[c.CreatedBy]
                            : "Unknown"
                    })
                    .ToListAsync();

                return checkins;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Failed to get all checkins for GymBranch: {gymBranchId}",
                    ex,
                    nameof(CheckinService),
                    nameof(GetAllCheckinsAsync)
                );
                throw;
            }
        }

        public async Task<PagedResult<GetCheckinViewModel>> GetWithPaginations(UserParameters userParam, int gymBranchId)
        {
            try
            {
                var userNames = await _context.Users
                    .Select(u => new { u.Id, u.UserName })
                    .ToDictionaryAsync(u => u.Id, u => u.UserName);

                var query = _context.Checkins
                    .Include(c => c.User)
                    .Include(c => c.GymBranch)
                    .Where(c => c.GymBranchId == gymBranchId)
                    .AsQueryable();

                // Apply filtering
                if (!string.IsNullOrEmpty(userParam.SearchTerm))
                {
                    query = query.Where(c =>
                        c.User.UserName.Contains(userParam.SearchTerm) ||
                        c.GymBranch.GymName.Contains(userParam.SearchTerm) ||
                        c.CheckinDate.ToString().Contains(userParam.SearchTerm));
                }

                // Apply sorting
                switch (userParam.SortBy)
                {
                    case "UserName":
                        query = userParam.SortDescending
                            ? query.OrderByDescending(c => c.User.UserName)
                            : query.OrderBy(c => c.User.UserName);
                        break;
                    case "GymBranchName":
                        query = userParam.SortDescending
                            ? query.OrderByDescending(c => c.GymBranch.GymName)
                            : query.OrderBy(c => c.GymBranch.GymName);
                        break;
                    case "CheckinDate":
                        query = userParam.SortDescending
                            ? query.OrderByDescending(c => c.CheckinDate)
                            : query.OrderBy(c => c.CheckinDate);
                        break;
                    default:
                        query = query.OrderByDescending(c => c.CheckinDate);
                        break;
                }

                // Get total count before pagination
                var totalCount = await query.CountAsync();

                // Apply pagination
                var items = await query
                    .Skip((userParam.PageNumber - 1) * userParam.PageSize)
                    .Take(userParam.PageSize)
                    .Select(c => new GetCheckinViewModel
                    {
                        CheckinId = c.CheckinId,
                        CheckinDate = c.CheckinDate,
                        UserId = c.UserId,
                        UserName = c.User.UserName,
                        GymBranchId = c.GymBranchId,
                        GymBranchName = c.GymBranch.GymName,
                        CreatedBy = c.CreatedBy,
                        CreatedByUserName = c.CreatedBy != null && userNames.ContainsKey(c.CreatedBy)
                            ? userNames[c.CreatedBy]
                            : "Unknown"
                    })
                    .ToListAsync();

                return new PagedResult<GetCheckinViewModel>(
                    items,
                    totalCount,
                    userParam.PageNumber,
                    userParam.PageSize);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Failed to get paginated checkins. Page: {userParam.PageNumber}, Size: {userParam.PageSize}, Search: {userParam.SearchTerm}",
                    ex,
                    nameof(CheckinService),
                    nameof(GetWithPaginations)
                );
                throw;
            }
        }

        public async Task<bool> Add(SaveCheckinViewModel model, string createdById, int gymBranchId)
        {
            try
            {
                // Verify user belongs to the same gym branch
                var user = await _context.TblUsers
                    .FirstOrDefaultAsync(u => u.UserId == model.UserId && u.GymBranchId == gymBranchId);

                if (user == null)
                {
                    throw new Exception("User not found or doesn't belong to this gym branch");
                }

                // Set CreatedBy and CheckinDate (if not set)
                model.CreatedBy = createdById;
                if (model.CheckinDate == default)
                {
                    model.CheckinDate = DateTime.Now;
                }

                var entity = new Checkin
                {
                    CheckinDate = model.CheckinDate,
                    UserId = model.UserId,
                    GymBranchId = gymBranchId,
                    CreatedBy = model.CreatedBy
                };

                _context.Add(entity);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Failed to add checkin for User: {model.UserId}, GymBranch: {gymBranchId}, CreatedBy: {createdById}",
                    ex,
                    nameof(CheckinService),
                    nameof(Add)
                );
                throw;
            }
        }

        public async Task<bool> CreateCheckinWithInvitationsAsync(SaveCheckinViewModel model, List<InvitedUserRequest> invitedUsers, string createdById, int gymBranchId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Verify main user belongs to the same gym branch
                var mainUser = await _context.TblUsers
                    .FirstOrDefaultAsync(u => u.UserId == model.UserId && u.GymBranchId == gymBranchId);

                if (mainUser == null)
                {
                    throw new Exception("Main user not found or doesn't belong to this gym branch");
                }

                // 1. Create the main checkin
                var mainCheckin = new Checkin
                {
                    CheckinDate = DateTime.Now,
                    UserId = model.UserId,
                    GymBranchId = gymBranchId,
                    CreatedBy = createdById
                };

                _context.Checkins.Add(mainCheckin);

                // 2. Create invited users and their checkins
                foreach (var invitedUser in invitedUsers)
                {
                    // Check if phone already exists in the same gym branch
                    var existingUser = await _context.TblUsers
                        .FirstOrDefaultAsync(u => u.UserPhone == invitedUser.UserPhone && u.GymBranchId == gymBranchId);

                    if (existingUser != null)
                    {
                        throw new Exception($"Phone number {invitedUser.UserPhone} is already registered in this gym branch");
                    }

                    // Get next user code for this gym branch
                    var nextUserCode = await _context.TblUsers
                        .Where(u => u.GymBranchId == gymBranchId)
                        .OrderByDescending(u => u.UserCode)
                        .Select(u => u.UserCode)
                        .FirstOrDefaultAsync() + 1;

                    // Create new user
                    var newUser = new TblUser
                    {
                        UserCode = nextUserCode,
                        UserName = invitedUser.UserName,
                        UserPhone = invitedUser.UserPhone,
                        IsActive = false,
                        CreatedDate = DateTime.Now,
                        CreatedBy = createdById,
                        GymBranchId = gymBranchId
                    };

                    _context.TblUsers.Add(newUser);
                    await _context.SaveChangesAsync();

                    // Create checkin for invited user
                    var invitedCheckin = new Checkin
                    {
                        CheckinDate = DateTime.Now,
                        UserId = newUser.UserId,
                        GymBranchId = gymBranchId,
                        CreatedBy = createdById
                    };

                    _context.Checkins.Add(invitedCheckin);
                }

                // 3. Update the inviting user's invitation count and check limits
                var invitingUserMembership = await _context.TblUserMemberShips
                    .Include(m => m.MemberShipTypes)
                    .Where(m => m.UserId == model.UserId && m.IsActive)
                    .OrderByDescending(m => m.StartDate)
                    .FirstOrDefaultAsync();

                if (invitingUserMembership != null)
                {
                    var newInvitationCount = (invitingUserMembership.invitationUsed ?? 0) + invitedUsers.Count;

                    if (newInvitationCount > invitingUserMembership.MemberShipTypes.invitationCount)
                    {
                        throw new Exception($"Exceeded maximum invitations allowed for this membership. Maximum is {invitingUserMembership.MemberShipTypes.invitationCount}");
                    }

                    invitingUserMembership.invitationUsed = newInvitationCount;
                    _context.TblUserMemberShips.Update(invitingUserMembership);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logger.LogErrorAsync(
                    $"Failed to create checkin with invitations. Main User: {model.UserId}, Invited Users: {invitedUsers.Count}, GymBranch: {gymBranchId}",
                    ex,
                    nameof(CheckinService),
                    nameof(CreateCheckinWithInvitationsAsync)
                );
                throw;
            }
        }

        public async Task<bool> Delete(int id, int gymBranchId)
        {
            try
            {
                var checkin = await _context.Checkins
                    .FirstOrDefaultAsync(c => c.CheckinId == id && c.GymBranchId == gymBranchId);

                if (checkin == null)
                {
                    throw new Exception("Checkin not found or doesn't belong to this gym branch");
                }

                _context.Checkins.Remove(checkin);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Failed to delete checkin. CheckinId: {id}, GymBranch: {gymBranchId}",
                    ex,
                    nameof(CheckinService),
                    nameof(Delete)
                );
                throw;
            }
        }

        public async Task<TblUserViewModel> SearchUserByCodeAsync(int code, int gymBranchId)
        {
            try
            {
                return await _context.TblUserMemberShips
                    .Include(ms => ms.User)
                    .Include(ms => ms.MemberShipTypes)
                    .Where(ms => ms.User.UserCode == code && ms.User.GymBranchId == gymBranchId)
                    .OrderByDescending(ms => ms.StartDate)
                    .Select(ms => new TblUserViewModel
                    {
                        UserId = ms.User.UserId,
                        UserName = ms.User.UserName,
                        UserCode = ms.User.UserCode,
                        UserPhone = ms.User.UserPhone,
                        IsActive = ms.User.IsActive,
                        TblUserMemberShips = new List<TblUserMemberShipViewModel>
                        {
                            new TblUserMemberShipViewModel
                            {
                                MembershipName = ms.MemberShipTypes.Name,
                                IsActive = ms.IsActive,
                                StartDate = ms.StartDate,
                                EndDate = ms.EndDate,
                                invitationUsed = ms.invitationUsed,
                                MaxInvitations = ms.MemberShipTypes.invitationCount
                            }
                        }
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Failed to search user by code. Code: {code}, GymBranch: {gymBranchId}",
                    ex,
                    nameof(CheckinService),
                    nameof(SearchUserByCodeAsync)
                );
                throw;
            }
        }

        public async Task<TblUserViewModel> SearchUserByPhoneAsync(string phone, int gymBranchId)
        {
            try
            {
                return await _context.TblUserMemberShips
                    .Include(ms => ms.User)
                    .Include(ms => ms.MemberShipTypes)
                    .Where(ms => ms.User.UserPhone == phone && ms.User.GymBranchId == gymBranchId)
                    .OrderByDescending(ms => ms.StartDate)
                    .Select(ms => new TblUserViewModel
                    {
                        UserId = ms.User.UserId,
                        UserName = ms.User.UserName,
                        UserCode = ms.User.UserCode,
                        UserPhone = ms.User.UserPhone,
                        IsActive = ms.User.IsActive,
                        TblUserMemberShips = new List<TblUserMemberShipViewModel>
                        {
                            new TblUserMemberShipViewModel
                            {
                                MembershipName = ms.MemberShipTypes.Name,
                                IsActive = ms.IsActive,
                                StartDate = ms.StartDate,
                                EndDate = ms.EndDate,
                                invitationUsed = ms.invitationUsed,
                                MaxInvitations = ms.MemberShipTypes.invitationCount
                            }
                        }
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Failed to search user by phone. Phone: {phone}, GymBranch: {gymBranchId}",
                    ex,
                    nameof(CheckinService),
                    nameof(SearchUserByPhoneAsync)
                );
                throw;
            }
        }

        public async Task<bool> CheckPhoneExistsAsync(string phone, int gymBranchId)
        {
            try
            {
                return await _context.TblUsers.AnyAsync(u => u.UserPhone == phone && u.GymBranchId == gymBranchId);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Failed to check phone existence. Phone: {phone}, GymBranch: {gymBranchId}",
                    ex,
                    nameof(CheckinService),
                    nameof(CheckPhoneExistsAsync)
                );
                throw;
            }
        }

        public SaveCheckinViewModel GetDetailsById(int id, int gymBranchId)
        {
            try
            {
                var checkin = _context.Checkins
                    .Include(c => c.User)
                    .Include(c => c.GymBranch)
                    .FirstOrDefault(c => c.CheckinId == id && c.GymBranchId == gymBranchId);

                if (checkin == null)
                    return null;

                return new SaveCheckinViewModel
                {
                    CheckinId = checkin.CheckinId,
                    CheckinDate = checkin.CheckinDate,
                    UserId = checkin.UserId,
                    UserName = checkin.User?.UserName,
                    GymBranchId = checkin.GymBranchId,
                    GymBranchName = checkin.GymBranch?.GymName,
                    CreatedBy = checkin.CreatedBy
                };
            }
            catch (Exception ex)
            {
                _logger.LogErrorAsync(
                    $"Failed to get checkin details by ID. CheckinId: {id}, GymBranch: {gymBranchId}",
                    ex,
                    nameof(CheckinService),
                    nameof(GetDetailsById)
                ).Wait();
                throw;
            }
        }

        public async Task<GetCheckinViewModel> GetCheckinDetailsAsync(int id, int gymBranchId)
        {
            try
            {
                var userNames = await _context.Users
                    .Select(u => new { u.Id, u.UserName })
                    .ToDictionaryAsync(u => u.Id, u => u.UserName);

                var checkin = await _context.Checkins
                    .Include(c => c.User)
                    .Include(c => c.GymBranch)
                    .Where(c => c.CheckinId == id && c.GymBranchId == gymBranchId)
                    .Select(c => new GetCheckinViewModel
                    {
                        CheckinId = c.CheckinId,
                        CheckinDate = c.CheckinDate,
                        UserId = c.UserId,
                        UserName = c.User.UserName,
                        GymBranchId = c.GymBranchId,
                        GymBranchName = c.GymBranch.GymName,
                        CreatedBy = c.CreatedBy,
                        CreatedByUserName = c.CreatedBy != null && userNames.ContainsKey(c.CreatedBy)
                            ? userNames[c.CreatedBy]
                            : "Unknown"
                    })
                    .FirstOrDefaultAsync();

                return checkin;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Failed to get checkin details. CheckinId: {id}, GymBranch: {gymBranchId}",
                    ex,
                    nameof(CheckinService),
                    nameof(GetCheckinDetailsAsync)
                );
                throw;
            }
        }

        public async Task<List<SelectListItem>> GetUsersSelectList(int gymBranchId)
        {
            try
            {
                return await _context.TblUsers
                    .Where(u => u.IsActive && u.GymBranchId == gymBranchId)
                    .OrderBy(u => u.UserName)
                    .Select(u => new SelectListItem
                    {
                        Value = u.UserId.ToString(),
                        Text = u.UserName
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Failed to get users select list. GymBranch: {gymBranchId}",
                    ex,
                    nameof(CheckinService),
                    nameof(GetUsersSelectList)
                );
                throw;
            }
        }

        public async Task<List<SelectListItem>> GetGymBranchesSelectList()
        {
            try
            {
                return await _context.GymBranches
                    .OrderBy(g => g.GymName)
                    .Select(g => new SelectListItem
                    {
                        Value = g.GymBranchId.ToString(),
                        Text = g.GymName
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    "Failed to get gym branches select list",
                    ex,
                    nameof(CheckinService),
                    nameof(GetGymBranchesSelectList)
                );
                throw;
            }
        }

        public async Task<bool> IsUserCheckedInAsync(int userId, int gymBranchId)
        {
            try
            {
                // Check if user has an active check-in (within the last 3 hours) in the same gym branch
                var threeHoursAgo = DateTime.Now.AddHours(-3);
                var existingCheckin = await _context.Checkins
                    .Where(c => c.UserId == userId && c.GymBranchId == gymBranchId && c.CheckinDate >= threeHoursAgo)
                    .FirstOrDefaultAsync();

                return existingCheckin != null;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Failed to check if user is checked in. UserId: {userId}, GymBranch: {gymBranchId}",
                    ex,
                    nameof(CheckinService),
                    nameof(IsUserCheckedInAsync)
                );
                throw;
            }
        }
    }
}