using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

namespace Nireus
{
    public class HUDTextItem
    {
        public HUDTextItem(String content, Color color, Action call_back)
        {
            this.content = content;
            this.color = color;
            this.call_back = call_back;
        }


        public String content { get; private set; }
        public Color color { get; private set; }
        public Action call_back{ get; private set; }
}

    //[ExecuteInEditMode]
    public class HUDText : MonoBehaviour
    {
        public Text tpl;//字体样式模板

        //【调试字体效果时使用（代码一般不用！】：
        public bool try_once = false;//测试用，在unity编辑器中，每次按一下这个，会弹出一个字体，用来调试效果
        public Color color = Color.red;//测试用，代码里一般会在本类append函数中给字体颜色赋值

        //【关于字体登场动画】
        public float startAnimDuringTime = 0.2f;//字体出现阶段的动画持续时间（在阶段，指定持续时间内，字体从scale 0 变为 scale 1)
        public float stayDuringTime = 0.5f;//字体保持阶段的持续时间（在该阶段，字体会保持不动一段时间)


        //【关于字体消失阶段】：
        public bool useEndFadeAnim = true;//字消失时，是否会有fade效果（淡出）
        public float endAnimMoveDistance = 5.0f;//字消失时，是否向上移动，移动多少距离
        public float endAnimDuringTime = 0.2f;//字体消失阶段的持续时间

        //【关于同时显示多个字体时的行为】：
        private float _cur_overlap_idx = 0;
        public int overlapIndexResetValue = 10; //字体位置数量（假如该值为5，场上有6个字体同时显示，那么1-5个字体的位置是 i*overlap_spacing, 第六个字体的位置和第一个字体一样，因为字体位置数量只有5个）
        public float overlapPositionSpacing = 30.0f; //如果有多个字体同时出现，2个字体之间的相对位置（位置间隔）
        public float showFontTimeSpacing = 0.1f;//2个字不能在同一瞬间出现，而是指定了一个字和字之间出现的时间间隔，比如同时调用了2次append，第一个字体在0s出现，第二个字体在指定时间后才出现
    

        private GameObjectPool<Text> _pool = null;
        private Queue<HUDTextItem> _queue = new Queue<HUDTextItem>();
        private void Awake()
        {
            _pool = new GameObjectPool<Text>((Text text) =>
            {
                text.transform.SetParent(this.transform, false);
            });
            _pool.SetTemplatePrefab(tpl.gameObject);
            tpl.gameObject.SetActive(false);

            try_once = false;
        }

      
        public void append(string text, Color color, Action finish_call_back = null)
        {
            if (gameObject.activeInHierarchy == false)
            {
                finish_call_back?.Invoke();
                return;
            }
            _queue.Enqueue(new HUDTextItem(text, color, finish_call_back));

            float spacing = 0;


            var now = Time.timeSinceLevelLoad;
            spacing = now - _last_handle_time;
           
       

            if (spacing > showFontTimeSpacing)
            {
                _last_handle_time = now;
                _handle();
            }
            else
            {
                _last_handle_time = now + showFontTimeSpacing - spacing;
                Scheduler.Instance.CreateScheduler("HUDText_AppendText", _handle, showFontTimeSpacing - spacing, 1);
            }
        }

        private float _last_handle_time = 0;
        private void _handle()
        {
            if(_queue.Count == 0)
            {
                return;
            }
            var item = _queue.Dequeue();
            var position = _cur_overlap_idx * overlapPositionSpacing;
            bool need_reset_overlap_idx_when_font_disappear = (_cur_overlap_idx == 0);
            _cur_overlap_idx++;

            var txt = _pool.Spawn();
            txt.text = item.content;
            txt.color = item.color;
            txt.transform.SetAsLastSibling();
            var rect_transform = txt.GetComponent<RectTransform>();
            rect_transform.localPosition = new Vector3(0, position, 0);

            Action action = () =>
            {
                if (useEndFadeAnim)
                {
                    txt.DOFade(0.0f, endAnimDuringTime);
                }
                rect_transform.DOLocalMoveY(endAnimMoveDistance + position, endAnimDuringTime).OnComplete(() =>
                {
                    _pool.Despawn(txt);
                    if(need_reset_overlap_idx_when_font_disappear)
                    {
                        _cur_overlap_idx = 0;
                    }
                    item.call_back?.Invoke();
                });
            };

            txt.transform.localScale = new Vector3();
            txt.transform.DOScale(new Vector3(1, 1, 1), startAnimDuringTime).SetEase(Ease.OutBack).OnComplete(() =>
            {
                Scheduler.Instance.CreateScheduler("HUDText_AppendTextComplete", action, stayDuringTime, 1);
            });
        }

#if UNITY_EDITOR
        private System.Random r = new System.Random(DateTime.Now.Millisecond);
        private void FixedUpdate()
        {
            if(try_once)
            {
                append("-" + r.Next(10000,20000).ToString(), color);
                try_once = false;
            }
        }
#endif
    }
}
