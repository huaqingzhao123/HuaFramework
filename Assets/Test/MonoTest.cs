using HuaFramework.Architecture;
using HuaFramework.Configs;
using HuaFramework.IOC;
using HuaFramework.ResourcesRef;
using HuaFramework.TypeEvents;
using HuaFramework.Utility;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[ExecuteInEditMode]
public class MonoTest : MonoBehaviour,IOnEvent<EventA>
{
    [ContextMenuItem("执行操作","Excute")]//ContextMenuItem可为属性添加右键操作,点击执行名称为Excute的函数
    public AutoGenConfig autoGenConfig;
    TypeEventSystem typeEventSystem;

    public int Id;
    void Start()
    {


    }


    /// <summary>
    /// 测试引用置空后其它引用是否为空
    /// </summary>
    void IsNullTest()
    {
        var iocontainer = new IOCContainer();
        var temp = iocontainer;
        iocontainer = null;
        Debug.LogErrorFormat("iocontainer是否为null:{0},temp是否为null:{1}", iocontainer == null, temp == null);
        var array = new byte[1024];
        var tempArray = array;
        array = null;
        Debug.LogErrorFormat("array是否为null:{0},tempArray是否为null:{1}", array == null, tempArray == null);
    }

    void Excute()
    {
        Debug.LogError("我通过鼠标右键点击Inspector中的我执行了");
    }
    private void BindablePropertyTest()
    {
        BindableProperty<List<int>> property = new BindableProperty<List<int>>();
        List<int> test = new List<int>();
        BindableProperty<int> a = new BindableProperty<int>();
        int b = 0;
        b = a;
        test = property;
        if (test == (List<int>)property)
        {

        }
    }

    [ContextMenu("测试延时线程")]//ContextMenu也用于在组件上鼠标右键添加菜单项,同CONTEXT/组件名+按钮名
    private void TaskTest()
    {
        typeEventSystem = new TypeEventSystem();
        TaskTest taskTest = new TaskTest(typeEventSystem);
        typeEventSystem.Register<EventTaskStart>(EventTaskStartHandle);
        typeEventSystem.Register<EventTaskEnd>(EventTaskEndHandle);
        Task.Run(async () => {
            await taskTest.Test();
        });
    }
    private void EventTaskStartHandle(EventTaskStart obj)
    {
        Debug.LogError("计时任务开始");
    }

    private void EventTaskEndHandle(EventTaskEnd obj)
    {
        Debug.LogError("计时任务结束了！");

    }

    private void AssetBundleTest()
    {
#if UNITY_EDITOR
        var asset = AssetBundle.LoadFromFile(HotResUtil.LocalAssetBundleFolder + "Windows64");
        var assetmainst = asset.LoadAsset<AssetBundleManifest>("assetbundleManifest");
        var dependce = assetmainst.GetDirectDependencies("asset2");
        foreach (var item in dependce)
        {
            Debug.LogError("asset1依赖资源:" + item);
        }
# endif
    }


    private void TypeEventSystemTest()
    {
        typeEventSystem = new TypeEventSystem();
        typeEventSystem.Register<EventA>(a =>
        {
            Debug.LogErrorFormat("EventA的类型:{0}", a.GetType());
        }).UnRegisterWhenDestroy(gameObject);
        typeEventSystem.Register<EventB>(a =>
        {
            Debug.LogErrorFormat("EventB的类型:{0},数值:{1}", a.GetType(), a.a);
        }).UnRegisterWhenDestroy(gameObject);
        typeEventSystem.Register<EventGroup>(a =>
        {
            Debug.LogErrorFormat("EventC的类型:{0}", a.GetType());
        }).UnRegisterWhenDestroy(gameObject);
        typeEventSystem.Register<EventGroup>(a =>
        {
            Debug.LogErrorFormat("EventD的类型:{0}", a.GetType());
        }).UnRegisterWhenDestroy(gameObject);
        typeEventSystem.Send(new EventA());
        typeEventSystem.Send(new EventB() { a = 1 });
        //typeEventSystem.Send<EventC>(new EventC());
        //typeEventSystem.Send<EventD>(new EventD());
        typeEventSystem.Send<EventGroup>(new EventC());
        typeEventSystem.Send<EventGroup>(new EventD());
    }

    public void OnEvent(EventA e)
    {
        throw new System.NotImplementedException();
    }

}

public struct EventA
{

}
public struct EventB
{
   public int a;
}
public interface EventGroup { }

public struct EventC : EventGroup
{

}
public struct EventD : EventGroup
{

}
