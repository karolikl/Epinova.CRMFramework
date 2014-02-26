using System.Collections.Generic;
using System.Web.Services.Protocols;
using Epinova.CRMFramework.Exception;
using Epinova.CRMFramework.Query;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace Epinova.CRMFramework
{
    static class QueryHelper
    {
        /// <summary>
        /// Creates an OrderExpression from a CrmSortingCriteria object and adds it to the query
        /// </summary>
        /// <param name="query">The query to which the OrderExpression should be added</param>
        /// <param name="sortingCriteria">The sortingCriteria</param>
        /// <returns>An OrderExpression</returns>
        public static void AddOrderExpression(QueryExpression query, CrmSortingCriteria sortingCriteria)
        {
            if (sortingCriteria == null)
                return;

            OrderType orderType = sortingCriteria.SortOrder == CrmSortOrder.Ascending
                                      ? OrderType.Ascending
                                      : OrderType.Descending;

            OrderExpression orderExpression = new OrderExpression(sortingCriteria.AttributeName, orderType);
            query.Orders.Add(orderExpression);
        }

        /// <summary>
        /// Creates a FilterExpression from a CrmQuery object
        /// </summary>
        /// <param name="query">The query to which the FilterExpression should be added</param>
        /// <param name="crmQuery">The CrmQuery object</param>
        /// <returns>A FilterExpression</returns>
        public static void AddFilterExpression(QueryExpression query, CrmQuery crmQuery)
        {
            FilterExpression filterExpression = CreateFilterExpression(crmQuery);
            if (filterExpression != null)
                query.Criteria.AddFilter(CreateFilterExpression(crmQuery));
        }

        public static FilterExpression CreateFilterExpression(CrmQuery crmQuery)
        {
            if (crmQuery == null)
                return null;

            FilterExpression filterExpression =
                new FilterExpression(OperatorMapper.GetMappedOperator(crmQuery.LogicalOperator));

            foreach (CrmAttributeCriterion condition in crmQuery.Conditions)
            {
                ConditionOperator conditionOperator = OperatorMapper.GetMappedOperator(condition.ConditionOperator);
                ConditionExpression conditionExpression = new ConditionExpression(condition.AttributeName, conditionOperator, condition.AttributeValue);
                filterExpression.AddCondition(conditionExpression);
            }
            return filterExpression;
        }

        /// <summary>
        /// Sets the PagingInfo property of the QueryExpression
        /// </summary>
        /// <param name="query">The query to which the PagingInfo should be set</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Page size</param>
        public static void AddPagingInfo(QueryExpression query, int page, int pageSize)
        {
            if (page > -1 && pageSize > -1)
                query.PageInfo = new PagingInfo { Count = pageSize, PageNumber = page, ReturnTotalRecordCount = true };
        }

        /// <summary>
        /// Creates and executes a RetrieveMultipleRequest to CRM
        /// </summary>
        /// <param name="service">The CRM Service</param>
        /// <param name="query">The query to be executed</param>
        /// <param name="totalCount">The total number of records found</param>
        /// <returns>A list of entities retrieved from CRM</returns>
        public static List<Entity> ExecuteQueryExpression(IOrganizationService service, QueryExpression query, out int totalCount)
        {
            RetrieveMultipleRequest multipleRequest = new RetrieveMultipleRequest();
            multipleRequest.Query = query;

            RetrieveMultipleResponse response = (RetrieveMultipleResponse)Execute(service, multipleRequest);
            totalCount = response.EntityCollection.TotalRecordCount;

            List<Entity> dynamicEntities = new List<Entity>();
            foreach (Entity businessEntity in response.EntityCollection.Entities)
            {
                dynamicEntities.Add(businessEntity);
            }

            return dynamicEntities;
        }

        /// <summary>
        /// Executes a request
        /// </summary>
        /// <param name="service">The CRM Service</param>
        /// <param name="request">The request to be executed</param>
        /// <returns>The response</returns>
        private static OrganizationResponse Execute(IOrganizationService service, OrganizationRequest request)
        {
            try
            {
                return service.Execute(request);
            }
            catch (System.Exception e)
            {
                if (e is SoapException)
                    throw new CrmSoapException((SoapException)e);
                throw new CrmException(e.Message);
            }
        }
    }
}
