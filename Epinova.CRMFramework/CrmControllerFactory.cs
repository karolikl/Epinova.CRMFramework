
using Epinova.CRMFramework.Interfaces;

namespace Epinova.CRMFramework
{
    /// <summary>
    /// CRM Factory class used to instanciate EntityContollers
    /// </summary>
    public class CrmControllerFactory : ICrmControllerFactory
    {
        private static ICrmControllerFactory _instance;
        private static ICrmServiceFactory _service;

        protected CrmControllerFactory()
        {
        }

        protected CrmControllerFactory(ICrmServiceFactory newService)
        {
            _service = newService;
        }

        public static void SetTestingInstance(ICrmControllerFactory newInstance, ICrmServiceFactory newService)
        {
            _instance = newInstance;
            _service = newService;
        }

        public static ICrmControllerFactory Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new CrmControllerFactory(new CrmServiceFactory());
                return _instance;
            }
        }

        public CrmEntityController<T> GetEntityController<T>() where T : class, new()
        {
            return new CrmEntityController<T>(_service);
        }

        public CrmManyToManyRelationshipController<T, V> GetManyToManyRelationshipController<T, V>()
            where T : class, new()
            where V : class, new()
        {
            return new CrmManyToManyRelationshipController<T, V>(_service);
        }
    }
}
