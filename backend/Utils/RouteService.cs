using TSM.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace backend.Utils
{
    public static class RouteService
    {
        public static async Task<List<Place>> GetRouteAsync(double myLat, double myLong, List<long> categories, ModelBase dbContext)
        {
            var fcat = new List<long>(categories);
            var closest = new List<Place>();
            var pos = new { lat = myLat, lon = myLong };
            var route = new List<Place>();

            for (var iter = 0; iter < categories.Count; iter++)
            {
                foreach (var cat in new HashSet<long>(fcat))
                {
                    var sqlRows = await dbContext.Places
                        .Where(p => p.CategoryId == cat)
                        .OrderBy(p => Math.Sqrt(Math.Pow(p.Lat - pos.lat, 2) + Math.Pow(p.Long - pos.lon, 2)))
                        .ToListAsync();

                    foreach (var place in sqlRows)
                    {
                        if (!route.Contains(place))
                        {
                            closest.Add(place);
                            break;
                        }
                    }
                }

                Place singleClosest = null;
                double? minDist = null;
                foreach (var place in closest)
                {
                    var dist = Math.Sqrt(Math.Pow(place.Lat - pos.lat, 2) + Math.Pow(place.Long - pos.lon, 2));
                    if (singleClosest == null || dist < minDist)
                    {
                        singleClosest = place;
                        minDist = dist;
                    }
                }

                route.Add(singleClosest);
                closest = new List<Place>();
                pos = new { lat = singleClosest.Lat, lon = singleClosest.Long };
                fcat.Remove(singleClosest.CategoryId);
            }

            return route;
        }
    }
}
