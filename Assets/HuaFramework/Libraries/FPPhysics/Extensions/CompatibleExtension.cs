/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2021/2/3 17:41:11
 */

using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;

#if UNITY_EDITOR

using UnityEditor;

#endif

using FPPhysics;
using Vector2 = FPPhysics.Vector2;
using Vector3 = FPPhysics.Vector3;
using Vector4 = FPPhysics.Vector4;
using Quaternion = FPPhysics.Quaternion;
using Matrix4x4 = FPPhysics.Matrix4x4;
using UVector2 = UnityEngine.Vector2;
using UVector3 = UnityEngine.Vector3;
using UVector4 = UnityEngine.Vector4;
using UQuaternion = UnityEngine.Quaternion;
using UMatrix4x4 = UnityEngine.Matrix4x4;
using Mathf = FPPhysics.FPUtility;
using Single = FPPhysics.Fix64;

using Newtonsoft.Json.Linq;

namespace AliveCell
{
    //[Newtonsoft.Json.Shims.Preserve]
    public class FPVectorConverter : JsonConverter
    {
        private static readonly Type FP = typeof(Fix64);
        private static readonly Type V2 = typeof(Vector2);
        private static readonly Type V3 = typeof(Vector3);
        private static readonly Type V4 = typeof(Vector4);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            Type type = value.GetType();
            if (type == FP)
            {
                Fix64 fp = (Fix64)value;
                WriteFP(writer, fp);
            }
            else if (type == V2)
            {
                Vector2 vector = (Vector2)value;
                WriteVector(writer, vector.x, vector.y, null, null);
            }
            else if (type == V3)
            {
                Vector3 vector2 = (Vector3)value;
                WriteVector(writer, vector2.x, vector2.y, vector2.z, null);
            }
            else if (type == V4)
            {
                Vector4 vector3 = (Vector4)value;
                WriteVector(writer, vector3.x, vector3.y, vector3.z, vector3.w);
            }
            else
            {
                writer.WriteNull();
            }
        }

        private void WriteFP(JsonWriter writer, Fix64 fp)
        {
            writer.WriteValue((float)fp);
        }

        private static void WriteVector(JsonWriter writer, Fix64 x, Fix64 y, Fix64? z, Fix64? w)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue((float)x);
            writer.WritePropertyName("y");
            writer.WriteValue((float)y);
            if (z.HasValue)
            {
                writer.WritePropertyName("z");
                writer.WriteValue((float)z.Value);
                if (w.HasValue)
                {
                    writer.WritePropertyName("w");
                    writer.WriteValue((float)w.Value);
                }
            }
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (objectType == FP)
            {
                return PopulateFP(reader);
            }
            else if (objectType == V2)
            {
                return PopulateVector2(reader);
            }
            if (objectType == V3)
            {
                return PopulateVector3(reader);
            }
            return PopulateVector4(reader);
        }

        public override bool CanConvert(Type objectType)
        {
            if ((objectType != FP) && (objectType != V2) && (objectType != V3) && (objectType != V4))
            {
                return false;
            }
            return true;
        }

        private object PopulateFP(JsonReader reader)
        {
            Fix64 result = default(Fix64);
            if (reader.TokenType != JsonToken.Null && reader.Value != null)
            {
                result = (Fix64)Convert.ToDouble(reader.Value);
            }
            return result;
        }

        private static Vector2 PopulateVector2(JsonReader reader)
        {
            Vector2 result = default(Vector2);
            if (reader.TokenType != JsonToken.Null)
            {
                JObject jObject = JObject.Load(reader);
                result.x = (Fix64)jObject["x"].Value<float>();
                result.y = (Fix64)jObject["y"].Value<float>();
            }
            return result;
        }

        private static Vector3 PopulateVector3(JsonReader reader)
        {
            Vector3 result = default(Vector3);
            if (reader.TokenType != JsonToken.Null)
            {
                JObject jObject = JObject.Load(reader);
                result.x = (Fix64)jObject["x"].Value<float>();
                result.y = (Fix64)jObject["y"].Value<float>();
                result.z = (Fix64)jObject["z"].Value<float>();
            }
            return result;
        }

        private static Vector4 PopulateVector4(JsonReader reader)
        {
            Vector4 result = default(Vector4);
            if (reader.TokenType != JsonToken.Null)
            {
                JObject jObject = JObject.Load(reader);
                result.x = (Fix64)jObject["x"].Value<float>();
                result.y = (Fix64)jObject["y"].Value<float>();
                result.z = (Fix64)jObject["z"].Value<float>();
                result.w = (Fix64)jObject["w"].Value<float>();
            }
            return result;
        }
    }

#if UNITY_EDITOR

    public static class StaticOperation
    {
        [UnityEditor.Callbacks.DidReloadScripts]
        public static void Reload()
        {
            if (DataUtility.jsonSetting.Converters == null)
            {
                DataUtility.jsonSetting.Converters = new List<JsonConverter>();
            }
            DataUtility.jsonSetting.Converters.Add(new FPVectorConverter());
        }
    }

    [CustomPropertyDrawer(typeof(FPPhysics.Fix64))]
    public class Fix64Drawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty rawValueProperty = property.FindPropertyRelative("RawValue");
            label.text += "'";
            rawValueProperty.longValue = ((Fix64)EditorGUI.FloatField(
                position, label,
                (float)new Fix64() { RawValue = rawValueProperty.longValue })).RawValue;
        }

        [ObjectDrawer(typeof(FPPhysics.Fix64))]
        public static object Drawer(GUIContent title, object obj, Type type, object[] attrs)
        {
            RangeAttribute range = null;
            if (attrs != null)
            {
                range = Array.Find(attrs, t => t is RangeAttribute) as RangeAttribute;
            }

            title.text += "'";

            if (range != null)
            {
                obj = (Fix64)EditorGUILayout.Slider(title, (Fix64)obj, range.min, range.max);
            }
            else
            {
                obj = (Fix64)EditorGUILayout.FloatField(title, (Fix64)obj);
            }
            return obj;
        }
    }

#endif
}