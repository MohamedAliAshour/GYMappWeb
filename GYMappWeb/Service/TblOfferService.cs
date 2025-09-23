using GYMappWeb.Areas.Identity.Data;
using GYMappWeb.Helper;
using GYMappWeb.Helpers;
using GYMappWeb.Interface;
using GYMappWeb.Models;
using GYMappWeb.ViewModels.TblOffer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GYMappWeb.Service
{
    public class TblOfferService : ITblOffer
    {
        private readonly GYMappWebContext _context;

        public TblOfferService(GYMappWebContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<GetWithPaginationTblOfferViewModel>> GetAllOffersAsync(UserParameters userParameters, int gymBranchId)
        {
            var userNames = await _context.Users
                .Select(u => new { u.Id, u.UserName })
                .ToDictionaryAsync(u => u.Id, u => u.UserName);

            var query = _context.TblOffers
                .Include(o => o.MemberShipTypes)
                .Where(o => o.GymBranchId == gymBranchId) // Filter by gym branch
                .AsQueryable();

            // Apply filtering
            if (!string.IsNullOrEmpty(userParameters.SearchTerm))
            {
                query = query.Where(o =>
                    o.OfferName.Contains(userParameters.SearchTerm) ||
                    o.MemberShipTypes.Name.Contains(userParameters.SearchTerm) ||
                    o.DiscountPrecentage.ToString().Contains(userParameters.SearchTerm));
            }

            // Apply sorting
            switch (userParameters.SortBy)
            {
                case "Name":
                    query = userParameters.SortDescending
                        ? query.OrderByDescending(o => o.OfferName)
                        : query.OrderBy(o => o.OfferName);
                    break;
                case "Discount":
                    query = userParameters.SortDescending
                        ? query.OrderByDescending(o => o.DiscountPrecentage)
                        : query.OrderBy(o => o.DiscountPrecentage);
                    break;
                case "Membership":
                    query = userParameters.SortDescending
                        ? query.OrderByDescending(o => o.MemberShipTypes.Name)
                        : query.OrderBy(o => o.MemberShipTypes.Name);
                    break;
                default:
                    query = query.OrderBy(o => o.OfferName);
                    break;
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var items = await query
                .Skip((userParameters.PageNumber - 1) * userParameters.PageSize)
                .Take(userParameters.PageSize)
                .Select(o => new GetWithPaginationTblOfferViewModel
                {
                    OffId = o.OffId,
                    OfferName = o.OfferName,
                    DiscountPrecentage = o.DiscountPrecentage,
                    MemberShipTypesId = o.MemberShipTypesId,
                    CreatedBy = o.CreatedBy,
                    CreatedDate = o.CreatedDate,
                    CreatedByUserName = o.CreatedBy != null && userNames.ContainsKey(o.CreatedBy)
                        ? userNames[o.CreatedBy]
                        : "Unknown",
                    MemberShipTypes = o.MemberShipTypes,
                    IsActive = o.IsActive
                })
                .ToListAsync();

            return new PagedResult<GetWithPaginationTblOfferViewModel>(
                items,
                totalCount,
                userParameters.PageNumber,
                userParameters.PageSize);
        }

        public async Task<bool> AddOfferAsync(SaveTblOfferViewModel model, string createdById, int gymBranchId)
        {
            // Check for duplicate offer name in the same gym branch
            bool nameExists = await _context.TblOffers
                .AnyAsync(o => o.OfferName.ToLower() == model.OfferName.ToLower() && o.GymBranchId == gymBranchId);

            if (nameExists)
            {
                throw new Exception("An offer with this name already exists in this gym branch.");
            }

            model.CreatedBy = createdById;
            model.CreatedDate = DateTime.Today;
            model.IsActive = true;
            model.GymBranchId = gymBranchId; // Set gym branch ID

            var entity = ObjectMapper.Mapper.Map<TblOffer>(model);
            _context.Add(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateOfferAsync(SaveTblOfferViewModel model, int id, string updatedById, int gymBranchId)
        {
            if (id != model.OffId)
            {
                throw new Exception("Offer ID mismatch");
            }

            var existingOffer = await _context.TblOffers
                .FirstOrDefaultAsync(o => o.OffId == id && o.GymBranchId == gymBranchId);

            if (existingOffer == null)
            {
                throw new Exception("Offer not found or doesn't belong to this gym branch");
            }

            // Check for duplicate name (excluding current record)
            bool nameExists = await _context.TblOffers
                .AnyAsync(o => o.OfferName.ToLower() == model.OfferName.ToLower() &&
                             o.OffId != id &&
                             o.GymBranchId == gymBranchId);

            if (nameExists)
            {
                throw new Exception("An offer with this name already exists in this gym branch.");
            }

            existingOffer.OfferName = model.OfferName;
            existingOffer.DiscountPrecentage = model.DiscountPrecentage;
            existingOffer.MemberShipTypesId = model.MemberShipTypesId;
            existingOffer.CreatedBy = updatedById;
            existingOffer.CreatedDate = DateTime.Today;
            existingOffer.IsActive = model.IsActive;

            _context.Update(existingOffer);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteOfferAsync(int id, int gymBranchId)
        {
            var offer = await _context.TblOffers
                .FirstOrDefaultAsync(o => o.OffId == id && o.GymBranchId == gymBranchId);

            if (offer == null)
            {
                throw new Exception("Offer not found or doesn't belong to this gym branch");
            }

            _context.TblOffers.Remove(offer);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HasRelatedMembershipsAsync(int offerId, int gymBranchId)
        {
            return await _context.TblUserMemberShips
                .Include(m => m.Off)
                .AnyAsync(m => m.OffId == offerId && m.Off.GymBranchId == gymBranchId);
        }

        public async Task<TblOffer> GetOfferByIdAsync(int id, int gymBranchId)
        {
            return await _context.TblOffers
                .FirstOrDefaultAsync(o => o.OffId == id && o.GymBranchId == gymBranchId);
        }

        public async Task<SaveTblOfferViewModel> GetOfferDetailsByIdAsync(int id, int gymBranchId)
        {
            var offer = await _context.TblOffers
                .FirstOrDefaultAsync(o => o.OffId == id && o.GymBranchId == gymBranchId);

            if (offer == null)
            {
                throw new Exception("Offer not found or doesn't belong to this gym branch");
            }

            return ObjectMapper.Mapper.Map<SaveTblOfferViewModel>(offer);
        }

        public async Task<bool> ToggleOfferStatusAsync(int id, int gymBranchId)
        {
            var offer = await _context.TblOffers
                .FirstOrDefaultAsync(o => o.OffId == id && o.GymBranchId == gymBranchId);

            if (offer == null)
            {
                throw new Exception("Offer not found or doesn't belong to this gym branch");
            }

            // Toggle the IsActive status
            offer.IsActive = !offer.IsActive;

            _context.Update(offer);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}