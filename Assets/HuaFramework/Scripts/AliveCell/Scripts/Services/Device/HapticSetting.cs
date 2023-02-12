/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/12/25 16:12:58
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;
using MoreMountains.NiceVibrations;

#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.AnimatedValues;

#endif

namespace AliveCell
{
    public enum HapticMethods
    {
        None = 0,
        NativePreset,
        Transient,
        Continuous,
        Stop,
        Advance,
    }

    [Serializable]
    public class HapticInfo
    {
        public string name = "";
        public HapticMethods hapticMethod = HapticMethods.NativePreset;
        public bool allowRumble = true;

        //NativePreset

        public HapticTypes hapticType = HapticTypes.None;

        //Transient

        [Range(0, 1)]
        public float intensity = 1f;

        [Range(0, 1)]
        public float sharpness = 1f;

        //Continuous

        public float duration = 1f;
        public bool independTimeScale = false;
        public AnimationCurve intensityScaleCurve;
        public AnimationCurve sharpnessScaleCurve;

        //Advance

        public TextAsset AHAPFile;
        public MMNVAndroidWaveFormAsset androidWave;
        public MMNVRumbleWaveFormAsset rumbleWave;

        public override string ToString()
        {
            return $"Haptic({name}):method={hapticMethod}, hapticType={hapticType}, intensity={intensity}, sharpness={sharpness}, duration={duration}";
        }
    }

    [CreateAssetMenu(menuName = "AliveCell/HapticSetting")]
    [Serializable]
    public class HapticSetting : ScriptableObject
    {
        public List<HapticInfo> hapticInfos;
    }

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(HapticInfo))]
    public class HapticInfoProperty : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty nameSp = property.FindPropertyRelative("name");
            SerializedProperty hapticMethodSp = property.FindPropertyRelative("hapticMethod");
            SerializedProperty hapticTypeSp = property.FindPropertyRelative("hapticType");
            SerializedProperty intensitySp = property.FindPropertyRelative("intensity");
            SerializedProperty sharpnessSp = property.FindPropertyRelative("sharpness");
            SerializedProperty independTimeScaleSp = property.FindPropertyRelative("independTimeScale");
            SerializedProperty intensityScaleCurveSp = property.FindPropertyRelative("intensityScaleCurve");
            SerializedProperty sharpnessScaleCurveSp = property.FindPropertyRelative("sharpnessScaleCurve");
            SerializedProperty durationSp = property.FindPropertyRelative("duration");
            SerializedProperty allowRumbleSp = property.FindPropertyRelative("allowRumble");
            SerializedProperty AHAPFile = property.FindPropertyRelative("AHAPFile");
            SerializedProperty androidWave = property.FindPropertyRelative("androidWave");
            SerializedProperty rumbleWave = property.FindPropertyRelative("rumbleWave");

            float height = 0f;
            height += EditorGUIUtility.singleLineHeight;
            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                height += EditorGUI.GetPropertyHeight(nameSp);
                height += EditorGUI.GetPropertyHeight(hapticMethodSp);

                HapticMethods method = (HapticMethods)hapticMethodSp.intValue;

                switch (method)
                {
                    case HapticMethods.NativePreset:
                        height += EditorGUI.GetPropertyHeight(allowRumbleSp);
                        height += EditorGUI.GetPropertyHeight(hapticTypeSp);
                        break;

                    case HapticMethods.Transient:
                        height += EditorGUI.GetPropertyHeight(allowRumbleSp);
                        height += EditorGUI.GetPropertyHeight(intensitySp);
                        height += EditorGUI.GetPropertyHeight(sharpnessSp);
                        break;

                    case HapticMethods.Continuous:
                        height += EditorGUI.GetPropertyHeight(allowRumbleSp);
                        height += EditorGUI.GetPropertyHeight(intensitySp);
                        height += EditorGUI.GetPropertyHeight(sharpnessSp);
                        height += EditorGUI.GetPropertyHeight(durationSp);
                        height += EditorGUI.GetPropertyHeight(independTimeScaleSp);
                        height += EditorGUI.GetPropertyHeight(intensityScaleCurveSp);
                        height += EditorGUI.GetPropertyHeight(sharpnessScaleCurveSp);
                        break;

                    case HapticMethods.Advance:
                        height += EditorGUI.GetPropertyHeight(AHAPFile);
                        height += EditorGUI.GetPropertyHeight(androidWave);
                        height += EditorGUI.GetPropertyHeight(rumbleWave);
                        break;
                }
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty nameSp = property.FindPropertyRelative("name");
            SerializedProperty hapticMethodSp = property.FindPropertyRelative("hapticMethod");
            SerializedProperty hapticTypeSp = property.FindPropertyRelative("hapticType");
            SerializedProperty intensitySp = property.FindPropertyRelative("intensity");
            SerializedProperty sharpnessSp = property.FindPropertyRelative("sharpness");
            SerializedProperty independTimeScaleSp = property.FindPropertyRelative("independTimeScale");
            SerializedProperty intensityScaleCurveSp = property.FindPropertyRelative("intensityScaleCurve");
            SerializedProperty sharpnessScaleCurveSp = property.FindPropertyRelative("sharpnessScaleCurve");
            SerializedProperty durationSp = property.FindPropertyRelative("duration");
            SerializedProperty allowRumbleSp = property.FindPropertyRelative("allowRumble");
            SerializedProperty AHAPFile = property.FindPropertyRelative("AHAPFile");
            SerializedProperty androidWave = property.FindPropertyRelative("androidWave");
            SerializedProperty rumbleWave = property.FindPropertyRelative("rumbleWave");

            EditorGUI.BeginProperty(position, label, property);
            Rect rect = EditorGUI.IndentedRect(position);

            rect.height = EditorGUIUtility.singleLineHeight;
            property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, label, true);
            rect.y += rect.height;
            if (property.isExpanded)
            {
                rect.height = EditorGUI.GetPropertyHeight(nameSp);
                EditorGUI.PropertyField(rect, nameSp);
                rect.y += rect.height;

                EditorGUI.BeginChangeCheck();
                rect.height = EditorGUI.GetPropertyHeight(hapticMethodSp);
                EditorGUI.PropertyField(rect, hapticMethodSp);
                rect.y += rect.height;
                bool methodChanged = EditorGUI.EndChangeCheck();

                HapticMethods method = (HapticMethods)hapticMethodSp.intValue;
                if (methodChanged && method != HapticMethods.Continuous)
                {
                    intensityScaleCurveSp.animationCurveValue = AnimationCurve.Constant(0, 1, 1);
                    sharpnessScaleCurveSp.animationCurveValue = AnimationCurve.Constant(0, 1, 1);
                }

                switch (method)
                {
                    case HapticMethods.NativePreset:
                        rect.height = EditorGUI.GetPropertyHeight(allowRumbleSp);
                        EditorGUI.PropertyField(rect, allowRumbleSp);
                        rect.y += rect.height;
                        rect.height = EditorGUI.GetPropertyHeight(hapticTypeSp);
                        EditorGUI.PropertyField(rect, hapticTypeSp);
                        rect.y += rect.height;
                        break;

                    case HapticMethods.Transient:
                        rect.height = EditorGUI.GetPropertyHeight(allowRumbleSp);
                        EditorGUI.PropertyField(rect, allowRumbleSp);
                        rect.y += rect.height;
                        rect.height = EditorGUI.GetPropertyHeight(intensitySp);
                        EditorGUI.PropertyField(rect, intensitySp);
                        rect.y += rect.height;
                        rect.height = EditorGUI.GetPropertyHeight(sharpnessSp);
                        EditorGUI.PropertyField(rect, sharpnessSp);
                        rect.y += rect.height;
                        break;

                    case HapticMethods.Continuous:
                        rect.height = EditorGUI.GetPropertyHeight(allowRumbleSp);
                        EditorGUI.PropertyField(rect, allowRumbleSp);
                        rect.y += rect.height;
                        rect.height = EditorGUI.GetPropertyHeight(intensitySp);
                        EditorGUI.PropertyField(rect, intensitySp);
                        rect.y += rect.height;
                        rect.height = EditorGUI.GetPropertyHeight(sharpnessSp);
                        EditorGUI.PropertyField(rect, sharpnessSp);
                        rect.y += rect.height;
                        rect.height = EditorGUI.GetPropertyHeight(durationSp);
                        EditorGUI.PropertyField(rect, durationSp);
                        rect.y += rect.height;
                        rect.height = EditorGUI.GetPropertyHeight(independTimeScaleSp);
                        EditorGUI.PropertyField(rect, independTimeScaleSp);
                        rect.y += rect.height;
                        rect.height = EditorGUI.GetPropertyHeight(intensityScaleCurveSp);
                        EditorGUI.PropertyField(rect, intensityScaleCurveSp);
                        rect.y += rect.height;
                        rect.height = EditorGUI.GetPropertyHeight(sharpnessScaleCurveSp);
                        EditorGUI.PropertyField(rect, sharpnessScaleCurveSp);
                        rect.y += rect.height;
                        break;

                    case HapticMethods.Advance:
                        rect.height = EditorGUI.GetPropertyHeight(AHAPFile);
                        EditorGUI.PropertyField(rect, AHAPFile);
                        rect.y += rect.height;
                        rect.height = EditorGUI.GetPropertyHeight(androidWave);
                        EditorGUI.PropertyField(rect, androidWave);
                        rect.y += rect.height;
                        rect.height = EditorGUI.GetPropertyHeight(rumbleWave);
                        EditorGUI.PropertyField(rect, rumbleWave);
                        rect.y += rect.height;
                        break;
                }
            }
            EditorGUI.EndProperty();
        }
    }

#endif
}