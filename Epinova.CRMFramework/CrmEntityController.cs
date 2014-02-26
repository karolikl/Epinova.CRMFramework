using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Services.Protocols;
using Epinova.CRMFramework.Attributes;
using Epinova.CRMFramework.Exception;
using Epinova.CRMFramework.Interfaces;
using Epinova.CRMFramework.Query;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Epinova.CRMFramework
{
    public class CrmEntityController<T> where T : class, new()
    {
        private readonly IOrganizationService _service;
        private readonly CrmControllerHelper _crmControllerHelper;
        private readonly Dictionary<PropertyInfo, string> _mapping;
        private readonly CrmMetaDataController _crmMetaDataController;

        private string EntityName
        {
            get
            {
                return _crmControllerHelper.GetEntitySchemaName(typeof(T));
            }
        }

        public CrmEntityController()
        {
        }

        public CrmEntityController(ICrmServiceFactory serviceFactory)
        {
            _service = serviceFactory.ServiceProxy;
            _mapping = EntityMapper.CreateMapping(typeof(T));
            _crmMetaDataController = new CrmMetaDataController(serviceFactory);
            _crmControllerHelper = new CrmControllerHelper(_crmMetaDataController);
        }

        public T Get(Guid guid)
        {
            Entity entity = GetEntityFromCrm(guid);
            return MapDynamicEntity(entity);
        }

        public List<T> FindAll()
        {
            return FindAll(null);
        }

        public List<T> FindAll(CrmSortingCriteria sortingCriteria)
        {
            int totalCount;
            List<Entity> entities = RetrieveMultipleEntities(null, -1, -1, out totalCount, sortingCriteria);
            return MapDynamicEntityList(entities);
        }

        public List<T> FindAll(int page, int pageSize, out int totalCount, CrmSortingCriteria sortingCriteria)
        {
            List<Entity> dynamicEntities = RetrieveMultipleEntities(null, page, pageSize, out totalCount, sortingCriteria);
            return MapDynamicEntityList(dynamicEntities);
        }

        public List<T> Find(CrmQuery query)
        {
            return Find(query, null);
        }

        public List<T> Find(CrmQuery query, CrmSortingCriteria sortingCriteria)
        {
            int totalCount;
            return Find(query, -1, -1, out totalCount, sortingCriteria);
        }

        public List<T> Find(CrmQuery query, int page, int pageSize, out int totalCount, CrmSortingCriteria sortingCriteria)
        {
            List<Entity> dynamicEntities = RetrieveMultipleEntities(query, page, pageSize, out totalCount, sortingCriteria);
            return MapDynamicEntityList(dynamicEntities);
        }

        public Guid Create(T entity)
        {
            Entity dynamicEntity = CreateDynamicEntityFromT(entity, false);
            return CreateEntity(dynamicEntity);
        }

        public void Update(T entity)
        {
            Entity dynamicEntity = CreateDynamicEntityFromT(entity, true);
            UpdateEntity(dynamicEntity);
        }

        public void Delete(Guid guid)
        {
            DeleteEntity(guid);
        }

        //#region Private methods
        /// <summary>
        /// Deletes a business entity
        /// </summary>
        /// <param name="guid">The guid of the entity to be deleted</param>
        private void DeleteEntity(Guid guid)
        {
            try
            {
                _service.Delete(EntityName, guid);
            }
            catch (System.Exception e)
            {
                if (e is SoapException)
                {
                    throw new CrmSoapException((SoapException)e);
                }
                throw new CrmException(e.Message);
            }
        }

        /// <summary>
        /// Creates an entity
        /// </summary>
        /// <param name="entity">The entity to be creates</param>
        /// <returns>The guid of the created entity</returns>
        private Guid CreateEntity(Entity entity)
        {
            try
            {
                return _service.Create(entity);
            }
            catch (System.Exception e)
            {
                if (e is SoapException)
                {
                    throw new CrmSoapException((SoapException)e);
                }
                throw new CrmException(e.Message);
            }
        }

        /// <summary>
        /// Updates a business entity
        /// </summary>
        /// <param name="entity">The business entity to be updated</param>
        private void UpdateEntity(Entity entity)
        {
            try
            {
                _service.Update(entity);
            }
            catch (System.Exception e)
            {
                if (e is SoapException)
                {
                    throw new CrmSoapException((SoapException)e);
                }
                throw new CrmException(e.Message);
            }
        }

        /// <summary>
        /// Creates an entity from T
        /// </summary>
        /// <param name="entity">The entity to create a dynamic entity of</param>
        /// <param name="isUpdate">True if this is an update operation</param>
        /// <returns>An entity</returns>
        private Entity CreateDynamicEntityFromT(T entity, bool isUpdate)
        {
            Entity newEntity = new Entity();
            newEntity.LogicalName = EntityName;

            foreach (KeyValuePair<PropertyInfo, string> keyValuePair in _mapping)
            {
                PropertyInfo propertyInfo = keyValuePair.Key;
                object propertyValue = propertyInfo.GetValue(entity, null);

                if (propertyValue != null)
                {
                    string attributeName = keyValuePair.Value;
                    string referenceEntity = GetCrmReferenceEntityFromCustomAttributes(propertyInfo);
                    bool isPrimaryKey;
                    object property = _crmMetaDataController.CreateCrmProperty(propertyValue, attributeName, EntityName, isUpdate, referenceEntity, out isPrimaryKey);

                    if (property != null)
                    {
                        if (isPrimaryKey)
                            newEntity.Id = (Guid)propertyValue;
                        else
                            newEntity.Attributes.Add(attributeName, property);
                    }
                }
            }
            return newEntity;
        }

        /// <summary>
        /// Loops through a list of entities and maps them to the correct type
        /// </summary>
        /// <param name="entities">List of entities</param>
        /// <returns>List of objects of type T</returns>
        private List<T> MapDynamicEntityList(List<Entity> entities)
        {
            List<T> mappedEntities = new List<T>();
            foreach (Entity entity in entities)
            {
                T mappedEntity = MapDynamicEntity(entity);
                mappedEntities.Add(mappedEntity);
            }
            return mappedEntities;
        }

        /// <summary>
        /// Maps the DynamicEntity to the correct type
        /// </summary>
        /// <param name="entity">The entity to map</param>
        /// <returns>Object of type T</returns>
        private T MapDynamicEntity(Entity entity)
        {
            if (entity == null)
                return null;

            T retrievedObject = new T();
            foreach (KeyValuePair<string, object> attribute in entity.Attributes)
            {
                if (_mapping.ContainsValue(attribute.Key))
                {
                    List<PropertyInfo> propertyInfos = EntityMapper.FindMappingKeyByMappingValue(attribute.Key, _mapping);
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

        /// <summary>
        /// Creates a QueryExpression with sorting, and executes the expression
        /// </summary>
        /// <returns>A list of entities retrieved with the query</returns>
        private List<Entity> RetrieveMultipleEntities(CrmQuery crmQuery, int page, int pageSize, out int totalCount, CrmSortingCriteria sortingCriteria)
        {
            QueryExpression query = new QueryExpression();
            query.EntityName = EntityName;
            query.ColumnSet = EntityMapper.GetColumnsBasedOnMapping(_mapping);

            QueryHelper.AddFilterExpression(query, crmQuery);
            QueryHelper.AddOrderExpression(query, sortingCriteria);
            QueryHelper.AddPagingInfo(query, page, pageSize);

            return QueryHelper.ExecuteQueryExpression(_service, query, out totalCount);
        }
 
        /// <summary>
        /// Gets an entity from CRM
        /// </summary>
        /// <param name="guid">Guid of the entity</param>
        /// <returns>An entity</returns>
        protected internal Entity GetEntityFromCrm(Guid guid)
        {
            Entity account = new Entity(EntityName);

            // Create a column set to define which attributes should be retrieved.
            ColumnSet attributes = EntityMapper.GetColumnsBasedOnMapping(_mapping);

            // Retrieve the account and its name and ownerid attributes.
            return _service.Retrieve(account.LogicalName, guid, attributes);
        }

        /// <summary>
        /// Gets the value of ReferenceEntity from the PropertyInfo's custom attributes
        /// </summary>
        /// <param name="info">A PropertyInfo object for the property</param>
        /// <returns>The value of the propertys ReferenceEntity</returns>
        private static string GetCrmReferenceEntityFromCustomAttributes(PropertyInfo info)
        {
            object[] attributes = info.GetCustomAttributes(typeof(CrmAttributeAttribute), false);
            if (attributes.Length == 1)
            {
                return ((CrmAttributeAttribute)attributes[0]).ReferenceEntity;
            }
            return null;
        }
    }
}
