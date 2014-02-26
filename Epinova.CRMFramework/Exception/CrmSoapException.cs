using System;
using System.Web.Services.Protocols;

namespace Epinova.CRMFramework.Exception
{
    class CrmSoapException : SystemException
    {
        private readonly string _message;

        public CrmSoapException(SoapException exception)
        {
            _message = exception.Detail.SelectSingleNode("//description").InnerXml;
        }

        public override string Message
        {
            get
            {
                return String.Format("Epinova.CRMFramework failed with exception: {0}",
                    _message);
            }
        }
    }
}
