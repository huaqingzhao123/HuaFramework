using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Nireus
{
    public static class GameObjectEx
    {

        public static void SetActiveIfNeeded(this GameObject go, bool active)
        {
            if (go.activeSelf != active)
            {
                go.SetActive(active);
            }
        }

        public static void setLayer(this GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform t in go.transform)
            {
                setLayer(t.gameObject, layer);
            }
        }


        public static T AddComponentIfNeeded<T>(this GameObject go) where T : Component
        {
            T t = go.GetComponent<T>();
            if (t == null)
                return go.AddComponent<T>();
            return t;
        }

        public static Component AddComponentIfNeeded(this GameObject go, Type compType)
        {
            var comp = go.GetComponent(compType);
            if (comp == null)
            {
                return go.AddComponent(compType);
            }

            return comp;
        }

        public static void SetParentAndResetLocalTrans(this Transform child, Transform parent)
        {
            child.SetParent(parent);
            child.localPosition = Vector3.zero;
            child.localRotation = Quaternion.identity;
            child.localScale = Vector3.one;
        }


        public static void SetLayerRecursively(this GameObject go, int layer)
        {
            go.setLayer(layer);
            foreach (Component child in go.GetComponentInChildren<Transform>(true))
            {
                child.gameObject.setLayer(layer);
                SetLayerRecursively(child.gameObject, layer);
            }
        }

        public static void DestroyIfNeed(this GameObject go)
        {
            if (go != null)
            {
                GameObject.Destroy(go);
                go = null;
            }
        }

        public static void DestroyImmediateIfNeed(this GameObject go)
        {
            if (go != null)
            {
                GameObject.DestroyImmediate(go);
                go = null;
            }
        }

        public static Sprite CreateSprite(Texture2D texture, SpriteMeshType meshType = SpriteMeshType.FullRect)
        {
            if (texture != null)
            {
                Sprite spt = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f, 0, meshType);
                return spt;
            }
            return null;
        }

        public static void AddChild(this Transform transform, Transform child, bool worldPositionStays)
        {
            if (child.parent != transform)
                child.SetParent(transform, worldPositionStays);
        }
        public static void AddChildAndResetLocal(this Transform transform, Transform child)
        {
            if (child.parent != transform)
            {
                child.SetParent(transform);
                child.localPosition = Vector3.zero;
                child.localRotation = Quaternion.identity;
                child.localScale = Vector3.one;
            }
        }

        public static void SetRenderEnable(this GameObject go, bool enable, bool recursively = true)
        {
            Renderer render = go.GetComponent<Renderer>();
            if (render != null)
            {
                render.enabled = enable;
            }
            if (recursively)
            {
                foreach (Transform t in go.transform)
                {
                    SetRenderEnable(t.gameObject, enable, true);
                }
            }
        }

        public static void SetRenderEnable(this Renderer render, bool enable, bool recursively = true)
        {
            render.enabled = enable;
            if (recursively)
            {
                foreach (Transform t in render.transform)
                {
                    SetRenderEnable(render, enable, true);
                }
            }
        }

        public static Transform FindRecursively(this Transform t, string name)
        {
            Transform find = t.Find(name);
            if (find != null)
            {
                return find;
            }           
            
            foreach (Transform t2 in t)
            {
                var t3 = FindRecursively(t2,name);
                if(t3 != null)return t3;
            }
            
            return null;
        }

        public static string GetFullname(this GameObject obj)
        {
            StringBuilder fn = new StringBuilder();

            if (obj == null)
            {
                GameDebug.Log("[GameObject.GetFullname] target gameObject is null.");
                return fn.ToString();
            }

            fn.Insert(0, obj.name);

            var parent = obj.transform.parent;
            while (parent)
            {
                fn.Insert(0, "/");
                fn.Insert(0, parent.name);
                parent = parent.parent;
            }

            return fn.ToString();
        }
    }
}
