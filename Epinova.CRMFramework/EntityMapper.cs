using System;
using System.Collections.Generic;
using System.Reflection;
using Epinova.CRMFramework.Attributes;
using Microsoft.Xrm.Sdk.Query;

namespace Epinova.CRMFramework
{
    static class EntityMapper
    {
        /// <summary>
        /// Creates a mapping dictionary containing the name of the attribute as a key and the 
        /// corresponding PropertyInfo object as a value
        /// </summary>
        /// <returns>The mapping dictionary</returns>
        public static Dictionary<PropertyInfo, string> CreateMapping(Type type)
        {
            Dictionary<PropertyInfo, string> mapping = new Dictionary<PropertyInfo, string>();
            PropertyInfo[] propertyInfo = type.GetProperties();

            foreach (PropertyInfo info in propertyInfo)
            {
                object[] attributes = info.GetCustomAttributes(typeof(CrmAttributeAttribute), false);
                if (attributes.Length == 1)
                {
                    string attributeName = ((CrmAttributeAttribute)attributes[0]).CrmAttributeName;
                    if (!mapping.ContainsKey(info))
                    {
                        mapping.Add(info, attributeName);
                    }
                }
            }
            return mapping;
        }

        /// <summary>
        /// Creates a ColumnSet object based on the keys in the mapping dictionary
        /// </summary>
        /// <returns>A ColumnSet</returns>
        public static ColumnSet GetColumnsBasedOnMapping(Dictionary<PropertyInfo, string> mapping)
        {
            // Create the column set object that indicates the properties to be retrieved.
            ColumnSet cols = new ColumnSet();
            foreach (string attribute in mapping.Values)
            {
                cols.AddColumn(attribute);
            }
            return cols;
        }

        /// <summary>
        /// Gets a list of PropertyInfo objects based on the value of _mapping
        /// </summary>
        /// <param name="value">Value to find keys for</param>
        /// <param name="mapping">Mapping dictionary</param>
        /// <returns>A list of PropertyInfo objects</returns>
        public static List<PropertyInfo> FindMappingKeyByMappingValue(string value, Dictionary<PropertyInfo, string> mapping)
        {
            List<PropertyInfo> keys = new List<PropertyInfo>();
            foreach (PropertyInfo key in mapping.Keys)
            {
                if (mapping[key] == value)
                    keys.Add(key);
            }
            return keys;
        }
    }
}
