namespace Shared.Common
{
    public class GuidUtils
    {
        private static readonly string Characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private static readonly ThreadLocal<Random> Random = new ThreadLocal<Random>(() => new Random(Guid.NewGuid().GetHashCode()));

        public static String GenerateGuid()
        {
            Guid newGuid = Guid.NewGuid();
            return newGuid.ToString();
        }

        public static String GenerateLittleGuid()
        {
            return GenerateGuid().Split("-").ToList()[0];
        }

        public static string GenerateShortId(int length = 3)
        {
            if (length < 3) length = 3;
            if (length > 8) length = 8;

            var result = new char[length];
            var random = Random.Value!;
            for (int i = 0; i < length; i++)
            {
                result[i] = Characters[random.Next(Characters.Length)];
            }
            return new string(result);
        }

        public static string GenerateUniqueShortId(HashSet<string> existingIds, int initialLength = 3)
        {
            const int maxAttempts = 25; // Reduced to trigger length increment faster
            int currentLength = Math.Max(3, Math.Min(8, initialLength)); // Ensure valid bounds

            while (currentLength <= 8)
            {
                // Check occupancy rate first - if too crowded, skip to next length
                var idsOfCurrentLength = existingIds.Count(id => id.Length == currentLength);
                var maxPossibleForLength = GetMaxCombinations(currentLength);
                var occupancyRate = (double)idsOfCurrentLength / maxPossibleForLength;
                
                // If we're getting close to capacity (>50%), jump to next length immediately
                // This is more aggressive to ensure we progress through lengths
                if (occupancyRate > 0.50)
                {
                    currentLength++;
                    continue;
                }

                // Try to generate a unique ID at current length
                int collisionCount = 0;
                for (int attempt = 0; attempt < maxAttempts; attempt++)
                {
                    string id = GenerateShortId(currentLength);
                    if (!existingIds.Contains(id))
                    {
                        return id;
                    }
                    collisionCount++;
                }
                
                // If we had too many collisions, move to next length
                // This provides a secondary check in case occupancy calculation is off
                if (collisionCount >= maxAttempts)
                {
                    currentLength++;
                }
            }

            // Enhanced fallback: use timestamp + random component to ensure uniqueness
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            var timestampSuffix = timestamp.Length >= 5 ? timestamp.Substring(timestamp.Length - 5) : timestamp;
            var randomSuffix = GenerateShortId(3);
            var fallbackId = $"{timestampSuffix}{randomSuffix}";
            return fallbackId.Length > 8 ? fallbackId.Substring(0, 8) : fallbackId;
        }

        public static long GetMaxCombinations(int length)
        {
            return (long)Math.Pow(Characters.Length, length);
        }
    }
}
