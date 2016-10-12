
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;


namespace FJW.SDK2Api
{

    /// <summary>
    /// Api的 config 配置
    /// </summary>
    public class ApiSection: ConfigurationSection
    {
        [ConfigurationProperty("Methods", IsDefaultCollection = true)]
        public MethodCollection Methods { get { return (MethodCollection)base["Methods"]; } }

       
    }

    public class MethodCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new MethodElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((MethodElement)element).Method;
        }
        public IEnumerable<string> AllKeys
        {
            get { return BaseGetAllKeys().Cast<string>(); }
        }

        public new MethodElement this[string name]
        {
            get { return (MethodElement)BaseGet(name); }
        }
    }

    public class MethodElement : ConfigurationElement
    {
        [ConfigurationProperty("Method", IsRequired = true, IsKey = true)]
        public string Method
        {
            get { return base["Method"].ToString(); }
            set { base["Method"] = value; }
        }

        [ConfigurationProperty("EntryPoint", IsRequired = true)]
        public string EntryPoint
        {
            get { return base["EntryPoint"].ToString(); }
            set { base["EntryPoint"] = value; }
        }
    }
}
