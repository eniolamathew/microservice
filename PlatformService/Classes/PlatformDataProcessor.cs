using LinqToDB;
using PlatformService.Interfaces;
using PlatformService.Models.Entities;
using MicroServices.DataAccess.Interfaces;

namespace PlatformService.Classes 
{
    public class PlatformDataProcessor : IPlatformDataProcessor
    {
        private readonly IConnection DataBaseConnection;

        public ITable<PlatformEntity> Platforms => DataBaseConnection.GetTable<PlatformEntity>();

        public IConnection Connection => DataBaseConnection;

        public PlatformDataProcessor(IConnection connection)
        {
            DataBaseConnection = connection;
        }

        public async Task<PlatformEntity?> GetAsync(int id)
        {
            return await Platforms.FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<PlatformEntity>> GetAllAsync()
        {
            return await Platforms.Where(a => !a.IsDeleted).ToListAsync();
        }

        public async Task<PlatformEntity?> AddAsync(PlatformAddEntity platform)
        {
            var id = await DataBaseConnection.InsertWithInt32IdentityAsync(platform);
            return await GetAsync(id);
        }

        public async Task UpdateAsync(PlatformUpdateEntity platform)
        {
            if (platform.Id <= 0)
            {
                throw new ArgumentException("Invalid platform Id.");
            }

            var existingPlatform = await Platforms.FirstOrDefaultAsync(p => p.Id == platform.Id );

            if (existingPlatform != null)
            {
                existingPlatform.Name = platform.Name;
                existingPlatform.Description = platform.Description;
                existingPlatform.Price = platform.Price;
                existingPlatform.Owner = platform.Owner;
                existingPlatform.IsDeleted = platform.IsDeleted;

                await DataBaseConnection.UpdateAsync(existingPlatform);
            }
            else
            {
                throw new KeyNotFoundException("Platform not found.");
            }
        }

        public async Task<IEnumerable<PlatformEntity>> GetByIdsAsync(List<int> ids)
        {
            return await Platforms.Where(p => ids.Contains(p.Id)).ToListAsync(); 
        }

        public async Task<PlatformEntity> DeletePlatformAsync(int platformId)
        {
            await Platforms.Where(a => a.Id == platformId).Set(a => a.IsDeleted, true).UpdateAsync();
            return Platforms.Single(a => a.Id == platformId);
        }
    }
}