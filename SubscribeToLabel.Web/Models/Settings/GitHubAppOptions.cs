namespace DotNet.SubscribeToLabel.Web.Models.Settings
{
    public class GitHubAppOptions
    {
        public string Name { get; set; } = default!;
        public int ApplicationId { get; set; } = default!;
        public int InstallationId { get; set; } = default!;
        public PrivateKeyOptions PrivateKey { get; set; } = default!;
    }
}