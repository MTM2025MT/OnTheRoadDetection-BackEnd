using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities; // <--- ADD THIS NAMESPACE
using NetTopologySuite.IO;
using Newtonsoft.Json.Linq;
using PotholeApi.Models;
using System;
using System.IO;
using System.Linq;

namespace PotholeApi.Helpers
{
    internal static class DistrictSeeder
    {
        public static void Seed(Context context, ILogger logger, string basePath)
        {
            var filePath = Path.Combine(basePath, "Data", "export2.geojson");
            if (!File.Exists(filePath))
            {
                logger.LogWarning("GeoJSON file not found at: {FilePath}", filePath);
                return;
            }

            var json = File.ReadAllText(filePath);
            var root = JObject.Parse(json);

            // GeoJSON uses "features" array
            var features = root["features"] as JArray;
            if (features == null)
            {
                logger.LogWarning("No features found in GeoJSON file");
                return;
            }

            logger.LogInformation("Starting to seed {FeatureCount} districts from GeoJSON", features.Count);

            int successCount = 0;
            int failCount = 0;

            var factory = new GeometryFactory(new PrecisionModel(), 4326);

            foreach (var feature in features)
            {
                try
                {
                    // Extract name from properties
                    var properties = feature["properties"];
                    var name = properties?["name"]?.ToString();

                    if (string.IsNullOrWhiteSpace(name))
                    {
                        failCount++;
                        continue;
                    }

                    // Skip if already exists
                    if (context.Districts.Any(d => d.Name == name))
                    {
                        logger.LogInformation("⊘ District already exists: {DistrictName}", name);
                        continue;
                    }

                    // Extract geometry
                    var geometry = feature["geometry"];
                    if (geometry == null)
                    {
                        failCount++;
                        continue;
                    }

                    var geometryType = geometry["type"]?.ToString();
                    var coordinates = geometry["coordinates"];

                    if (coordinates == null)
                    {
                        failCount++;
                        continue;
                    }

                    // Parse coordinates based on geometry type
                    Geometry geom = null;

                    if (geometryType == "MultiPolygon")
                    {
                        // MultiPolygon: coordinates[i][j][k] = [lon, lat]
                        var polygons = new List<Polygon>();

                        foreach (var polygon in coordinates)
                        {
                            var rings = polygon as JArray;
                            if (rings == null || rings.Count == 0) continue;

                            // First ring is outer boundary
                            var outerRing = rings[0] as JArray;
                            if (outerRing == null || outerRing.Count < 3) continue;

                            var coords = new List<Coordinate>();
                            foreach (var point in outerRing)
                            {
                                double lon = (double)point[0];
                                double lat = (double)point[1];
                                // SQL Server expects: X = lon, Y = lat (standard GIS order)
                                coords.Add(new Coordinate(lon, lat));
                            }

                            if (coords.Count >= 3)
                            {
                                // Close the ring if needed
                                if (!coords[0].Equals2D(coords[coords.Count - 1]))
                                {
                                    coords.Add(coords[0].Copy());
                                }

                                try
                                {
                                    var linearRing = factory.CreateLinearRing(coords.ToArray());
                                    polygons.Add(factory.CreatePolygon(linearRing));
                                }
                                catch (Exception ex)
                                {
                                    logger.LogWarning(ex, "Error creating polygon for district: {DistrictName}", name);
                                }
                            }
                        }

                        if (polygons.Count > 0)
                        {
                            geom = factory.CreateMultiPolygon(polygons.ToArray());
                        }
                    }
                    else if (geometryType == "Polygon")
                    {
                        // Polygon: coordinates[i][j] = [lon, lat]
                        var rings = coordinates as JArray;
                        if (rings != null && rings.Count > 0)
                        {
                            var outerRing = rings[0] as JArray;
                            if (outerRing != null && outerRing.Count >= 3)
                            {
                                var coords = new List<Coordinate>();
                                foreach (var point in outerRing)
                                {
                                    double lon = (double)point[0];
                                    double lat = (double)point[1];
                                    coords.Add(new Coordinate(lon, lat));
                                }

                                if (!coords[0].Equals2D(coords[coords.Count - 1]))
                                {
                                    coords.Add(coords[0].Copy());
                                }

                                var linearRing = factory.CreateLinearRing(coords.ToArray());
                                var polygon = factory.CreatePolygon(linearRing);
                                geom = factory.CreateMultiPolygon(new[] { polygon });
                            }
                        }
                    }

                    if (geom == null)
                    {
                        failCount++;
                        continue;
                    }

                    // Fix invalid geometries (self-intersecting, wrong winding order, etc.)
                    if (!geom.IsValid)
                    {
                        geom = GeometryFixer.Fix(geom);
                    }

                    // Ensure result is MultiPolygon
                    MultiPolygon finalGeometry = null;

                    if (geom is MultiPolygon mp)
                    {
                        finalGeometry = mp;
                    }
                    else if (geom is Polygon p)
                    {
                        finalGeometry = factory.CreateMultiPolygon(new[] { p });
                    }
                    else if (geom is GeometryCollection gc)
                    {
                        var polys = gc.Geometries
                            .Where(g => g is Polygon)
                            .Cast<Polygon>()
                            .ToArray();

                        if (polys.Length > 0)
                        {
                            finalGeometry = factory.CreateMultiPolygon(polys);
                        }
                    }

                    if (finalGeometry == null)
                    {
                        failCount++;
                        continue;
                    }

                    finalGeometry.SRID = 4326;

                    context.Districts.Add(new District
                    {
                        Name = name,
                        Geometry = finalGeometry
                    });

                    successCount++;
                    logger.LogInformation("✓ Successfully added district: {DistrictName}", name);
                }
                catch (Exception ex)
                {
                    failCount++;
                    logger.LogError(ex, "Error processing feature");
                }
            }

            context.SaveChanges();
            logger.LogInformation("=== District Seeding Complete ===");
            logger.LogInformation("Successfully added: {SuccessCount} districts", successCount);
            logger.LogInformation("Failed: {FailCount}", failCount);
        }
    }
}