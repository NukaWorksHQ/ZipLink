namespace Shared.Common
{
    public class GuidUtils
    {
        public static String GenerateGuid()
        {
            Guid newGuid = Guid.NewGuid();
            return newGuid.ToString();
        }

        public static String GenerateLittleGuid()
        {
            return GenerateGuid().Split("-").ToList()[0];
        }
    }
}
