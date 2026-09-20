using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PotholeApi.DTO;
using PotholeApi.IRepositories;
using PotholeApi.Models;
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
    public class PotholeDashboardsService : IPotholeDashboardsService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPotholeRepository _potholeRepository;
        private readonly Context _context;
        private readonly INeighbourhoodRepository _neighbourhoodRepository;
        private readonly ILogger<PotholeDashboardsService> _logger;

        public PotholeDashboardsService(INeighbourhoodRepository neighbourhoodRepository, Context context, IPotholeRepository potholeRepository, UserManager<ApplicationUser> userManager, ILogger<PotholeDashboardsService> logger)
        {
            _userManager = userManager;
            _potholeRepository = potholeRepository;
            _context = context;
            _neighbourhoodRepository = neighbourhoodRepository;
            _logger = logger;
        }

        public async Task<DashboardPotholeDataDto> GetPotholeDashboardsServices(int officerId)
        {
            var districtOfTheOfficer = _context.Officers
                .Where(o => o.Id == officerId)
                .Select(o => o.District)
                .FirstOrDefault();

            if (districtOfTheOfficer == null)
            {
                _logger.LogWarning("GetPotholeDashboardsServices: officer {OfficerId} has no assigned district", officerId);
                return new DashboardPotholeDataDto
                {
                    DashboardNeighbourhoods = new List<DashboardNeighbourhoodDto>(),
                    LevelCounts = new Dictionary<Level, int>()
                };
            }

            var potholesInDistrict = await _potholeRepository.GetPotholesByDistrictIdAsync(districtOfTheOfficer.Id);
            var neighbourhoods = await _neighbourhoodRepository.GetNeighbourhoodsByDistrictIdWithPotholesAsync(districtOfTheOfficer.Id);
            if (neighbourhoods == null || !neighbourhoods.Any())
            {
                _logger.LogInformation("GetPotholeDashboardsServices: district {DistrictId} has no neighbourhoods", districtOfTheOfficer.Id);
                return new DashboardPotholeDataDto
                {
                    DashboardNeighbourhoods = new List<DashboardNeighbourhoodDto>(),
                    LevelCounts = new Dictionary<Level, int>()
                };
            }

            var neighbourhoodDtos = neighbourhoods.Select(n =>
            {
                var potholes = n.Potholes ?? new List<Pothole>();
                var numberOfPotholesInNeighbourhood = potholes.Count();
                var severityOfNeighbourhood = numberOfPotholesInNeighbourhood > 0
                    ? potholes.Average(p => (int)p.LevelOfSeverity)
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

            return new DashboardPotholeDataDto
            {
                DashboardNeighbourhoods = neighbourhoodDtos.ToList(),
                LevelCounts = levelCounts,
            };
        }
    }
}
