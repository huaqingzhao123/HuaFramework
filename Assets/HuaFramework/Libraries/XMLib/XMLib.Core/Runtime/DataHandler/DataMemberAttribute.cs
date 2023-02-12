

using System;

namespace XMLib.DataHandlers
{
    /// <summary>
    /// DataMemberAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class DataMemberAttribute : Attribute
    {
        public string aliasName { get; private set; } = null;
        public DataMemberAttribute(string aliasName)
        {
            this.aliasName = aliasName;
        }
        
        public DataMemberAttribute()
        {
        }
    }
}