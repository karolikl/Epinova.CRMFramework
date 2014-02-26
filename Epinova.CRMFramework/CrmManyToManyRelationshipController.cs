using System;
using System.Collections.Generic;
using System.Reflection;
using Epinova.CRMFramework.Exception;
using Epinova.CRMFramework.Interfaces;
using Epinova.CRMFramework.Query;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Epinova.CRMFramework
{
    public class CrmManyToManyRelationshipController<T, V>
        where T : class, new()
        where V : class, new()
    {
        private readonly IOrganizationService _service;
        private readonly Dictionary<PropertyInfo, string> _mappingT;
        private readonly Dictionary<PropertyInfo, string> _mappingV;
        private readonly CrmMetaDataController _crmMetaDataController;
        private readonly CrmControllerHelper _crmControllerHelper;

        private string EntityNameT
        {
            get
            {
                return _crmControllerHelper.GetEntitySchemaName(typeof(T));
            }
        }

        private string EntityNameV
        {
            get
            {
                return _crmControllerHelper.GetEntitySchemaName(typeof(V));
            }
        }

        public CrmManyToManyRelationshipController(ICrmServiceFactory serviceFactory)
        {
            _service = serviceFactory.ServiceProxy;
            _mappingT = EntityMapper.CreateMapping(typeof(T));
            _mappingV = EntityMapper.CreateMapping(typeof(V));
            _crmMetaDataController = new CrmMetaDataController(serviceFactory);
            _crmControllerHelper = new CrmControllerHelper(_crmMetaDataController);
        }

        public List<V> Find(CrmQuery query)
        {
            return Find(query, null);
        }

        public List<V> Find(CrmQuery query, CrmSortingCriteria sortingCriteria)
        {
            int totalCount;
            return Find(query, -1, -1, out totalCount, sortingCriteria);
        }

        public List<V> Find(CrmQuery query, int page, int pageSize, out int totalCount, CrmSortingCriteria sortingCriteria)
        {
            List<Entity> dynamicEntities = CreateAndExecuteQueryExpression(query, page, pageSize, out totalCount, sortingCriteria);
            return MapDynamicEntityList(dynamicEntities);
        }

        private List<Entity> CreateAndExecuteQueryExpression(CrmQuery query, int page, int pageSize, out int totalCount, CrmSortingCriteria sortingCriteria)
        {
            FilterExpression filterExpression = QueryHelper.CreateFilterExpression(query);

            string entityPrimaryKeyAttribute = GetCrmEntityPrimaryKeyAttribute(typeof(T));
            string returnedEntityPrimaryKeyAttribute = GetCrmEntityPrimaryKeyAttribute(typeof(V));

            LinkEntity filterLinkEntity = new LinkEntity();
            filterLinkEntity.LinkToEntityName = EntityNameT;
            filterLinkEntity.LinkFromAttributeName = entityPrimaryKeyAttribute;
            filterLinkEntity.LinkToAttributeName = entityPrimaryKeyAttribute;
            filterLinkEntity.LinkCriteria = filterExpression;

            LinkEntity linkTableEntity = new LinkEntity();
            linkTableEntity.LinkToEntityName = GetManyToManyRelationshipSchemaName();
            linkTableEntity.LinkFromAttributeName = returnedEntityPrimaryKeyAttribute;
            linkTableEntity.LinkToAttributeName = returnedEntityPrimaryKeyAttribute;

            linkTableEntity.LinkEntities.Add(filterLinkEntity);

            QueryExpression queryExpression = new QueryExpression();
            queryExpression.EntityName = EntityNameV;
            queryExpression.ColumnSet = EntityMapper.GetColumnsBasedOnMapping(_mappingV);
            queryExpression.LinkEntities.Add(linkTableEntity);

            QueryHelper.AddOrderExpression(queryExpression, sortingCriteria);
            QueryHelper.AddPagingInfo(queryExpression, page, pageSize);

            return QueryHelper.ExecuteQueryExpression(_service, queryExpression, out totalCount);
        }

        /// <summary>
        /// Loops through a list of Dynamic entities and maps them to the correct type
        /// </summary>
        /// <param name="entities">List of entities</param>
        /// <returns>List of objects of type T</returns>
        private List<V> MapDynamicEntityList(List<Entity> entities)
        {
            List<V> mappedEntities = new List<V>();
            foreach (Entity entity in entities)
            {
                V mappedEntity = MapDynamicEntity(entity);
                mappedEntities.Add(mappedEntity);
            }
            return mappedEntities;
        }

        /// <summary>
        /// Maps the DynamicEntity to the correct type
        /// </summary>
        /// <param name="entity">The Entity</param>
        /// <returns>Object of type T</returns>
        private V MapDynamicEntity(Entity entity)
        {
            if (entity == null)
                return null;

            V retrievedObject = new V();
            foreach (KeyValuePair<string, object> attribute in entity.Attributes)
                {
                    if (GetMappingForType(typeof(V)).ContainsValue(attribute.Key))
                    {
                        List<PropertyInfo> propertyInfos = EntityMapper.FindMappingKeyByMappingValue(attribute.Key, _mappingV);
                        foreach (PropertyInfo propertyInfo in propertyInfos)
                        {
                            try
                            {
                                object propertyValue = _crmControllerHelper.GetCrmPropertyValue(attribute.Value, propertyInfo.PropertyType, entity.LogicalName, attribute.Key);
                                propertyInfo.SetValue(retrievedObject, propertyValue, null);
                            }
                            catch (System.Exception e)
                            {
                                throw new CrmException(e.Message, entity.LogicalName, attribute.Key);
                            }
                        }
                    }
                }
            return retrievedObject;
        }

        private string GetManyToManyRelationshipSchemaName()
        {
            return _crmMetaDataController.GetManyToManyRelationshipSchemaName(EntityNameT, EntityNameV);
        }

        /// <summary>
        /// Gets the CRM schemaname of the entitys primary key attribute
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>CRM schemaname</returns>
        private string GetCrmEntityPrimaryKeyAttribute(Type type)
        {
            string entityName = _crmControllerHelper.GetEntitySchemaName(type);
            return _crmMetaDataController.GetPrimaryKeyAttributeScemaName(entityName);
        }

        /// <summary>
        /// Gets a mapping based on type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Mapping</returns>
        private Dictionary<PropertyInfo, string> GetMappingForType(Type type)
        {
            if (type == typeof(T))
                return _mappingT;
            if (type == typeof(V))
                return _mappingV;
            return null;
        }
    }
}
