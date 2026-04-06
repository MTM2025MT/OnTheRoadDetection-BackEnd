using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using PotholeApi.DTO;
using PotholeApi.IRepositories;
using PotholeApi.Models;
using PotholeApi.Repositories;
using System.Threading.Tasks;
using static PotholeApi.Models.Enums;
namespace PotholeApi.Services.DashboardsServices
{
    /// <summary>
    /// Provides services for retrieving dashboard data related to pothole incidents and statistics.
    /// </summary>
    /// <remarks>This service supplies aggregated data for dashboards, such as pothole counts, severity
    /// levels, and cost estimates, typically used by officers or administrators to monitor and analyze pothole trends
    /// across different neighborhoods.</remarks>
    public class PotholeDashboardsService: IPotholeDashboardsService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPotholeRepository _potholeRepository;
        private readonly Context _context;
        private readonly INeighbourhoodRepository _neighbourhoodRepository;
        public PotholeDashboardsService(INeighbourhoodRepository neighbourhoodRepository, Context context, IPotholeRepository potholeRepository, UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _potholeRepository = potholeRepository;
            _context = context;
            _neighbourhoodRepository = neighbourhoodRepository;

        }
        public async Task<DashboardPotholeDataDto> GetPotholeDashboardsServices(int OfficerId)
        {
            // Temporary hardcoded data for demonstration purposes
             District districtoftheOfficer = _context.Officers.Where(O=>O.Id== OfficerId).Select(O=>O.District).FirstOrDefault();
            Console.WriteLine("/////////////////"+districtoftheOfficer.Name);
            if (districtoftheOfficer == null)
            {
                Console.WriteLine("the districatofofficer is null");
            }
            
             
             var potholesInDistrict =await _potholeRepository.GetPotholesByDistrictIdAsync(districtoftheOfficer.Id);
            Console.WriteLine(potholesInDistrict);
            if (potholesInDistrict == null)
            {
                Console.WriteLine("the potholes in district are null");
            }
             var totalPotholes = potholesInDistrict.Count();
             var neighbourhoods = await _neighbourhoodRepository.GetNeighbourhoodsByDistrictIdWithPotholesAsync(districtoftheOfficer.Id);
              if(neighbourhoods == null || !neighbourhoods.Any())
            {
                Console.WriteLine("neighbourhoods is null" );
                return new DashboardPotholeDataDto
                {
                    DashboardNeighbourhoods = new List<DashboardNeighbourhoodDto>(),
                    LevelCounts = new Dictionary<Level, int>()
                };
            }
               var neighbourhoodDtos = neighbourhoods.Select(n =>
               {
                   var potholes = n.Potholes??new List<Pothole> ();
                   var numberOfPotholesInNeighbourhood = potholes.Count();
                     
                     var severityOfNeighbourhood = numberOfPotholesInNeighbourhood > 0 ?
                      potholes.Average(p => (int)p.LevelOfSeverity) 
                   : 0;
                     var cost = 1; // Assuming a fixed cost per pothole for demonstration
    
                     return new DashboardNeighbourhoodDto
                     {
                          name = n.Name,
                          NumberOfpotholes = numberOfPotholesInNeighbourhood,
                          severityOfNeighbourhood = severityOfNeighbourhood,
                          cost = cost
                     };
               });
            var levelCounts = Enum.GetValues(typeof(Level))
                .Cast<Level>()
                .ToDictionary(
                    level => level,
                    level => potholesInDistrict.Count(p => p.LevelOfSeverity == level)
                );

               var dashboardData = new DashboardPotholeDataDto
               {
                    DashboardNeighbourhoods = neighbourhoodDtos.ToList(),
                    LevelCounts = levelCounts,
        
               };
            return dashboardData;
        }
    }
}
