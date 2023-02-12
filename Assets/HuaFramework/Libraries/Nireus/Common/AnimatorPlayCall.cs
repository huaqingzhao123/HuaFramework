using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
 
namespace Nireus
{ 
    /// <summary>
    /// Animator 播放动画回调相关
    /// </summary>
    public class AnimatorPlayCall : MonoBehaviour
    {
        List<AnimatorPlayCallParam> Pools = new List<AnimatorPlayCallParam>();
        List<AnimatorPlayCallParam> Pools_ft = new List<AnimatorPlayCallParam>();
        List<AnimatorPlayCallParam> Pools_remove_list = new List<AnimatorPlayCallParam>();
 
        static AnimatorPlayCall instance;
        public static AnimatorPlayCall I
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameObject("AnimatorPlayCall").AddComponent<AnimatorPlayCall>();
                    GameObject.DontDestroyOnLoad(instance);
                }
                return instance;
            }
        }
 
        /// <summary>
        /// 检测Animator是否播放完毕
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public void OnIsPlayOver(Animator animator,bool isCurrent, string name,System.Action action,int scene_index)
        {
            if (action == null)
                return;
            var new_callback= new AnimatorPlayCallParam(animator, name, isCurrent, action,scene_index);
            
            Pools_ft.Add(new_callback);
        }
        
        /// <summary>
        /// 检测Animator是否播放完毕
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="endTime"></param>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public void OnIsPlayOver(Animator animator,float endTime,bool isCurrent, string name,System.Action action,int scene_index)
        {
            if (action == null)
                return;
            
            var new_callback= new AnimatorPlayCallParam(animator,endTime, name, isCurrent, action,scene_index);
            
            Pools_ft.Add(new_callback);
        }

        public void RemoveAllCallBack()
        {
            Pools.Clear();
        }
        
        public void RemoveCallBackByAnimationNamePublic(Animator animator, string animationName,int scene_index)
        {
            var new_callback= new AnimatorPlayCallParam(animator,0, animationName, true,null,0);
            Pools_remove_list.Add(new_callback);
        }
        
        private void RemoveCallBackByAnimationName(Animator animator, string animationName,int scene_index)
        {
            var remove_list = new List<AnimatorPlayCallParam>();
            foreach (var callBack in Pools)
            {
                if (callBack.Animator == animator && callBack.AnimationName == animationName
                && callBack.Scene_Index == scene_index)
                {
                    remove_list.Add(callBack);
                }
            }

            foreach (var remove_item in remove_list)
            {
                Pools.Remove(remove_item);
            }
        }

        private void PoolRemove(AnimatorPlayCallParam callParam)
        {
            foreach (var callBack in Pools)
            {
                if (callBack.Equals(callParam))
                {
                    Pools.Remove(callBack);
                    return;
                }
            }
        }

        public void RemoveSceneAllCallBack(int scene_index)
        {
            foreach (var callBack in Pools)
            {
                if (callBack.Scene_Index == scene_index)
                {
                    Pools_remove_list.Add(callBack);
                }
            }
        }
 
        void Update()
        {
            if (Pools.Count > 0)
            {
                var remove_list = new List<AnimatorPlayCallParam>();
                for (int i = 0; i < Pools.Count; i++)
                {
                    if (Pools[i].Check())
                    {
                        remove_list.Add(Pools[i]);
                    }
                }

                foreach (var remove_item in remove_list)
                {
                    Pools.Remove(remove_item);
                }
            }
            
            if (Pools_remove_list.Count > 0)
            {
                foreach (var sg in Pools_remove_list)
                {
                    RemoveCallBackByAnimationName(sg.Animator,sg.AnimationName,sg.Scene_Index);
                }
                Pools_remove_list.Clear();
            }
            
            if (Pools_ft.Count > 0)
            {
                foreach (var sg in Pools_ft)
                {
                    RemoveCallBackByAnimationName(sg.Animator,sg.AnimationName,sg.Scene_Index);
                }
                Pools.AddRange(Pools_ft);
                Pools_ft.Clear();
            }
        }

        public bool IsInRemoveList(Animator animator, string animationName)
        {
            foreach (var sg in Pools_remove_list)
            {
                if (sg.Animator == animator && sg.AnimationName == animationName)
                {
                    return true;
                }
            }
            
            return false;
        }
 
        /// <summary>
        /// 播放回调参数
        /// </summary>
        class AnimatorPlayCallParam:IEquatable<AnimatorPlayCallParam>
        {
            public AnimatorPlayCallParam(Animator animator, string name, bool isCurrent, System.Action call,int scene_index)
            {
                Layer = 0;
                Action = call;
                Animator = animator;
                IsCurrent = isCurrent;
                AnimationName = name;
                EndTime = 1;
                Scene_Index = scene_index;
            }
 
            public AnimatorPlayCallParam(Animator animator, bool isCurrent,int layer, System.Action call,int scene_index)
            {
                Layer = layer;
                Action = call;
                Animator = animator;
                IsCurrent = isCurrent;
                EndTime = 1;
                Scene_Index = scene_index;
            }
            
            public AnimatorPlayCallParam(Animator animator, float endTime, string name, bool isCurrent, System.Action call,int scene_index)
            {
                Layer = 0;
                Action = call;
                Animator = animator;
                IsCurrent = isCurrent;
                AnimationName = name;
                EndTime = endTime;
                Scene_Index = scene_index;
            }
 
            /// <summary>
            /// 层
            /// </summary>
            public int Layer { private set; get; }
 
            /// <summary>
            /// 回调行为
            /// </summary>
            public System.Action Action { private set; get; }
 
            /// <summary>
            /// Animator 动画
            /// </summary>
            public Animator Animator { private set; get; }
 
            /// <summary>
            /// 是否当前动画
            /// </summary>
            public bool IsCurrent { private set; get; }
            
            /// <summary>
            /// 是否当前动画名字
            /// </summary>
            public string AnimationName { private set; get; }
            
            /// <summary>
            /// 是否当前动画播放结束点 0-1
            /// </summary>
            public float EndTime { private set; get; }
            
            public int Scene_Index{ private set; get; }

            private AnimatorStateInfo CurInfo
            {
                get
                {
                    return Animator.GetCurrentAnimatorStateInfo(Layer);
                }
            }
            
            /// <summary>
            /// 每帧检测动画状态
            /// </summary>
            /// <returns></returns>
            public bool Check()
            {
                if ( Animator == null|| Animator.isActiveAndEnabled == false)
                {
                    return true;
                }
                if (Action == null)
                    return true;
                
                if ((CurInfo.normalizedTime > EndTime) && (CurInfo.IsName(AnimationName)))//normalizedTime：0-1在播放、0开始、1结束 MyPlay为状态机动画的名字
                {
                    //完成后的逻辑
                    if (Action != null && AnimatorPlayCall.I.IsInRemoveList(Animator,AnimationName)==false)
                    {
                        //Animator.speed = 0;
                        Action.Invoke();
                        return true;
                    }
                }  
                
                return false;
            }
            
            public bool Equals(AnimatorPlayCallParam y)
            {
                if (Animator.Equals(y.Animator)&&Action.Equals(y.Action)&&AnimationName.Equals(y.AnimationName))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        
        class AnimatorPlayCallParamComparer : IEqualityComparer<AnimatorPlayCallParam>  
        {  
            public static AnimatorPlayCallParamComparer Default = new AnimatorPlayCallParamComparer();  
            #region IEqualityComparer<PrefabInfoSgAssetEditor> 成员  
            public bool Equals(AnimatorPlayCallParam x, AnimatorPlayCallParam y)  
            {  
                return x.Animator.Equals(y.Animator)&&x.Action.Equals(y.Action)&&x.AnimationName.Equals(y.AnimationName);  
            }  
            public int GetHashCode(AnimatorPlayCallParam obj)  
            {  
                return obj.GetHashCode();  
            }  
            #endregion  
        }  
    }

    /// <summary>
    /// Animator扩展
    /// </summary>
    public static class AnimatorExtension
    {
        public static void SetTrigger(this Animator animator, string name, System.Action action,int scene_index)
        {
            if (!animator)
            {
                return;
            }
            animator.SetTrigger(name);
            AnimatorPlayCall.I.OnIsPlayOver(animator,false, name, action,scene_index);
        }
 
        public static void SetBool(this Animator animator, string name, bool value, System.Action action,int scene_index)
        {
            if (!animator)
            {
                return;
            }
            animator.SetBool(name, value);
            AnimatorPlayCall.I.OnIsPlayOver(animator, false,name, action,scene_index);
        }
 
        public static void Play(this Animator animator, string name, System.Action action,int scene_index)
        {
            if (!animator)
            {
                return;
            }

            animator.speed = 1;
            animator.Play(name);
            AnimatorPlayCall.I.OnIsPlayOver(animator,true,name, action,scene_index);
        }
        
        public static void Play(this Animator animator, string name,float start_time,int scene_index)
        {
            if (!animator)
            {
                return;
            }

            animator.speed = 1;
            animator.Play(name,0,start_time);
            AnimatorPlayCall.I.OnIsPlayOver(animator,true,name, null,scene_index);
        }
 
        public static void PlayAgain(this Animator animator,string name, System.Action action, float speed ,int scene_index)
        {
            if (!animator)
            {
                return;
            }
            animator.speed = speed;
            animator.Play(name,0,0f);
            AnimatorPlayCall.I.OnIsPlayOver(animator,true,name, action,scene_index);
        }
        
        public static void PlayInterval(this Animator animator,string name,float startTime,float endTime, System.Action action,int scene_index)
        {
            if (!animator)
            {
                return;
            }
            animator.speed = 1;
            animator.Play(name,0,startTime);
            AnimatorPlayCall.I.OnIsPlayOver(animator,endTime,true,name, action,scene_index);
        } 
        public static void OnOver(this Animator animater,bool isCurrent,string name, System.Action action,int scene_index)
        {
            if (!animater)
            {
                return;
            }
            AnimatorPlayCall.I.OnIsPlayOver(animater, isCurrent,name, action,scene_index);
        }
        
        public static void RemoveCallBackByAnimationName(this Animator animater,string name,int scene_index)
        {
            if (!animater)
            {
                return;
            }
            AnimatorPlayCall.I.RemoveCallBackByAnimationNamePublic(animater,name,scene_index);
        }
    }
 
}