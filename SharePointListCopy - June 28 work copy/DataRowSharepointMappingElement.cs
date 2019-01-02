
//MyCompany.MyProject.csproj which results in MyCompany.MyProject.dll
//Add a Folder called "Configuration"

namespace SharePointListCopy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using System.Configuration;


    public class DataRowSharepointMapping : ConfigurationElement
    {

        private const string SharePointColumn_NAME = "SharePointColumn";
        private const string DataRowColumn_FOLDER = "DataRowColumn";

        [ConfigurationProperty(SharePointColumn_NAME, DefaultValue = "", IsKey = false, IsRequired = true)]
        public string SharePointColumn
        {
            get
            {
                return ((string)(base[SharePointColumn_NAME]));
            }
            set
            {
                base[SharePointColumn_NAME] = value;
            }
        }

        [ConfigurationProperty(DataRowColumn_FOLDER, DefaultValue = "", IsKey = true, IsRequired = true)]
        public string DataRowColumn
        {
            get
            {
                return ((string)(base[DataRowColumn_FOLDER]));
            }
            set
            {
                base[DataRowColumn_FOLDER] = value;
            }
        }



    }

    //-----------------------------------------------------------------------

    //-----------------------------------------------------------------------

    [ConfigurationCollection(typeof(DataRowSharepointMapping))]
    public class DataRowSharepointMappingCollection : ConfigurationElementCollection
    {

        protected override ConfigurationElement CreateNewElement()
        {
            return new DataRowSharepointMapping();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DataRowSharepointMapping)(element)).DataRowColumn;
        }


        public DataRowSharepointMapping this[int idx]
        {
            get
            {
                return (DataRowSharepointMapping)BaseGet(idx);
            }
        }

        new public DataRowSharepointMapping this[string key]
        {
            get
            {
                return (DataRowSharepointMapping)BaseGet(key);
            }
        }
    }

    //-----------------------------------------------------------------------

    //-----------------------------------------------------------------------

    public class DataRowSharepointMappingConfigSection : ConfigurationSection
    {
        private const string TRANSFORMATION_TO_DIRECTORY_MAPPINGS = "DataRowSharepointMappings";

        [ConfigurationProperty(TRANSFORMATION_TO_DIRECTORY_MAPPINGS)]
        public DataRowSharepointMappingCollection TransformationToDirectoryMappingItems
        {
            get { return ((DataRowSharepointMappingCollection)(base[TRANSFORMATION_TO_DIRECTORY_MAPPINGS])); }
        }
    }

    //-----------------------------------------------------------------------

    //-----------------------------------------------------------------------

    public static class MyRetriever
    {
        public static readonly string MAPPINGS_CONFIGURATION_SECTION_NAME = "DataRowSharepointMappingsSection";

        public static DataRowSharepointMappingCollection GetTheCollection()
        {
            DataRowSharepointMappingConfigSection mappingsSection = (DataRowSharepointMappingConfigSection)ConfigurationManager.GetSection(MAPPINGS_CONFIGURATION_SECTION_NAME);
            if (mappingsSection != null)
            {
                return mappingsSection.TransformationToDirectoryMappingItems;
            }
            return null; // OOPS!

        }

        public static DataRowSharepointMappingCollection GetTheCollection(string ConfigSectionName)
        {
            DataRowSharepointMappingConfigSection mappingsSection = (DataRowSharepointMappingConfigSection)ConfigurationManager.GetSection(ConfigSectionName);
            if (mappingsSection != null)
            {
                return mappingsSection.TransformationToDirectoryMappingItems;
            }
            return null; // OOPS!

        }
    }

}