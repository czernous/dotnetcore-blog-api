#pragma warning disable 1591 

namespace api.Models
{
    public class ApiSettings : IApiSettings
    {
        public string ApiKey { get; set; }

        public string CloudinaryName { get; set; }
        public string CloudinaryKey { get; set; }
        public string CloudinarySecret { get; set; }
        public string CloudinaryUrl { get; set; }
    }

    public interface IApiSettings
    {
        string ApiKey { get; set; }
        string CloudinaryName { get; set; }
        string CloudinaryKey { get; set; }
        string CloudinarySecret { get; set; }
        string CloudinaryUrl { get; set; }
    }
}