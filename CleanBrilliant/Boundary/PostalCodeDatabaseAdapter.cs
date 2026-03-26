using CleanBrilliant.Domain.DomainInterface;

namespace CleanBrilliant.Boundary
{
    public class PostalCodeDatabaseAdapter : IPostalService
    {
        private static readonly Lazy<Dictionary<string, (double Lat, double Lng)>> _postalLookup =
            new(LoadPostalData);

        private static Dictionary<string, (double Lat, double Lng)> LoadPostalData()
        {
            var lookup = new Dictionary<string, (double Lat, double Lng)>();
            var baseDir = Directory.GetCurrentDirectory();
            var files = new[] { "SG.txt", "MY.txt" };

            foreach (var file in files)
            {
                var path = Path.Combine(baseDir, file);
                if (!File.Exists(path))
                    path = Path.Combine(baseDir, "..", file);
                if (!File.Exists(path))
                    continue;

                foreach (var line in File.ReadLines(path))
                {
                    var cols = line.Split('\t');
                    if (cols.Length < 11)
                        continue;

                    var postalCode = cols[1].Trim();
                    if (string.IsNullOrEmpty(postalCode))
                        continue;

                    if (double.TryParse(cols[9], out var lat) && double.TryParse(cols[10], out var lng))
                    {
                        lookup.TryAdd(postalCode, (lat, lng));
                    }
                }
            }

            return lookup;
        }

        private static (double Lng, double Lat) LookupPostal(string postalCode)
        {
            // Default fallback: Singapore CBD
            const double defaultLat = 1.2830;
            const double defaultLng = 103.8513;

            if (!string.IsNullOrEmpty(postalCode) && _postalLookup.Value.TryGetValue(postalCode.Trim(), out var coords))
                return (coords.Lng, coords.Lat);

            return (defaultLng, defaultLat);
        }

        public async Task<((double Longitude, double Latitude) Source, (double Longitude, double Latitude) Dest)> GetPostalConversion(string sourcePostal, string destPostal)
        {
            await Task.CompletedTask;
            var src = LookupPostal(sourcePostal);
            var dest = LookupPostal(destPostal);
            return ((src.Lng, src.Lat), (dest.Lng, dest.Lat));
        }
    }
}
