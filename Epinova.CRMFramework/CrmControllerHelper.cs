using System;
using System.Collections.Generic;
using Epinova.CRMFramework.Attributes;
using Microsoft.Xrm.Sdk;

namespace Epinova.CRMFramework
{
    class CrmControllerHelper
    {
        private readonly CrmMetaDataController _crmMetaDataController;
        public CrmControllerHelper(CrmMetaDataController crmMetaDataController)
        {
            _crmMetaDataController = crmMetaDataController;
        }

        /// <summary>
        /// Gets an attribute's value
        /// </summary>
        /// <param name="attributeValue">A CRM attribute value as an object</param>
        /// <param name="propertyInfoType">The PropertyInfo's type</param>
        /// <returns>The propery's value</returns>
        internal object GetCrmPropertyValue(object attributeValue, Type propertyInfoType, string entityName, string attributeName)
        {
            Type type = attributeValue.GetType();

            if (type == typeof(Money))
                return ((Money)attributeValue).Value;

            if (type == typeof(EntityReference))
            {
                if (propertyInfoType == typeof(Guid))
                    return ((EntityReference)attributeValue).Id;
                if (propertyInfoType == typeof(string))
                    return ((EntityReference)attributeValue).Name;
            }

            if (type == typeof(OptionSetValue))
            {
                int value = ((OptionSetValue)attributeValue).Value;
                if (propertyInfoType == typeof(int))
                    return value;
                if (propertyInfoType == typeof(string))
                    return _crmMetaDataController.GetOptionSetValueString(entityName, attributeName, value);
                if (propertyInfoType == typeof (Dictionary<int, string>))
                    return _crmMetaDataController.GetOptionSet(entityName, attributeName);
            }

            if (type == typeof(EntityCollection))
            {
                List<Guid> collection = new List<Guid>(); 
                DataCollection<Entity> entities = ((EntityCollection) attributeValue).Entities;
                foreach (Entity entity in entities)
                {
                    collection.Add(entity.Id);
                }
                return collection;
            }

            return attributeValue;
        }

        /// <summary>
        /// Gets the entity's CRM name from the Class Attribute
        /// </summary>
        /// <returns>The entity's CRM name</returns>
        internal string GetEntitySchemaName(Type type)
        {
            object[] typeAttributes = type.GetCustomAttributes(typeof(CrmEntityAttribute), false);
            if (typeAttributes.Length == 1)
                return ((CrmEntityAttribute)typeAttributes[0]).CrmEntityName;
            return string.Empty;
        }
    }
}
