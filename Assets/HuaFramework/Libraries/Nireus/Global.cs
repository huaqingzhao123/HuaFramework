using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

using System;

namespace Nireus
{
	public class Global : SingletonBehaviour<Global>
    {
        private static object lockObject;

		private List<Nireus.Delegate.CallFuncVoid> _update_callback_list = new List<Nireus.Delegate.CallFuncVoid>();
        private List<Nireus.Delegate.CallFuncVoid> _late_update_callback_list = new List<Delegate.CallFuncVoid>();

        private Rect _camera_rect;
		public Rect getCameraRect() { return _camera_rect; }

        public override void Initialize()
        {
            base.Initialize();
        }

        void Awake()
		{
			//UINetWating.getInstance().Hide();
		}

		void Start()
		{
			//calueCameraRect();
			set3DCameraRect();
			//测试代码
			/*
			GameObject new_path = new GameObject("new camera path");
			CameraPathBezierAnimator animator = new_path.AddComponent<CameraPathBezierAnimator>();
			CameraPathBezier bezier = new_path.AddComponent<CameraPathBezier>();
			animator.animationTarget = GameObject.Find("3D Camera").transform;
			new_path.transform.parent = GameObject.Find("3D Normal").transform;
			new_path.transform.localPosition = new Vector3(0f, 0f, 0f);
			bezier.AddNewPoint();
			bezier.AddNewPoint();
			bezier.AddNewPoint();
			animator.mode = CameraPathBezierAnimator.modes.loop;
			animator.enabled = true;
			bezier.loop = true;
			 * */
		}

		void Update()
		{
            int len = _update_callback_list.Count;
            Delegate.CallFuncVoid action = null;
            for (int i = 0; i < len; i++)
            {
                action = _update_callback_list[i];
                if (action == null)
                {
                    if (i < (len - 1))
                    {
                        action = _update_callback_list[len - 1];
                        _update_callback_list.RemoveAt(len - 1);
                        len--;
                        _update_callback_list[i] = action;
                        if (action != null)
                        {
                            action.Invoke();
                        }
                    }
                }
                else
                {
                    action.Invoke();
                }
            }
        }

        void LateUpdate()
        {
            int len = _late_update_callback_list.Count;
            Delegate.CallFuncVoid action = null;
            for (int i = 0; i < len; i++)
            {
                action = _late_update_callback_list[i];
                if (action == null)
                {
                    if (i < (len - 1))
                    {
                        action = _late_update_callback_list[len - 1];
                        _late_update_callback_list.RemoveAt(len - 1);
                        len--;
                        _late_update_callback_list[i] = action;
                        if (action != null)
                        {
                            action.Invoke();
                        }
                    }
                }
                else
                {
                    action.Invoke();
                }
            }
        }

		public void startCoroutine(IEnumerator ie)
		{
			StartCoroutine(ie);
		}

		public void registerScheduleOnce(float time, Action schedule_callback)
		{
            if(time <= 0)
            {
                schedule_callback?.Invoke();
                return;
            }
            if(schedule_callback != null)
            {
			    StartCoroutine(timer(time, schedule_callback));
            }
		}
		public void nextFrame(Action action)
		{
			StartCoroutine(nextFrame1(action));
		}
		private IEnumerator nextFrame1(Action schedule_callback)
		{
			yield return null;
			schedule_callback?.Invoke();
		}

        public Coroutine registerScheduleOnce_canStop(float time, Action schedule_callback)
        {
            if (schedule_callback != null)
            {
                return StartCoroutine(timer(time, schedule_callback));
            }
            return null;
        }

        public void unRegisterScheduleOnce(Coroutine coroutine)
        {
            StopCoroutine(coroutine);
        }


        private IEnumerator timer(float time, Action schedule_callback)
		{
			yield return new WaitForSeconds(time);
			schedule_callback?.Invoke();
		}

		public void registerUpdate(Delegate.CallFuncVoid update_callback)
		{
            int index = _update_callback_list.IndexOf(update_callback);
            if (index < 0)
            {
                _update_callback_list.Add(update_callback);
            }
        }
        public void registerLateUpdate(Delegate.CallFuncVoid update_callback)
        {
            int index = _late_update_callback_list.IndexOf(update_callback);
            if (index < 0)
            {
                _late_update_callback_list.Add(update_callback);
            }
        }
        public void unregisterUpdate(Delegate.CallFuncVoid update_callback)
        {
            int index = _update_callback_list.IndexOf(update_callback);
            if (index >= 0)
            {
                //在执行时从List删除
                _update_callback_list[index] = null;
            }
        }
        public void unregisterLateUpdate(Delegate.CallFuncVoid update_callback)
        {
            int index = _late_update_callback_list.IndexOf(update_callback);
            if (index >= 0)
            {
                //在执行时从List删除
                _late_update_callback_list[index] = null;
            }
        }
        /*
        private void calueCameraRect()
		{
			GameObject img_1 = GameObject.Find("Image_1");
			GameObject img_2 = GameObject.Find("Image_2");
			img_1.transform.SetParent(null);
			img_2.transform.SetParent(null);

			Image image = img_1.GetComponent<Image>();
			image.color = new Color(image.color.r, image.color.g, image.color.b, 1);

			image = img_2.GetComponent<Image>();
			image.color = new Color(image.color.r, image.color.g, image.color.b, 1);

			GameObject canvas = GameObject.Find("Over ParticleSystem");
			img_1.transform.SetParent(canvas.transform);
			img_2.transform.SetParent(canvas.transform);
			img_1.SetActive(true);
			img_2.SetActive(true);
			if (!img_1 || !img_2) return;
			RectTransform img_trans_1 = (RectTransform)img_1.transform;
			RectTransform img_trans_2 = (RectTransform)img_2.transform;

			float screen_delta = Screen.width * 1.0f / Screen.height;
			{
				_camera_rect = new Rect(0, 0, 1, 1);
			}
		}
        */
		private void set3DCameraRect()
		{
			//GameObject camera = GameObject.Find("3D Camera");
			//Camera cam = camera.GetComponent<Camera>();
			//cam.rect = _camera_rect;
			//GameObject camera2 = GameObject.Find("Front Camera");
			//Camera cam2 = camera2.GetComponent<Camera>();
			//cam2.rect = _camera_rect;
		}
		
    }
}