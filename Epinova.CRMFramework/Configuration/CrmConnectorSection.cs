using System;
using System.Configuration;

namespace Epinova.CRMFramework.Configuration
{
    class CrmConnectorSection : ConfigurationSection
    {
        private static CrmConnectorSection _instance = ConfigurationManager.GetSection("crmFramework/crmConnector") as CrmConnectorSection;

        public static CrmConnectorSection Instance
        {
            get
            {
                return _instance;
            }
        }

        // Create a "webServiceUrl" attribute.
        [ConfigurationProperty("crmUrl", IsRequired = true)]
        public String CrmUrl
        {
            get
            {
                return (String)this["crmUrl"];
            }
            set
            {
                this["crmUrl"] = value;
            }
        }

        // Create a "username" attribute.
        [ConfigurationProperty("username", IsRequired = true)]
        public String UserName
        {
            get
            {
                return (String)this["username"];
            }
            set
            {
                this["username"] = value;
            }
        }

        // Create a "password" attribute.
        [ConfigurationProperty("password", IsRequired = true)]
        public String Password
        {
            get
            {
                return (String)this["password"];
            }
            set
            {
                this["password"] = value;
            }
        }

        // Create a "domain" attribute.
        [ConfigurationProperty("domain", IsRequired = true)]
        public String Domain
        {
            get
            {
                return (String)this["domain"];
            }
            set
            {
                this["domain"] = value;
            }
        }

        // Create a "organization" attribute.
        [ConfigurationProperty("organization", IsRequired = true)]
        public String Organization
        {
            get
            {
                return (String)this["organization"];
            }
            set
            {
                this["organization"] = value;
            }
        }
    }
}
