using System;
using System.Collections.Generic;
using System.Linq;
using Epinova.CRMFramework.Exception;
using Epinova.CRMFramework.Interfaces;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace Epinova.CRMFramework
{
    class CrmMetaDataController
    {
        private EntityMetadata _crmEntityMetaData;
        private AttributeMetadata _attributeMetadata;
        private readonly IOrganizationService _service;

        public CrmMetaDataController(ICrmServiceFactory serviceFactory)
        {
            _service = serviceFactory.ServiceProxy;
        }

        public bool IsEntityPrimaryKey(string entityName, string attributeName)
        {
            if (_crmEntityMetaData == null)
                _crmEntityMetaData = GetCrmEntityMetaData(entityName);

            if (_crmEntityMetaData != null && _crmEntityMetaData.PrimaryIdAttribute == attributeName)
                return true;
            return false;
        }

        public Dictionary<int, string> GetOptionSet(string entityName, string attributeName)
        {
            Dictionary<int, string> optionSet = new Dictionary<int, string>();
            AttributeMetadata attributeMetaData = GetCrmAttributeMetaData(entityName, attributeName);
            if (attributeMetaData == null || !attributeMetaData.AttributeType.HasValue)
                return optionSet;

            OptionMetadataCollection optionMetadataCollection = null;
            if (attributeMetaData.AttributeType.Value == AttributeTypeCode.Picklist)
            {
                PicklistAttributeMetadata retrievedPicklistAttributeMetadata = (PicklistAttributeMetadata)attributeMetaData;
                optionMetadataCollection = retrievedPicklistAttributeMetadata.OptionSet.Options;
            }

            if (optionMetadataCollection != null && optionMetadataCollection.Count > 0)
            {
                foreach (OptionMetadata optionMetadata in optionMetadataCollection)
                {
                    if (optionMetadata.Value.HasValue)
                        optionSet.Add(optionMetadata.Value.Value, optionMetadata.Label.UserLocalizedLabel.Label);
                }
            }
            return optionSet;
        }

        public string GetOptionSetValueString(string entityName, string attributeName, int value)
        {
            AttributeMetadata attributeMetaData = GetCrmAttributeMetaData(entityName, attributeName);
            if (attributeMetaData == null || !attributeMetaData.AttributeType.HasValue)
                return string.Empty;

            if (attributeMetaData.AttributeType.Value == AttributeTypeCode.Picklist)
            {
                PicklistAttributeMetadata retrievedPicklistAttributeMetadata =
 (PicklistAttributeMetadata)attributeMetaData;
                return GetOptionSetLabelFromValue(retrievedPicklistAttributeMetadata.OptionSet.Options, value);
            }
            
            if (attributeMetaData.AttributeType.Value == AttributeTypeCode.Status)
            {
                StatusAttributeMetadata retrievedStatusAttributeMetadata =
                    (StatusAttributeMetadata)attributeMetaData;
                return GetOptionSetLabelFromValue(retrievedStatusAttributeMetadata.OptionSet.Options, value);
            }
            
            if (attributeMetaData.AttributeType.Value == AttributeTypeCode.State)
            {
                StateAttributeMetadata retrievedStatusAttributeMetadata =
                    (StateAttributeMetadata)attributeMetaData;
                return GetOptionSetLabelFromValue(retrievedStatusAttributeMetadata.OptionSet.Options, value);
            }
            return string.Empty;
        }

        /// <summary>
        /// Checks if the attributeMetaData is valid for creation and updates
        /// </summary>
        /// <param name="attributeMetadata">The attribute metadata</param>
        /// <param name="attributeName">The name of the attribute</param>
        /// <param name="isUpdate">Specifies if this operation is an update operation</param>
        /// <returns>A string containing an exception</returns>
        private void CheckIsAttributeMetaDataValid(AttributeMetadata attributeMetadata, string attributeName, string entityName, bool isUpdate)
        {
            if (attributeMetadata == null)
                throw new CrmException(String.Format("The specified CRM Attribute \"{0}\" does not exist", attributeName));

            if (isUpdate)
            {
                if (!attributeMetadata.IsValidForUpdate.HasValue || !attributeMetadata.IsValidForUpdate.Value)
                    throw new CrmException(String.Format("The specified CRM Attribute \"{0}\" for entity \"{1}\" is not valid for update", attributeName, entityName));
            }
            else
            {
                if (!attributeMetadata.IsValidForCreate.HasValue || !attributeMetadata.IsValidForCreate.Value)
                    throw new CrmException(String.Format("The specified CRM Attribute \"{0}\" for entity \"{1}\" is not valid for creation", attributeName, entityName));
            }
        }

        public object CreateCrmProperty(object propertyValue, string attributeName, string entityName, bool isUpdate, string referenceEntity, out bool isPrimaryKey)
        {
            if (_crmEntityMetaData == null)
                _crmEntityMetaData = GetCrmEntityMetaData(entityName);

            _attributeMetadata = GetAttributeMetaData(attributeName);

            // The Primary Attribute is never valid for update, but must be included in order for the update to be completed.
            if (!IsEntityPrimaryKey(entityName, attributeName))
            {
                CheckIsAttributeMetaDataValid(_attributeMetadata, attributeName, entityName, isUpdate);
                isPrimaryKey = false;
            }
            else
                isPrimaryKey = true;

            if (_attributeMetadata == null || !_attributeMetadata.AttributeType.HasValue)
                return null;

            AttributeTypeCode attributeMetaDataType = _attributeMetadata.AttributeType.Value;
            if (attributeMetaDataType.Equals(AttributeTypeCode.BigInt))
                return propertyValue;

            if (attributeMetaDataType.Equals(AttributeTypeCode.Boolean))
                return propertyValue;

            if (attributeMetaDataType.Equals(AttributeTypeCode.Customer))
                return GetEntityReferenceFromPropertyValue(_attributeMetadata, propertyValue, attributeName, referenceEntity);

            if (attributeMetaDataType.Equals(AttributeTypeCode.DateTime))
                return propertyValue;

            if (attributeMetaDataType.Equals(AttributeTypeCode.Decimal))
                return propertyValue;

            if (attributeMetaDataType.Equals(AttributeTypeCode.Double))
                return propertyValue;

            if (attributeMetaDataType.Equals(AttributeTypeCode.EntityName))
                return propertyValue;

            if (attributeMetaDataType.Equals(AttributeTypeCode.Integer))
                return propertyValue;

            if (attributeMetaDataType.Equals(AttributeTypeCode.Lookup))
                return GetEntityReferenceFromPropertyValue(_attributeMetadata, propertyValue, attributeName, referenceEntity);

            if (attributeMetaDataType.Equals(AttributeTypeCode.ManagedProperty))
                return GetBooleanManagedPropertyFromPropertyValue(propertyValue, attributeName);

            if (attributeMetaDataType.Equals(AttributeTypeCode.Memo))
                return propertyValue;

            if (attributeMetaDataType.Equals(AttributeTypeCode.Money))
                return GetMoneyPropertyFromPropertyValue(propertyValue, attributeName);

            if (attributeMetaDataType.Equals(AttributeTypeCode.Owner))
                return propertyValue;

            if (attributeMetaDataType.Equals(AttributeTypeCode.Picklist))
                return GetOptionSetValuePropertyFromPropertyValue(propertyValue, attributeName);

            if (attributeMetaDataType.Equals(AttributeTypeCode.State))
                return GetOptionSetValuePropertyFromPropertyValue(propertyValue, attributeName);

            if (attributeMetaDataType.Equals(AttributeTypeCode.Status))
                return GetOptionSetValuePropertyFromPropertyValue(propertyValue, attributeName);

            if (attributeMetaDataType.Equals(AttributeTypeCode.String))
                return propertyValue;

            if (attributeMetaDataType.Equals(AttributeTypeCode.Uniqueidentifier))
                return propertyValue;

            if (attributeMetaDataType.Equals(AttributeTypeCode.PartyList))
                return null; //TODO

            if (attributeMetaDataType.Equals(AttributeTypeCode.CalendarRules))
                return null; //TODO

            return null;
        }

        /// <summary>
        /// Creates an EntityReference object from a property value
        /// </summary>
        /// <param name="attributeMetadata">The attribute's metadata</param>
        /// <param name="propertyValue">The property value</param>
        /// <param name="attributeName">The attribute name</param>
        /// <param name="referenceEntity">The entity the lookup references</param>
        /// <returns>A EntityReference object</returns>
        private EntityReference GetEntityReferenceFromPropertyValue(AttributeMetadata attributeMetadata, object propertyValue, string attributeName, string referenceEntity)
        {
            IsValidLookUpTarget(attributeMetadata, referenceEntity);

            if (propertyValue is Guid)
            {
                Guid value = (Guid)propertyValue;
                if (value != Guid.Empty)
                    return new EntityReference(referenceEntity, value);
                return null;
            }
            throw new CrmException(String.Format("Property for CRM Attribute \"{0}\" must be of type Guid", attributeName));
        }

        /// <summary>
        /// Checks if the referenceEntity is among the lookupAttributeMetadata Targets.
        /// </summary>
        /// <param name="attributeMetadata">The Lookup attribute's metadata</param>
        /// <param name="referenceEntity">The reference entity supplied by the user</param>
        private void IsValidLookUpTarget(AttributeMetadata attributeMetadata, string referenceEntity)
        {
            LookupAttributeMetadata lookupAttributeMetadata = (LookupAttributeMetadata)attributeMetadata;
            string validValues = String.Empty;
            foreach (string target in lookupAttributeMetadata.Targets)
            {
                if (target == referenceEntity)
                    return;

                if (validValues == String.Empty)
                    validValues += target;
                else
                    validValues += ", " + target;
            }
            throw new CrmException(String.Format("CRM Attribute \"{0}\" does not have a valid ReferenceEntity \"{1}\". The valid ReferenceEntities are: \"{2}\" ", lookupAttributeMetadata.LogicalName, referenceEntity, validValues));
        }

        /// <summary>
        /// Creates a Money object from a property value
        /// </summary>
        /// <param name="propertyValue">The property value</param>
        /// <param name="attributeName">The attribute name</param>
        /// <returns>A Money object</returns>
        private Money GetMoneyPropertyFromPropertyValue(object propertyValue, string attributeName)
        {
            if (propertyValue is decimal)
            {
                decimal value = (decimal)propertyValue;
                return new Money(value);
            }
            throw new CrmException(String.Format("Property for CRM Attribute \"{0}\" must be of type decimal", attributeName));
        }

        /// <summary>
        /// Creates a OptionSetValue object from a property value
        /// </summary>
        /// <param name="propertyValue">The property value</param>
        /// <param name="attributeName">The attribute name</param>
        /// <returns>A OptionSetValue object</returns>
        private OptionSetValue GetOptionSetValuePropertyFromPropertyValue(object propertyValue, string attributeName)
        {
            if (propertyValue is int)
            {
                int value = (int)propertyValue;
                return new OptionSetValue(value);
            }
            if (propertyValue is string)
            {
                string label = (string)propertyValue;
                int value = GetOptionSetValueFromLabel(label);
                if (value != -1)
                    return new OptionSetValue(value);
            }
            throw new CrmException(String.Format("Property for CRM Attribute \"{0}\" must be of type int", attributeName));
        }

        /// <summary>
        /// Creates a BooleanManagedProperty from a property value
        /// </summary>
        /// <param name="propertyValue">The property value</param>
        /// <param name="attributeName">Name of the attribute</param>
        /// <returns>A BooleanManagedProperty object</returns>
        private BooleanManagedProperty GetBooleanManagedPropertyFromPropertyValue(object propertyValue, string attributeName)
        {
            if (propertyValue is bool)
            {
                bool value = (bool)propertyValue;
                return new BooleanManagedProperty(value);
            }
            throw new CrmException(String.Format("Property for CRM Attribute \"{0}\" must be of type bool", attributeName));
        }

        private string GetOptionSetLabelFromValue(OptionMetadataCollection optionMetadataCollection, int value)
        {
            foreach (OptionMetadata optionMetadata in optionMetadataCollection)
            {
                if (optionMetadata.Value.HasValue && optionMetadata.Value == value)
                    return optionMetadata.Label.UserLocalizedLabel.Label;
            }
            return string.Empty;
        }

        /// <summary>
        /// Gets the value of a OptionSet by it's value
        /// </summary>
        /// <param name="label">The label</param>
        /// <returns>The value</returns>
        private int GetOptionSetValueFromLabel(string label)
        {
            if (!_attributeMetadata.AttributeType.HasValue)
                return -1;

            OptionMetadataCollection optionMetadataCollection = null;
            if (_attributeMetadata.AttributeType.Value == AttributeTypeCode.Picklist)
            {
                PicklistAttributeMetadata retrievedPicklistAttributeMetadata =
 (PicklistAttributeMetadata)_attributeMetadata;
                optionMetadataCollection = retrievedPicklistAttributeMetadata.OptionSet.Options;
            }
            else if (_attributeMetadata.AttributeType.Value == AttributeTypeCode.Status)
            {
                StatusAttributeMetadata retrievedStatusAttributeMetadata =
                    (StatusAttributeMetadata)_attributeMetadata;
                optionMetadataCollection = retrievedStatusAttributeMetadata.OptionSet.Options;
            }
            else if (_attributeMetadata.AttributeType.Value == AttributeTypeCode.State)
            {
                StateAttributeMetadata retrievedStatusAttributeMetadata =
                    (StateAttributeMetadata)_attributeMetadata;
                optionMetadataCollection = retrievedStatusAttributeMetadata.OptionSet.Options;
            }

            foreach (OptionMetadata optionMetadata in optionMetadataCollection)
            {
                if (optionMetadata.Label.UserLocalizedLabel.Label == label)
                    return optionMetadata.Value.HasValue ? optionMetadata.Value.Value : -1;
            }
            return -1;
        }

        /// <summary>
        /// Gets the attribute metadata for an attribute
        /// </summary>
        /// <param name="attributeName">Name of the attribute</param>
        /// <returns>The attributes metadata</returns>
        private AttributeMetadata GetAttributeMetaData(string attributeName)
        {
            try
            {
                return _crmEntityMetaData.Attributes.Single(meta => meta.LogicalName == attributeName);
            }
            catch (System.Exception e)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the schema name of the many to many relationship between fromEntityName and toEntityName
        /// </summary>
        /// <param name="fromEntityName">Name of entity</param>
        /// <param name="toEntityName">Name of entity</param>
        /// <returns>Schema name of relationship</returns>
        public string GetManyToManyRelationshipSchemaName(string fromEntityName, string toEntityName)
        {
            ManyToManyRelationshipMetadata[] manyToManyMetadata = GetCrmEntityManyToManyRelationships(fromEntityName);
            foreach (ManyToManyRelationshipMetadata metadata in manyToManyMetadata)
            {
                if (metadata.Entity1LogicalName == toEntityName && metadata.Entity2LogicalName == fromEntityName)
                {
                    return metadata.IntersectEntityName;
                }
                if (metadata.Entity2LogicalName == toEntityName && metadata.Entity1LogicalName == fromEntityName)
                {
                    return metadata.IntersectEntityName;
                }
            }
            throw new CrmException(String.Format("Could not find a many to many relationship between {0} and {1}", fromEntityName, toEntityName));
        }

        private ManyToManyRelationshipMetadata[] GetCrmEntityManyToManyRelationships(string entityName)
        {
            EntityMetadata entityMetadata = GetCrmEntityMetaData(entityName, EntityFilters.Relationships);
            return entityMetadata.ManyToManyRelationships;
        }

        public string GetPrimaryKeyAttributeScemaName(string entityName)
        {
            EntityMetadata entityMetadata = GetCrmEntityMetaData(entityName);
            return entityMetadata.PrimaryIdAttribute;
        }

        private AttributeMetadata GetCrmAttributeMetaData(string entityName, string attributeName)
        {
            RetrieveAttributeResponse response = ExecuteRetrieveAttributeRequest(entityName, attributeName);
            return response.AttributeMetadata;
        }

        /// <summary>
        /// Gets the entity metadata for an entity
        /// </summary>
        /// <param name="entityName">Name of the entity</param>
        /// <param name="entityFilters">EntityFilter to use</param>
        /// <returns>The entity's metadata</returns>
        private EntityMetadata GetCrmEntityMetaData(string entityName, EntityFilters entityFilters = EntityFilters.Attributes)
        {
            RetrieveEntityResponse response = ExecuteRetrieveEntityRequest(entityName, entityFilters);
            return response.EntityMetadata;
        }

        private RetrieveAttributeResponse ExecuteRetrieveAttributeRequest(string entityName, string attributeName)
        {
            RetrieveAttributeRequest request = new RetrieveAttributeRequest();
            request.EntityLogicalName = entityName;
            request.LogicalName = attributeName;
            request.RetrieveAsIfPublished = true;
            return (RetrieveAttributeResponse)_service.Execute(request);
        }

        /// <summary>
        /// Executes an entity request query
        /// </summary>
        /// <param name="entityName">Name of entity to retrieve</param>
        /// <returns>Response of query</returns>
        private RetrieveEntityResponse ExecuteRetrieveEntityRequest(string entityName, EntityFilters entityFilters)
        {
            RetrieveEntityRequest request = new RetrieveEntityRequest();
            request.LogicalName = entityName;
            request.EntityFilters = entityFilters;
            return (RetrieveEntityResponse)_service.Execute(request);
        }
    }
}
