
#if !ASSET_RESOURCES && !UNITY_IOS
using UnityEngine;
using System.Reflection;
using System.IO;
using System;
using System.Collections.Generic;

namespace Nireus
{
    public class DynamicDLL
    {
		public static Assembly create(string dll_path, bool need_encrypt = true)
		{
			FileStream fs = null;
			byte[] dll_bytes = null;
			try
			{
				fs = new FileStream(dll_path, FileMode.Open);
				dll_bytes = new byte[fs.Length];
				fs.Read(dll_bytes, 0, (int)fs.Length);
			}
			finally
			{
				if (fs != null) fs.Close();
			}
			if (need_encrypt) Encryption.xor(dll_bytes);
			Assembly assembly = Assembly.Load(dll_bytes);
			return assembly;
		}
    }

    public class DynamicClass
    {
        private Type _type;
        private object _obj;

        public static Dictionary<string, Assembly> dll_assemblys = new Dictionary<string, Assembly>();

        private DynamicClass(Type type, object obj)
        {
            _type = type;
            _obj = obj;
        }

        public static DynamicClass create(string dll_path, String class_name)
        {
            Assembly assembly = DynamicDLL.create(dll_path);
            Type type = assembly.GetType(class_name, true, true);
            System.Object obj = Activator.CreateInstance(type, true);
            return new DynamicClass(type, obj);
        }

        public System.Object invoke(string func_name, object[] args)
        {
            return _type.InvokeMember(func_name, BindingFlags.InvokeMethod | BindingFlags.Public, null, _obj, args);
        }
    }
}
#endif