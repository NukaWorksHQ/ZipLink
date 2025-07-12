namespace Shared.Common
{
    public class UsernameGenerator
    {
        private static readonly List<string> adjectives = new List<string>
    {
        "Swift", "Silent", "Brave", "Witty", "Bold", "Sneaky", "Mighty"
    };

        private static readonly List<string> nouns = new List<string>
    {
        "Falcon", "Shadow", "Tiger", "Wizard", "Rogue", "Ninja", "Dragon"
    };

        public static string GenerateTemporaryUsername()
        {
            Random rand = new Random();
            string adjective = adjectives[rand.Next(adjectives.Count)];
            string noun = nouns[rand.Next(nouns.Count)];
            int number = rand.Next(1000, 9999);

            return $"{adjective}{noun}{number}";
        }
    }
}
