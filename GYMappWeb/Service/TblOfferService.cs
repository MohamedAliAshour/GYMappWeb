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

        public async Task<PagedResult<GetWithPaginationTblOfferViewModel>> GetAllOffersAsync(UserParameters userParameters)
        {
            var userNames = await _context.Users
                .Select(u => new { u.Id, u.UserName })
                .ToDictionaryAsync(u => u.Id, u => u.UserName);

            var query = _context.TblOffers
                .Include(o => o.MemberShipTypes)
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
                    MemberShipTypes = o.MemberShipTypes
                })
                .ToListAsync();

            return new PagedResult<GetWithPaginationTblOfferViewModel>(
                items,
                totalCount,
                userParameters.PageNumber,
                userParameters.PageSize);
        }

        public async Task<bool> AddOfferAsync(SaveTblOfferViewModel model, string createdById)
        {
            model.CreatedBy = createdById;
            model.CreatedDate = DateTime.Today;

            var entity = ObjectMapper.Mapper.Map<TblOffer>(model);
            _context.Add(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateOfferAsync(SaveTblOfferViewModel model, int id, string updatedById)
        {
            if (id != model.OffId)
            {
                throw new Exception("Offer ID mismatch");
            }

            var existingOffer = await _context.TblOffers.FindAsync(id);
            if (existingOffer == null)
            {
                throw new Exception("Offer not found");
            }

            existingOffer.OfferName = model.OfferName;
            existingOffer.DiscountPrecentage = model.DiscountPrecentage;
            existingOffer.MemberShipTypesId = model.MemberShipTypesId;
            existingOffer.CreatedBy = updatedById;
            existingOffer.CreatedDate = DateTime.Today;

            _context.Update(existingOffer);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteOfferAsync(int id)
        {
            var offer = await _context.TblOffers.FindAsync(id);
            if (offer == null)
            {
                throw new Exception("Offer not found");
            }

            _context.TblOffers.Remove(offer);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HasRelatedMembershipsAsync(int offerId)
        {
            return await _context.TblUserMemberShips
                .AnyAsync(m => m.OffId == offerId);
        }

        public async Task<TblOffer> GetOfferByIdAsync(int id)
        {
            return await _context.TblOffers.FindAsync(id);
        }

        public async Task<SaveTblOfferViewModel> GetOfferDetailsByIdAsync(int id)
        {
            var offer = await _context.TblOffers.FindAsync(id);
            return ObjectMapper.Mapper.Map<SaveTblOfferViewModel>(offer);
        }
    }
}