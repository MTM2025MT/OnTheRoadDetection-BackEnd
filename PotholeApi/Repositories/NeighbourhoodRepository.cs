using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PotholeApi.IRepositories;
using PotholeApi.Models;

namespace PotholeApi.Repositories
{
    public class NeighbourhoodRepository: INeighbourhoodRepository
    {
        private readonly Context _context;
        private readonly ILogger<NeighbourhoodRepository> _logger;
        public NeighbourhoodRepository(Context context, ILogger<NeighbourhoodRepository> logger) {
         _context = context;
         _logger = logger;
        }

        public async Task<ICollection<Neighbourhood>> GetAllAsync(){
         return await _context.Neighbourhoods.ToListAsync();
        }
        public async Task<Neighbourhood?> GetByIdAsync(int id){
         var neighbourhood = await _context.Neighbourhoods.FindAsync(id);
         if (neighbourhood == null)
         {
             _logger.LogWarning("GetByIdAsync: neighbourhood {NeighbourhoodId} not found", id);
         }
         return neighbourhood;
        }
        public async Task<ICollection<Neighbourhood>> GetNeighbourhoodsByDistrictIdAsync(int districtId){

            return await _context.Neighbourhoods
               .Where(n => n.DistrictId == districtId)
               .ToListAsync();
        }

        public async Task<ICollection<Neighbourhood>> GetNeighbourhoodsByDistrictIdWithPotholesAsync(int districtId)
        {
            return await _context.Neighbourhoods
              .Where(n => n.DistrictId == districtId).Include(n => n.Potholes)
             .ToListAsync();
        }
    }
}
