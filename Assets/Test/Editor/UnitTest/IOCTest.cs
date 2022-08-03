using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using HuaFramework.IOC;
using System;

public class IOCTest 
{

    [Test]
   public void IOCGain()
    {
        ClassicIOCContainer classicIOCContainer = new ClassicIOCContainer();
        var taskTest = classicIOCContainer.Resolve<TaskTest>();
        Assert.IsNull(taskTest);
    }
    [Test]
    public void HashSetTest()
    {
        var hashList = new HashSet<Type>();
        var taskTest = typeof(TaskTest);
        //成功
        var bool1= hashList.Add(taskTest);
        //插入失败
        var bool2 = hashList.Add(taskTest);
        Assert.IsTrue(bool1);
        Assert.IsFalse(bool2);
    }


    [Test]
    public void IOC_RegisterInstance()
    {
        var iocContainer = new ClassicIOCContainer();
        iocContainer.RegisterInstance(new ClassicIOCContainer());
        iocContainer.RegisterInstance(new ClassicIOCContainer());
        var ioc1 = iocContainer.Resolve<ClassicIOCContainer>();
        var ioc2 = iocContainer.Resolve<ClassicIOCContainer>();
        Assert.AreEqual(ioc1, ioc2);
    }


    [Test]
    public void IOC_RegisterDependency()
    {
        var iocContainer = new ClassicIOCContainer();
        iocContainer.Register<IClassicIOC,ClassicIOCContainer>();
        var ioc = iocContainer.Resolve<IClassicIOC>();
        Assert.AreEqual(ioc.GetType(), typeof(ClassicIOCContainer));
    }

    [Test]
    public void IOC_RegisterInstanceTwice()
    {
        var iocContainer = new ClassicIOCContainer();
        iocContainer.RegisterInstance<ClassicIOCContainer>(iocContainer);
        var ioc1 = iocContainer.Resolve<ClassicIOCContainer>();
        var ioc2 = iocContainer.Resolve<ClassicIOCContainer>();
        Assert.AreEqual(ioc1, iocContainer);
        Assert.AreEqual(ioc2, iocContainer);
    }

    public class TemplateA
    {

    }
    public class TemplateB { }
    public class GameCtrl
    {
        [Inject]
        public TemplateA TemplateA { get; set; }
        [Inject]
        public TemplateB TemplateB { get; set; }

    }
    [Test]
    public void IOC_Inject()
    {
        var iocContainer = new ClassicIOCContainer();
        iocContainer.RegisterInstance(new TemplateA());
        iocContainer.Register<TemplateB>();
        GameCtrl gameCtrl = new GameCtrl();
        iocContainer.Inject(gameCtrl);

        Assert.IsNotNull(gameCtrl.TemplateA);
        Assert.IsNotNull(gameCtrl.TemplateB);
        Assert.AreEqual(gameCtrl.TemplateA.GetType(),typeof(TemplateA));
        Assert.AreEqual(gameCtrl.TemplateB.GetType(), typeof(TemplateB));

    }

    [Test]
    public void IOC_Clear()
    {
        var iocContainer = new ClassicIOCContainer();
        iocContainer.RegisterInstance(new TemplateA());
        iocContainer.RegisterInstance(iocContainer);
        iocContainer.Register<TemplateB>();
        iocContainer.Clear();
        var a = iocContainer.Resolve<TemplateA>();
        var b = iocContainer.Resolve<TemplateB>();
        var c = iocContainer.Resolve<ClassicIOCContainer>();
        Assert.IsNull(a);
        Assert.IsNull(b);
        Assert.IsNull(c);
    }
}
