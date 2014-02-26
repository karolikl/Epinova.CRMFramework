using System.Collections.Generic;

namespace Epinova.CRMFramework.Query
{
    public class CrmQuery
    {
        private readonly List<CrmAttributeCriterion> _attributes;

        public CrmLogicalOperator LogicalOperator
        {
            get;
            set;
        }

        public List<CrmAttributeCriterion> Conditions
        {
            get
            {
                return _attributes;
            }
        }

        public CrmQuery() : this(null) { } 

        public CrmQuery(CrmAttributeCriterion criterion)
        {
            _attributes = new List<CrmAttributeCriterion>();
            if (criterion != null)
                _attributes.Add(criterion);
            LogicalOperator = CrmLogicalOperator.And;
        }

        public void AddCondition(CrmAttributeCriterion criterion)
        {
            _attributes.Add(criterion);
        }
    }
}
