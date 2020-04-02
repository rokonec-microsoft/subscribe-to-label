namespace DotNet.SubscribeToLabel.Web.Models.Settings
{
    /// <summary>
    /// Support couple of ways how to store private ekey
    /// </summary>
    public class PrivateKeyOptions
    {
        /// <summary>
        /// Local path to the private key filePath to local .pem file
        /// </summary>
        public string? File { get; set; }

        /// <summary>
        /// String value. You can replace new lines using: awk '{printf "%s\\n", $0}' path/to/your/private-key.pem
        /// </summary>
        public string? Base64 { get; set; }

        /// <summary>
        /// Base64 encoded string: cat path/to/your/private-key.pem | openssl base64 | pbcopy
        /// </summary>
        public string? KeyString { get; set; }
    }
}
