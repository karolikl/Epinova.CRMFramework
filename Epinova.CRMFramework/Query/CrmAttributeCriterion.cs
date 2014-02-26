
namespace Epinova.CRMFramework.Query
{
    public class CrmAttributeCriterion
    {
        public string AttributeName
        {
            get;
            set;
        }

        public object AttributeValue
        {
            get;
            set;
        }

        public CrmConditionOperator ConditionOperator
        {
            get;
            set;
        }

        public CrmAttributeCriterion() { }

        public CrmAttributeCriterion(string attributeName, object attributeValue, CrmConditionOperator conditionOperator)
        {
            AttributeName = attributeName;
            AttributeValue = attributeValue;
            ConditionOperator = conditionOperator;
        }
    }
}
