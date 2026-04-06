using Microsoft.EntityFrameworkCore;
using PotholeApi.IRepositories;
using PotholeApi.Models;

namespace PotholeApi.Repositories
{
    public class NeighbourhoodRepository: INeighbourhoodRepository
    {
        private readonly Context _context;
        public NeighbourhoodRepository(Context context) {
         _context = context;
        }

        public async Task<ICollection<Neighbourhood>> GetAllAsync(){
         return await _context.Neighbourhoods.ToListAsync();
        }
        public Task<Neighbourhood?> GetByIdAsync(int id){
         return _context.Neighbourhoods.FindAsync(id).AsTask();
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
