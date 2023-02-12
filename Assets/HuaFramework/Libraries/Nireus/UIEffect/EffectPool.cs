using UnityEngine;
using System.Collections.Generic;
namespace Nireus
{
    public class EffectPool
    {
        private Transform root;
        private EffectController template;
        private List<EffectController> despawnFXs = new List<EffectController>();
        private bool released = false;

        public EffectPool(Transform root, GameObject template)
        {
            var fx = template.GetComponent<EffectController>();
            if (fx == null)
            {
                fx = template.AddComponent<EffectController>();
                fx.loop = true;
                fx.autoDestroy = true;
            }

            template.SetActive(false);

            //template.transform.SetParentAndResetLocalTrans(root);
            template.transform.SetParent(root,true);
            this.root = root;
            this.template = fx;
            this.released = false;
        }

        public void Release()
        {
            FreePooledFx();

            if (template != null)
                Object.Destroy(template.gameObject);
            template = null;

            released = true;
        }

        public EffectController Spawn(bool ignoreTimeScale = false,float beginTime = 0f,float endTime = 10f)
        {
            if (released)
                return null;

            if (template == null)
                return null;

            EffectController eff_obj = null;
            if (despawnFXs.Count == 0 || despawnFXs[despawnFXs.Count - 1].GetNextPoolTime() > Time.time)
            {
                var go = GameObject.Instantiate(template.gameObject) as GameObject;

                go.SetActive(true);
                
                eff_obj = go.GetComponent<EffectController>();
                eff_obj.ignoreTimeScale = ignoreTimeScale;
                // Add a root GO as default
                eff_obj.CreateRoot();
                //Object.DontDestroyOnLoad(fx.Root);
                eff_obj.FXPool = this;
            }
            else
            {
                eff_obj = despawnFXs[despawnFXs.Count - 1];
                despawnFXs.RemoveAt(despawnFXs.Count - 1);
                //eff_obj.Root.SetParentAndResetLocalTrans(null);
                eff_obj.Root.gameObject.SetActive(true);
            }
            eff_obj.Init();
            eff_obj.PlayFX(beginTime, endTime);
            return eff_obj;
        }

        public void OnDestroy(EffectController fx)
        {
            fx.ClearFinishCallback();
            Debug.Assert(fx.Root.parent != root);
            despawnFXs.Remove(fx);
        }

        public void Despawn(EffectController fx)
        {
            if (released)
            {
                Object.Destroy(fx.Root.gameObject);
            }
            else
            {

                // Reset data
                fx.ClearFinishCallback();
                fx.SetFreeToLastPoolTime(Time.time);

                Debug.Assert(fx.Root.parent != root);

                // Return to pool
            //    fx.Root.SetParentAndResetLocalTrans(root);
                fx.Root.gameObject.SetActive(false);
                despawnFXs.Insert(0, fx);
            }
        }

        public void FreePooledFx()
        {
            foreach (var fx in despawnFXs)
                Object.Destroy(fx.Root.gameObject);

            despawnFXs.Clear();

        }
    }
}