using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB;
using BrandService.Interfaces;
using BrandService.Models.Entities;
using BrandService.Models.Domain;
using MicroServices.DataAccess.Interfaces;

namespace PlatformService.Classes
{
    public class BrandDataProcessor : IBrandDataProcessor
    {
        private readonly IConnection _database;

        public ITable<BrandEntity> Brands => _database.GetTable<BrandEntity>();
        public ITable<BrandCountryRestrictionEntity> BrandCountryRestrictions => _database.GetTable<BrandCountryRestrictionEntity>();
        public IConnection Connection => _database;

        public BrandDataProcessor(IConnection database)
        {
            _database = database;
        }

        public async Task<BrandEntity?> GetAsync(int id)
        {
            var brand = await Brands.FirstOrDefaultAsync(b => b.Id == id);
            if (brand != null)
            {
                brand.CountryRestrictions = await GetCountryRestrictionsAsync(id);
            }

            return brand;
        }

        public async Task<List<int>> GetCountryRestrictionsAsync(int brandId)
        {
            return await BrandCountryRestrictions
                .Where(r => r.BrandId == brandId)
                .Select(r => r.CountryId)
                .ToListAsync();
        }

        public async Task<IEnumerable<BrandEntity>> GetAllAsync()
        {
            var query = from brand in Brands.Where(b => !b.IsDeleted)
                        join restriction in BrandCountryRestrictions
                            on brand.Id equals restriction.BrandId into restrictionGroup
                        select new BrandEntity
                        {
                            Id = brand.Id,
                            Description = brand.Description,
                            SupplierId = brand.SupplierId,
                            SupplierName = brand.SupplierName,
                            ImportId = brand.ImportId,
                            IsDeleted = brand.IsDeleted,
                            StatusId = brand.StatusId,
                            ShowInNavigation = brand.ShowInNavigation,
                            CountryRestrictionTypeId = brand.CountryRestrictionTypeId,
                            ArticleContentFilterId = brand.ArticleContentFilterId,
                            CountryRestrictions = restrictionGroup.Select(r => r.CountryId).ToList()
                        };

            return await query.ToListAsync();
        }

        public async Task<BrandEntity?> AddAsync(BrandAddEntity brand)
        {
            var id = await _database.InsertWithInt32IdentityAsync(brand);
            return await GetAsync(id);
        }

        public async Task UpdateAsync(BrandUpdateEntity brand)
        {
            if (brand.Id <= 0)
                throw new ArgumentException("Invalid Brand Id.");

            var existing = await Brands.FirstOrDefaultAsync(b => b.Id == brand.Id);
            if (existing == null)
                throw new KeyNotFoundException("Brand not found.");

            // Update fields
            existing.Description = brand.Description;
            existing.SupplierId = brand.SupplierId;
            existing.SupplierName = brand.SupplierName;
            existing.ImportId = brand.ImportId;
            existing.IsDeleted = brand.IsDeleted;
            existing.StatusId = brand.StatusId;
            existing.ShowInNavigation = brand.ShowInNavigation;
            existing.CountryRestrictionTypeId = brand.CountryRestrictionTypeId;
            existing.ArticleContentFilterId = brand.ArticleContentFilterId;

            await _database.UpdateAsync(existing);

            // Refresh country restrictions
            await RefreshCountryRestrictionsAsync(brand.Id, brand.CountryRestrictions);
        }

        public async Task<IEnumerable<BrandEntity>> GetByIdsAsync(List<int> ids)
        {
            return await Brands.Where(b => ids.Contains(b.Id)).ToListAsync();
        }

        public async Task<BrandEntity> DeleteBrandAsync(int brandId)
        {
            await Brands
                .Where(b => b.Id == brandId)
                .Set(b => b.IsDeleted, true)
                .UpdateAsync();

            return await Brands.SingleAsync(b => b.Id == brandId);
        }

        public async Task RefreshCountryRestrictionsAsync(int brandId, IEnumerable<int> countryIds)
        {
            if (brandId <= 0)
                throw new ArgumentException("Invalid Brand Id.");

            // Always clear previous restrictions
            await BrandCountryRestrictions
                .Where(r => r.BrandId == brandId)
                .DeleteAsync();

            if (countryIds == null || !countryIds.Any())
                return;

            var restrictions = countryIds.Select(id => new BrandCountryRestrictionEntity
            {
                BrandId = brandId,
                CountryId = id
            });

            await _database.BulkInsertAsync(restrictions);
        }
    }
}