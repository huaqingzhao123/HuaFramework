using UnityEngine;
using System.Collections.Generic;

namespace Nireus
{

    public class SceneManager : Singleton<SceneManager>
    {
        private UISceneContainer defultContainer = new UISceneContainer();

        private UISceneContainer _owner;
        public UISceneContainer onwerScene {
            get { if (_owner == null) return defultContainer;return _owner; }
        }

        public void SetOwner(UISceneContainer owner)
        {
            if (_owner != null && _owner != defultContainer)
            {
                //this._owner.SetVisible(false);
            }
            {
                this._owner = owner;
                //this._owner.SetVisible(true);
            }
        }

        public void pushScene(UIScene scene, System.Object param = null, bool isRoot = true)
        {
            onwerScene.pushScene(scene, param, isRoot);
        }

        public void popScene()
        {
            onwerScene.popScene();
        }

        //public void pop()
        //{
        //    onwerScene.pop();
        //}

        public void popAllScene()
        {
            onwerScene.popAllScene();
        }
        public void popToRootScene()
        {
            onwerScene.popRootScene();
        }
        public void popToScene(UIScene scene)
        {
            onwerScene.popToScene(scene);
        }

        // 替换栈顶场景;
        public void replaceScene(UIScene scene, object param = null,bool isRoot = true)
        {
            onwerScene.replaceScene(scene, param, isRoot);
        }

        public void cleanScene()
        {
            onwerScene.cleanScene();
        }

        public UIScene PeekScene()
        {
            return onwerScene.PeekScene();
        }

        public void SetCameraEnabled(bool enabled)
        {
            var cam = LayerManager.Instance.getLayerCamera(LayerType.Scene);
            if (cam != null)
            {
                cam.enabled = enabled;
            }
        }
    }
    public class UISceneContainer
    {
        public System.Action OnUISceneChanged;
        public System.Action<UIScene> OnUIScenePushed;
        public System.Action OnUIScenePopped;
        struct Data
        {
            public long id;
            public UIScene scene;
            public object param;
            public bool isRoot;
            public Data(long id, UIScene scene, object param, bool isRoot)
            {
                this.id = id;
                this.scene = scene;
                this.param = param;
                this.isRoot = isRoot;
            }
        }
        private Stack<Data> _scene_stack = new Stack<Data>();
        private long _auto_data_id;
        private long _GetDataId()
        {
            if (_auto_data_id >= long.MaxValue) _auto_data_id = 0;
            return ++_auto_data_id;
        }

        public void pushScene(UIScene scene, System.Object param = null, bool isPopRoot = true)
        {
            if (scene == null)
            {
                GameDebug.LogError("please load scene");
            }
            //LoadingScene.Instance.Hide(true);
            if (_scene_stack.Count > 0)
            {
                Data prevData = _scene_stack.Peek();
                // 栈顶元素就是它，不进行压栈;
                if (prevData.scene == scene)
                {
                    //if (param != null)
                    //{
                    //    scene.OnShowScene(param);
                    //}
                    return;
                }
                else
                {
                    prevData.scene.OnLeaveTop();
                    if (_scene_stack.Count > 0)
                    {
                        prevData.scene.OnHideScene();
                    }
                }
            }

            Data data = new Data(_GetDataId(), scene, param, isPopRoot);
            _scene_stack.Push(data);
            scene.OnPush();
            scene.OnShowScene(param);
            scene.OnTop();
            scene.OnPushEnd(param);
            OnUISceneChanged?.Invoke();
            OnUIScenePushed?.Invoke(scene);
        }

        public void popScene()
        {
            // 只剩最后一个场景，不能退出;
            if (_scene_stack.Count <= 1) return;

            Data data = _scene_stack.Pop();
            data.scene.OnHideScene();
            data.scene.OnPop();
            data.scene.OnLeaveTop();
            if (_scene_stack.Count <= 0) return;
            Data data2 = _scene_stack.Peek();
            var scene = data2.scene;
            scene.OnShowScene(data2.param);
            scene.OnTop();
            if (OnUISceneChanged != null)
                OnUISceneChanged();
            if (OnUIScenePopped != null)
            {
                OnUIScenePopped();
            }
        }

        public void pop()
        {
            // 只剩最后一个场景，不能退出;
            if (_scene_stack.Count <= 1) return;

            Data data = _scene_stack.Pop();
            data.scene.OnHideScene();
            data.scene.OnPop();
            data.scene.OnLeaveTop();
        }

        public void popAllScene()
        {
            Data data;
            while (_scene_stack.Count > 0)
            {
                data = _scene_stack.Pop();
                data.scene.OnHideScene();
                data.scene.OnPop();
                data.scene.OnLeaveTop();
            }
            if (OnUISceneChanged != null)
                OnUISceneChanged();
        }

        public void popRootScene()
        {
            while (_scene_stack.Count > 0)
            {
                Data data = _scene_stack.Peek();
                if (data.isRoot == false)
                {
                    _scene_stack.Pop();
                    data.scene.OnHideScene();
                    data.scene.OnPop();
                    data.scene.OnLeaveTop();
                }
                else
                {
                    popScene();//this is root scene
                    break;
                }
            }
        }

        public void popToScene(UIScene scene)
        {
            bool had = Find(scene) != -1;
            if (had)
            {
                while (_scene_stack.Count > 0)
                {
                    Data data = _scene_stack.Peek();
                    if (data.scene != scene)
                    {
                        popScene();
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        // 替换栈顶场景;
        public void replaceScene(UIScene scene, object param = null,bool isRoot = false)
        {
            if (_scene_stack.Count != 0)
            {
                Data prevData = _scene_stack.Peek();
                // 栈顶元素就是它
                if (prevData.scene == scene)
                {
                    return;
                }
                Data cur_data = _scene_stack.Pop();
                cur_data.scene.OnHideScene();
                cur_data.scene.OnPop();
                cur_data.scene.OnLeaveTop();
                // if (!(cur_data.scene is MainUIScene))
                // {
                //     GameObject.Destroy(cur_data.scene.gameObject);    
                // }
                if (OnUIScenePopped != null)
                {
                    OnUIScenePopped();
                }
            }
            Data data = new Data(_GetDataId(), scene, param, isRoot);
            _scene_stack.Push(data);
            scene.OnPush();
            scene.OnShowScene(param);
            scene.OnTop();
            if (OnUISceneChanged != null)
                OnUISceneChanged();
        }

        public void cleanScene()
        {
            _scene_stack.Clear();
        }

        public UIScene PeekScene()
        {
            if (_scene_stack.Count > 0)
                return _scene_stack.Peek().scene;
            return null;
        }

        int Find(UIScene scene)
        {
            int i = 0;
            foreach (var item in _scene_stack)
            {
                if (item.scene == scene)
                    return i;
                i++;
            }
            return i;
        }
    }


}
