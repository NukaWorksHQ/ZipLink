using Shared.Entities;

namespace Web.Common
{
    public class Utils
    {
        public static string GetFinalLink(Link link, IConfiguration config)
        {
            if (link is null)
                throw new ArgumentNullException(nameof(link));

            var endpoint = config["ApiHost"] + $"/{link.Id}";
            return endpoint;
        }
    }
}
