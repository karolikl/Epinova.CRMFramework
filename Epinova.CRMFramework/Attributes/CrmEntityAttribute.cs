using System;

namespace Epinova.CRMFramework.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CrmEntityAttribute : Attribute
    {
        private readonly string _crmEntityName;
        public CrmEntityAttribute(string crmEntityName)
        {
            _crmEntityName = crmEntityName;
        }

        public string CrmEntityName
        {
            get
            {
                return _crmEntityName;
            }
        }
    }
}
