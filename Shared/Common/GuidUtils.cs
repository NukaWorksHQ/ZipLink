namespace Shared.Common
{
    public class GuidUtils
    {
        private static readonly string Characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private static readonly Random Random = new Random();

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
            for (int i = 0; i < length; i++)
            {
                result[i] = Characters[Random.Next(Characters.Length)];
            }
            return new string(result);
        }

        public static string GenerateUniqueShortId(HashSet<string> existingIds, int initialLength = 3)
        {
            const int maxAttempts = 100;
            int currentLength = Math.Max(3, initialLength);

            while (currentLength <= 8)
            {
                for (int attempt = 0; attempt < maxAttempts; attempt++)
                {
                    string id = GenerateShortId(currentLength);
                    if (!existingIds.Contains(id))
                    {
                        return id;
                    }
                }
                currentLength++; // Augmenter la longueur si trop de collisions
            }

            // Fallback : utiliser un UUID partiel si tout est épuisé
            return GenerateLittleGuid().Substring(0, 8);
        }

        public static long GetMaxCombinations(int length)
        {
            return (long)Math.Pow(Characters.Length, length);
        }
    }
}
