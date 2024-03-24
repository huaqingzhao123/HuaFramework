/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/12/9 16:47:44
 */

using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using XMLib;
using Ease = DG.Tweening.Ease;
using System;
using XMLib.Extensions;

#if UNITY_EDITOR

using UnityEditorInternal;
using UnityEditor;
using UnityEditor.Experimental;

#endif

namespace AliveCell
{
    /// <summary>
    /// UIAnimControl
    /// </summary>
    public class UIAnimControl : MonoBehaviour, IResourcePoolCallback
    {
        [SerializeReference]
        protected List<UIAnimBase> _anims;

        public UIControl parent { get; protected set; }

        protected virtual void Awake()
        {
            UpdateParent();
        }

        public Tween GetTween(UIPanelStatus status, bool enterOrExit)
        {
            Sequence seq = DOTween.Sequence();
            foreach (var item in _anims)
            {
                if (item.status != status || item.enterOrExit != enterOrExit)
                {
                    continue;
                }

                Tween tween = item.GetTween();
                if (item.joinOrAppend)
                {
                    seq.Join(tween);
                }
                else
                {
                    seq.Append(tween);
                }
            }
            return seq;
        }

        public void OnInitialize()
        {
            foreach (var item in _anims)
            {
                item.OnInitialize(gameObject);
            }
        }

        public void OnTransformParentChanged()
        {
            UpdateParent();
        }

        public void UpdateParent()
        {
            UIControl newParent = transform.GetComponentInParent<UIControl>();
            SetParent(newParent);
        }

        public void SetParent(UIControl control)
        {
            if (control == parent)
            {
                return;
            }

            if (parent != null)
            {
                parent.RemoveAnim(this);
                parent = null;
            }
            parent = control;
            if (parent != null)
            {
                parent.AppendAnim(this);
            }
        }

        public void OnPushPool()
        {
        }

        public void OnPopPool()
        {
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(UIAnimControl))]
    public class UIAnimControlEditor : Editor
    {
        private ReorderableList _anims;

        private char[] splits = new char[] { '.', '/', '\\' };

        protected void OnEnable()
        {
            _anims = new ReorderableList(serializedObject, serializedObject.FindProperty("_anims"), true, true, true, true);
            _anims.drawHeaderCallback = OnDrawHeaderCallback;
            _anims.onAddDropdownCallback = OnAddDropdownCallback;
            _anims.drawElementCallback = OnDrawElementCallback;
            _anims.elementHeightCallback = OnElementHeightCallback;
        }

        private float OnElementHeightCallback(int index)
        {
            SerializedProperty element = _anims.serializedProperty.GetArrayElementAtIndex(index);
            return EditorGUI.GetPropertyHeight(element, true);
        }

        private void OnDrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.x += 12;
            rect.width -= 12;

            SerializedProperty element = _anims.serializedProperty.GetArrayElementAtIndex(index);
            if (!string.IsNullOrEmpty(element.managedReferenceFullTypename))
            {
                UIPanelStatus status = (UIPanelStatus)element.FindPropertyRelative("status").intValue;
                string enterOrExit = element.FindPropertyRelative("enterOrExit").boolValue ? "Enter" : "Exit";
                string joinOrAppend = element.FindPropertyRelative("joinOrAppend").boolValue ? "Join" : "Append";
                string typeName = element.managedReferenceFullTypename.Substring(element.managedReferenceFullTypename.LastIndexOfAny(splits) + 1);
                GUIContent content = new GUIContent($"{typeName}({joinOrAppend})|{status}({enterOrExit})");
                EditorGUI.PropertyField(rect, element, content, true);
            }
            else
            {
                EditorGUI.LabelField(rect, "无效类型");
            }
        }

        private void OnDrawHeaderCallback(Rect rect)
        {
            EditorGUI.LabelField(rect, "动画列表");
        }

        private void OnAddDropdownCallback(Rect buttonRect, ReorderableList list)
        {
            var types = TypeCache.GetTypesDerivedFrom<UIAnimBase>();
            var menu = new GenericMenu();
            foreach (var item in types)
            {
                menu.AddItem(new GUIContent(item.Name), false, OnClickMenu, item);
            }
            menu.ShowAsContext();
        }

        private void OnClickMenu(object obj)
        {
            int index = _anims.serializedProperty.arraySize;
            _anims.serializedProperty.arraySize++;
            _anims.index = index;

            SerializedProperty element = _anims.serializedProperty.GetArrayElementAtIndex(index);
            element.managedReferenceValue = Activator.CreateInstance((Type)obj);
            serializedObject.ApplyModifiedProperties();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.Space();
            _anims.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }

#endif
}