using UnityEngine;
using UnityEngine.UI;

namespace Nireus
{
    public class Delegate
	{
		public delegate void CallFuncVoid();
		public delegate void CallFuncBool(bool b);
		public delegate void CallFuncInt(int i);
        public delegate void CallFuncObject(System.Object obj);
        public delegate void CallFuncString(string s);

#if !ASSET_RESOURCES
        public delegate void CallFuncAssetBundle(AssetBundle ab);
#endif

		public delegate void CallFuncNetData(NetData net_data);

        public delegate void CallFuncButton(Button ui_base);

        public delegate void VoidDelegate(GameObject go);
    }
}
