using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Nireus
{
    public class FlowManager : MonoBehaviour
    {
        private static FlowManager _instance;
        public static FlowManager Instance => _instance;

        void Awake()
        {
            _instance = this;
        }

        List<UITemplate> _flow_tpls = new List<UITemplate>();
        
        public void AddFlow(UITemplate tpl,bool is_Root_move = false)
        {
            if (_flow_tpls.Contains(tpl))
            {
                GameDebug.LogError("can not repeat tpl");
                return;
            }
            else if (_flow_tpls.Count>0)
            {
                foreach (var item in _flow_tpls)
                {
                    item.gameObject.SetActive(false);
                }
            }
            LayerManager.getInstance().addToLayer(tpl.transform, LayerType.TIPS);
            tpl.transform.localScale = Vector3.one;
            if (tpl.IsShow() == false)
            {
                tpl.SetVisible(true);

            }

            var target_transform = tpl.transform;
            
            if (is_Root_move)
            {
                target_transform = tpl.transform.Find("Root");
                target_transform.localPosition = Vector3.zero;
            }
            Sequence seq = DOTween.Sequence();
            seq.Append(target_transform.DOLocalMove(target_transform.localPosition - new Vector3(0, 200f, 0), 1f).SetEase(Ease.OutCubic));
            seq.AppendInterval(0.5f);
            seq.OnComplete(() => { RemoveFlow(tpl, false); });
            _flow_tpls.Add(tpl);
        }
        public void RemoveFlow(UITemplate tpl, bool if_destroy = true)
        {
            int index = _flow_tpls.IndexOf(tpl);
            if (index == -1) return;
            LayerManager.getInstance().addToLayer(tpl, LayerType.HIDE);
            tpl.SetVisible(false);
            _flow_tpls.RemoveAt(index);
            if (if_destroy) Destroy(tpl.gameObject);
        }

    }

}

