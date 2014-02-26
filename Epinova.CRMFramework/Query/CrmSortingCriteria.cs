
namespace Epinova.CRMFramework.Query
{
    public class CrmSortingCriteria
    {
        private readonly string _attributeName;
        private readonly CrmSortOrder _sortOrder;

        public string AttributeName
        {
            get
            {
                return _attributeName;
            }
        }

        public CrmSortOrder SortOrder
        {
            get
            {
                return _sortOrder;
            }
        }

        public CrmSortingCriteria(string attributeName, CrmSortOrder sortOrder)
        {
            _attributeName = attributeName;
            _sortOrder = sortOrder;
        }
    }
}
