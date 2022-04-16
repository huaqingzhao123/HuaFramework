using UnityEngine;
using UnityEditor;
using HuaFramework.IOC;

namespace Assets.HuaFramework.Examples.Editor
{
    class IOCExample : MonoBehaviour
    {
        private void Start()
        {
            var container = new IOCContainer();
            container.Register<IStorage>(new PlayStorage());
            var storage = container.Get<IStorage>();
            storage.SaveString("name", "运行时存储");
            Debug.Log(storage.GetString("name"));
            container.Register<IStorage>(new EditorStorage());
            storage = container.Get<IStorage>();
            Debug.Log(storage.GetString("name"));
        }
    }
}
