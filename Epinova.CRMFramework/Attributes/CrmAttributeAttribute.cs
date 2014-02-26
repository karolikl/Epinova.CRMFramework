using System;

namespace Epinova.CRMFramework.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class CrmAttributeAttribute : Attribute
    {
        private readonly string _crmAttributeName;
        public CrmAttributeAttribute(string crmAttributeName)
        {
            _crmAttributeName = crmAttributeName;
        }

        public string CrmAttributeName
        {
            get
            {
                return _crmAttributeName;
            }
        }

        public string ReferenceEntity
        {
            get;
            set;
        }
    }
}
