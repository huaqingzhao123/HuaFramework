using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Nireus
{
    public class AnimLayoutGroupBase : UITemplate
    {
        public int play_count_once = 1;
        public float anim_next_second = 0;

        private int _index;
        private List<UnityEngine.Animation> _lst_anims = new List<UnityEngine.Animation>();

        private int _update_frame = 0;
        private int _frame_now = 0;

        public event UnityAction completeAction;

        protected override void OnEnable()
        {
            base.OnEnable();
#if UI_EDIT
            play();
#endif
        }

        protected virtual void _clearData()
        {
            _index = 0;
            _lst_anims.Clear();
        }

        public void Play()
        {
            _clearData();

            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (!child.gameObject.activeSelf) continue;

                _addAnimationChild(child);
            }

            _update_frame = (int)(anim_next_second * 60);
        }

        private void Update()
        {
            _frame_now++;

            if (_frame_now < _update_frame) return;

            _frame_now = 0;

            if (_index >= _lst_anims.Count || !gameObject.activeSelf)
            {
                _complete();
                return;
            }

            var next = _index + play_count_once;

            //若延时为0，则一起播放
            if (_update_frame == 0) next = _lst_anims.Count;

            for (; _index < next; _index++)
            {
                if (_index >= _lst_anims.Count) break;

                var animation = _lst_anims[_index];
                animation.Play();
            }
        }

        protected virtual void _addAnimationChild(Transform child)
        {
            var animation = child.GetComponent<UnityEngine.Animation>();
            if (animation == null) return;

            animation.Stop();
            _lst_anims.Add(animation);
        }
        
        protected virtual void _complete()
        {
            completeAction?.Invoke();
        }
    }
}
