using Microsoft.Xrm.Sdk.Query;

namespace Epinova.CRMFramework.Query
{
    internal static class OperatorMapper
    {
        internal static ConditionOperator GetMappedOperator(CrmConditionOperator conditionOperator)
        {
            if (conditionOperator == CrmConditionOperator.BeginsWith)
                return ConditionOperator.BeginsWith;
            if (conditionOperator == CrmConditionOperator.Contains)
                return ConditionOperator.Contains;
            if (conditionOperator == CrmConditionOperator.EndsWith)
                return ConditionOperator.EndsWith;
            if (conditionOperator == CrmConditionOperator.Equal)
                return ConditionOperator.Equal;
            if (conditionOperator == CrmConditionOperator.GreaterEqual)
                return ConditionOperator.GreaterEqual;
            if (conditionOperator == CrmConditionOperator.GreaterThan)
                return ConditionOperator.GreaterThan;
            if (conditionOperator == CrmConditionOperator.LessEqual)
                return ConditionOperator.LessEqual;
            if (conditionOperator == CrmConditionOperator.LessThan)
                return ConditionOperator.LessThan;
            if (conditionOperator == CrmConditionOperator.Like)
                return ConditionOperator.Like;
            if (conditionOperator == CrmConditionOperator.NotEqual)
                return ConditionOperator.NotEqual;
            if (conditionOperator == CrmConditionOperator.NotLike)
                return ConditionOperator.NotLike;
            return ConditionOperator.Equal;
        }

        public static LogicalOperator GetMappedOperator(CrmLogicalOperator logicalOperator)
        {
            if (logicalOperator == CrmLogicalOperator.And)
                return LogicalOperator.And;
            return LogicalOperator.Or;
        }
    }
}
