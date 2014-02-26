using System;
using System.Net;
using System.ServiceModel.Description;
using Epinova.CRMFramework.Configuration;
using Epinova.CRMFramework.Exception;
using Epinova.CRMFramework.Interfaces;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Epinova.CRMFramework
{
    class CrmServiceFactory : ICrmServiceFactory
    {
        private const string OrganizationWebServiceUrl = "{0}/{1}/XRMServices/2011/Organization.svc";
        private readonly IOrganizationService _organizationServiceProxy;
        public IOrganizationService ServiceProxy { get { return _organizationServiceProxy; } }

        public CrmServiceFactory()
        {
            Uri organizationUri = GetOrganizationUri();

            if (string.IsNullOrWhiteSpace(CrmConnectorSection.Instance.UserName))
                throw new CrmException("A value must be supplied for username in the <crmFramework> section in web.config");

            if (string.IsNullOrWhiteSpace(CrmConnectorSection.Instance.Password))
                throw new CrmException("A value must be supplied for password in the <crmFramework> section in web.config");

            if (string.IsNullOrWhiteSpace(CrmConnectorSection.Instance.Domain))
                throw new CrmException("A value must be supplied for domain in the <crmFramework> section in web.config");

            IServiceManagement<IOrganizationService> serviceManagement = ServiceConfigurationFactory.CreateManagement<IOrganizationService>(organizationUri);

            ClientCredentials clientCredentials = new ClientCredentials();
            clientCredentials.Windows.ClientCredential = CredentialCache.DefaultNetworkCredentials;
            clientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;
            clientCredentials.UserName.UserName = string.Format("{0}@{1}", CrmConnectorSection.Instance.UserName, CrmConnectorSection.Instance.Domain);
            clientCredentials.UserName.Password = CrmConnectorSection.Instance.Password;
            OrganizationServiceProxy organizationServiceProxy = new OrganizationServiceProxy(
                serviceManagement,
                clientCredentials);
            organizationServiceProxy.EnableProxyTypes();
            _organizationServiceProxy = organizationServiceProxy;
        }

        private Uri GetOrganizationUri()
        {
            if (string.IsNullOrWhiteSpace(CrmConnectorSection.Instance.CrmUrl))
                throw new CrmException("A value must be supplied for crmUrl in the <crmFramework> section in web.config");

            if (string.IsNullOrWhiteSpace(CrmConnectorSection.Instance.Organization))
                throw new CrmException("A value must be supplied for organization in the <crmFramework> section in web.config");

            string crmUrl = CrmConnectorSection.Instance.CrmUrl.EndsWith("/")
                                ? CrmConnectorSection.Instance.CrmUrl.TrimEnd('/')
                                : CrmConnectorSection.Instance.CrmUrl;

            return new Uri(string.Format(OrganizationWebServiceUrl, crmUrl,
                                            CrmConnectorSection.Instance.Organization));
        }
    }
}
