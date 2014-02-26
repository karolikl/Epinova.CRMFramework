using System;

namespace Epinova.CRMFramework.Exception
{
    public class CrmException : SystemException
    {
        private readonly string _message;

        public CrmException()
        {
        }

        public CrmException(string message)
        {
            _message = String.Format("Epinova.CRMFramework failed with exception: {0}",
                    message);
        }

        public CrmException(string message, string entity, string attribute)
        {
            _message = String.Format("Epinova.CRMFramework failed with exception: {0} Entity: {1}. Attribute: {2}",
                    message, entity, attribute);
        }

        public override string Message
        {
            get { return _message; }
        }
    }
}
