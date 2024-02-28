using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nireus
{
    public class GameObjectPool<T> where T : Component
    {
        private Transform root;
        private T template;
        private LinkedList<T> activedList = new LinkedList<T>();
        private LinkedList<T> deactiveList = new LinkedList<T>();
        private Action<T> OnInstance;

        private Boolean _if_template_set = false;

        public LinkedList<T> ActivedListCopy { get { return new LinkedList<T>(activedList); } }
        public int activeCount { get { return activedList.Count; } }

        public GameObjectPool(Action<T> OnInstance = null)
        {
            this.OnInstance = OnInstance;
        }

        public GameObjectPool(Transform root, GameObject template, int defaultAllocSize, Action<T> OnInstance = null)
        {
            var t = _SetTemplateObject(template);
            this.OnInstance = OnInstance;
            this.root = root;

            for (int i = 0; i < defaultAllocSize; i++)
            {
                T component = Instance(false);
                deactiveList.AddLast(component);
            }
        }

        protected virtual AssetLoaderManagerBase GetAssetLoader()
        {
            // implement in subclass
            return null;
        }

        protected virtual void LoadTemplate()
        {
            var loader = GetAssetLoader();
            if (loader == null) return;
            
            var ui = loader.LoadUI(typeof(T));
            SetTemplatePrefab(ui.gameObject);
        }

        public T GetTemplate()
        {
            if (template == null)
            {
                LoadTemplate();
            }

            return template;
        }

        public void SetTemplatePrefab(GameObject template)
        {
            if (_if_template_set)
            {
                return;
            }
            _if_template_set = true;

            var t = template.GetComponent<T>();

            if (t == null)
            {
                GameDebug.LogError("Prefab doesn't have the target component");
            }

            this.template = t;
        }

        private T _SetTemplateObject(GameObject template)
        {
            var t = template.GetComponent<T>();
            template.name = template.name + "(template)";
            if (t == null)
            {
                GameDebug.LogError("Need AddComponent " + typeof(T).ToString() + " To GameObjectPool template src");
            }
            template.SetActive(false);
            template.transform.SetParent(template.transform, false);
            this.template = t;
            return t;
        }

        public void DestroyPool()
        {
            DespawnAll(true);
            
            if (template != null)
                GameObject.Destroy(template.gameObject);
            template = null;
            _if_template_set = false;

        }

        public void DespawnAll(bool destroy)
        {
            if (destroy)
            {
                foreach (T c in deactiveList)
                {
                    if (c && c.gameObject)
                    {
                        GameObject.Destroy(c.gameObject);
                    }
                }

                foreach (T c in activedList)
                {
                    if (c && c.gameObject)
                    {
                        GameObject.Destroy(c.gameObject);
                    }
                }
                deactiveList.Clear();
                activedList.Clear();
            }
            else
            {
                foreach (T c in activedList)
                {
                    if(c == null) continue;
                    c.gameObject.SetActive(false);
                    deactiveList.AddLast(c);
                }
                activedList.Clear();
            }
        }

        private T Instance(bool active)
        {
            var tpl = GetTemplate();
            if (tpl == null) return null;

            var go = GameObject.Instantiate(tpl.gameObject);
            go.SetActive(active);
            var component = go.GetComponent<T>();
            if (root != null)
            {
                go.transform.SetParent(root);
            }

            return component;
        }

        public T Spawn(Transform spawn_to_this_parent = null,bool need_visible = true)
        {
            T component = null;
            if (deactiveList.Count == 0)
            {
                
                component = Instance(true);
                activedList.AddLast(component);
                OnInstance?.Invoke(component);
            }
            else
            {
                //TODO 先去除空  未修改完成  只出现了一个
                component = deactiveList.First.Value;
                // while (deactiveList!=null && component== null && deactiveList.Count!=0)
                // {
                //     deactiveList.RemoveFirst();
                //     if (deactiveList.Count == 0)
                //     {
                //         break;
                //     }
                //     component = deactiveList.First.Value;
                // }
                
                if (component == null)
                {
                    // component = Instance(true);
                    // activedList.AddLast(component);
                    // OnInstance?.Invoke(component);
                    deactiveList.RemoveFirst();
                    return component;
                }
                else
                {
                    deactiveList.RemoveFirst();
                    activedList.AddLast(component);
                    if(need_visible)
                    {
                        component.gameObject.SetActive(true);
                    }
                }
            }

            if (spawn_to_this_parent != null)
            {
                component?.transform.SetParent(spawn_to_this_parent, false);
            }

            return component;
        }

        public virtual void Despawn(T component, Transform pool_parent = null,bool need_visible = true)
        {
            if (!component) return;
            if(need_visible)
            {
                component.gameObject.SetActive(false);
            }
            activedList.Remove(component);
            deactiveList.AddLast(component);

            if (pool_parent != null)
            {
                component.transform.SetParent(pool_parent, false);
            }
        }

        public void Despawn(GameObject go)
        {
            T component = go.GetComponent<T>();

            Despawn(component);
        }

        public void Despawn(ICollection<T> lst_component, Transform pool_parent = null)
        {
            foreach (var component in lst_component)
            {
                Despawn(component, pool_parent);
            }

            lst_component.Clear();
        }

        public void DespawnToHideLayer(T component)
        {
            Despawn(component, LayerManager.getInstance().getLayer(LayerType.HIDE));
        }
        public void DespawnToHideLayerWithoutUnActive(T component)
        {
            Despawn(component, LayerManager.getInstance().getLayer(LayerType.HIDE), false);
        }
        public void DespawnToHideLayer(ICollection<T> lst_component)
        {
            foreach (var component in lst_component)
            {
                Despawn(component, LayerManager.getInstance().getLayer(LayerType.HIDE),false);
            }
            lst_component.Clear();
        }
    }
}