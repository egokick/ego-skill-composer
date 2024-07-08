using Azure.Identity;
using Microsoft.Identity.Client;

namespace skill_composer.Helper
{
    public static class AuthenticationHelper
    {
        private static InteractiveBrowserCredential? interactiveBrowserCredential = null;
        private static IPublicClientApplication? publicClientApplication = null;

        public static InteractiveBrowserCredential GetInteractiveBrowserCredential(string clientId)
        {
            if (interactiveBrowserCredential == null)
            {
                interactiveBrowserCredential = new InteractiveBrowserCredential(new InteractiveBrowserCredentialOptions
                {
                    ClientId = clientId,
                    TenantId = "common", // Use "common" for multi-tenant and personal accounts
                    AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
                });
            }

            return interactiveBrowserCredential;
        }

        public static IPublicClientApplication GetPublicClientApplication(string clientId)
        {
            if (publicClientApplication == null)
            {
                publicClientApplication = PublicClientApplicationBuilder.Create(clientId)
                    .WithAuthority(AzureCloudInstance.AzurePublic, "common")
                    .WithRedirectUri("http://localhost")
                    .Build();
            }

            return publicClientApplication;
        }
    }
}
