
using System;

namespace XMLib.DataHandlers
{
    /// <summary>
    /// DataContractAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public class DataContractAttribute : Attribute
    {
        public string[] childNames = null;
        public bool isMulti { get { return childNames != null && childNames.Length > 0; } }
        public bool genericAllField { get; set; } = false;
        public bool genericFile { get; set; } = true;

        public DataContractAttribute(params string[] childNames)
        {
            this.childNames = childNames;
        }

        public DataContractAttribute()
        {
        }
    }
}