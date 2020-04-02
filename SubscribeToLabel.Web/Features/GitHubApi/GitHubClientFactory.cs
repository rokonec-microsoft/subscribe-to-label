using System;
using System.Text;
using System.Threading.Tasks;
using DotNet.SubscribeToLabel.Web.Models.Settings;
using GitHubJwt;
using Microsoft.Extensions.Options;
using Octokit;

namespace DotNet.SubscribeToLabel.Web.Features.GitHubApi
{
    /// <summary>
    /// TODO: UT
    /// </summary>
    public class GitHubClientFactory : IGitHubClientFactory
    {
        readonly IOptions<GitHubAppOptions> _settings;

        public GitHubClientFactory(IOptions<GitHubAppOptions> gitHubSettings)
        {
            _settings = gitHubSettings;
        }

        public IGitHubClient GetGitHubClient()
        {
            // TODO: cache with 5 min TTL

            var jwtToken = GetJwtToken();

            if (string.IsNullOrEmpty(jwtToken))
            {
                throw new NullReferenceException("Unable to generate token");
            }

            // Pass the JWT as a Bearer token to Octokit.net
            GitHubClient appClient = new GitHubClient(new ProductHeaderValue(_settings.Value.Name))
            {
                Credentials = new Credentials(jwtToken, AuthenticationType.Bearer)
            };

            return appClient;
        }

        public async Task<IGitHubClient> GetGitHubInstallationClient(int installationId)
        {
            var appClient = GetGitHubClient();

            // TODO: cache with 5 min TTL
            var installationToken = await appClient.GitHubApps.CreateInstallationToken(installationId);

            // create a client with the installation token
            var installationClient = new GitHubClient(new ProductHeaderValue($"{_settings.Value.Name}_{installationId}"))
            {
                Credentials = new Credentials(installationToken.Token)
            };

            return installationClient;
        }

        public Task<IGitHubClient> GetGitHubInstallationClient()
        {
            return GetGitHubInstallationClient(_settings.Value.InstallationId);
        }

        private string GetJwtToken()
        {
            GitHubAppOptions settings = _settings.Value;

            var factoryOptions = new GitHubJwtFactoryOptions
            {
                AppIntegrationId = settings.ApplicationId, // The GitHub App Id
                ExpirationSeconds = 9*60 // 10 minutes is the maximum time allowed, but because exp claim is absolute time, lets have it 9 minutes to cover time skew
            };

            // TODO: refactor into TryXXX(factoryOptions)
            if (!string.IsNullOrEmpty(settings.PrivateKey.KeyString))
            {
                // use `awk '{printf "%s\\n", $0}' private-key.pem` to store the pem data
                var privateKey = settings.PrivateKey.KeyString.Replace("\n", Environment.NewLine, StringComparison.Ordinal);

                var generator = new GitHubJwtFactory(new StringPrivateKeySource(privateKey), factoryOptions);

                return generator.CreateEncodedJwtToken();
            }
            else if (!string.IsNullOrEmpty(settings.PrivateKey.Base64))
            {
                byte[] data = Convert.FromBase64String(settings.PrivateKey.Base64);
                string decodedString = Encoding.UTF8.GetString(data);

                var generator = new GitHubJwtFactory(new StringPrivateKeySource(decodedString), factoryOptions);

                return generator.CreateEncodedJwtToken();
            }
            else if (!string.IsNullOrEmpty(settings.PrivateKey.File))
            {
                var generator = new GitHubJwtFactory(new FilePrivateKeySource(settings.PrivateKey.File), factoryOptions);

                return generator.CreateEncodedJwtToken();
            }
            else
            {
                throw new InvalidOperationException("Not configured GitHubAppSettings.PrivateKey");
            }
        }
    }
}