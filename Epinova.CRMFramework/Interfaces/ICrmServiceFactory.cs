using Microsoft.Xrm.Sdk;

namespace Epinova.CRMFramework.Interfaces
{
    public interface ICrmServiceFactory
    {
        IOrganizationService ServiceProxy { get; }
    }
}
