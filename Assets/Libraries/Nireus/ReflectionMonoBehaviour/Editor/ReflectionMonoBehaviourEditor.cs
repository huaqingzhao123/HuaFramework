using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Reflection;

namespace Nireus.Editor
{
    [CustomEditor(typeof(ReflectionMonoBehaviour))]
    public class ReflectionMonoBehaviourEditor : UnityEditor.Editor
    {
        public class Field
        {
            public FieldInfo fsieldInfo;
            public bool foldout = true;
            public string name;
            public Type type;
            public string value;
            public int size;
            public List<string> values = new List<string>();
        }

        public class Method
        {
            public MethodInfo methodInfo;
            public object[] parameters;
            public string name;
            public Type type;
            public string value;
            public int size;
            public List<string> values = new List<string>();
        }

        public class ClassSerialize
        {
            public MonoBehaviour monoBehaviour;
            public string name;
            public List<Field> allField = new List<Field>();
            public List<Method> allMethod = new List<Method>();
            public bool isShow = true;
        }


        ReflectionMonoBehaviour instance;

        public List<ClassSerialize> allClassSerialize;
        

        void OnEnable()
        {
            instance = target as ReflectionMonoBehaviour;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!Application.isPlaying)
            {
                return;
            }

            if (allClassSerialize == null)
            {
                allClassSerialize = new List<ClassSerialize>();

                for (int i = 0; i < instance.allMonoBehaviour.Count; i++)
                {
                    MonoBehaviour monoBehaviour = instance.allMonoBehaviour[i];

                    allClassSerialize.Add(ParseClass(monoBehaviour));
                }
            }



            ShowClass(allClassSerialize);


        }

        /// <summary>
        /// 解析类
        /// </summary>
        public ClassSerialize ParseClass(MonoBehaviour monoBehaviour)
        {
            ClassSerialize classSerialize = new ClassSerialize();

            if (monoBehaviour == null)
            {
                return classSerialize;
            }

            Type type = monoBehaviour.GetType();

            classSerialize.monoBehaviour = monoBehaviour;
            classSerialize.name = type.Name;


            FieldInfo[] allFieldInfo = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance |
                                                      BindingFlags.Public | BindingFlags.DeclaredOnly |
                                                      BindingFlags.Static);

            foreach (FieldInfo fieldInfo in allFieldInfo)
            {
                classSerialize.allField.Add(ParseField(fieldInfo));
            }

            MethodInfo[] allMemberInfo = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public |
                                                         BindingFlags.DeclaredOnly | BindingFlags.Static);

            foreach (MethodInfo memberInfo in allMemberInfo)
            {
                if (Attribute.IsDefined(memberInfo, typeof(EditShowMethodAttribute)))
                {
                    classSerialize.allMethod.Add(ParseMethod(memberInfo));
                }
            }

            return classSerialize;
        }

        /// <summary>
        /// 解析字段
        /// </summary>
        /// <returns></returns>
        public Field ParseField(FieldInfo fieldInfo)
        {
            Field field = new Field();
            field.fsieldInfo = fieldInfo;



            return field;
        }

        /// <summary>
        /// 解析方法
        /// </summary>
        /// <returns></returns>
        public Method ParseMethod(MethodInfo methodInfo)
        {
            Method method = new Method();
            method.methodInfo = methodInfo;

            return method;
        }


        public void ShowClass(List<ClassSerialize> allClassSerialize)
        {
            if (allClassSerialize == null)
            {
                return;
            }

            for (int i = 0; i < allClassSerialize.Count; i++)
            {
                ClassSerialize classSerialize = allClassSerialize[i];

                classSerialize.isShow = GUILayout.Toggle(classSerialize.isShow, classSerialize.name);

                if (classSerialize.isShow == true)
                {
                    ShowClass(classSerialize);
                }
            }
        }

        public void ShowClass(ClassSerialize classSerialize)
        {
            for (int i = 0; i < classSerialize.allField.Count; i++)
            {
                ShowField(classSerialize, classSerialize.allField[i]);
            }

            for (int i = 0; i < classSerialize.allMethod.Count; i++)
            {
                ShowMethod(classSerialize, classSerialize.allMethod[i]);
            }
        }

        public void ShowMethod(ClassSerialize classSerialize, Method method)
        {
            ParameterInfo[] allParameterInfo = method.methodInfo.GetParameters();

            if (method.parameters == null)
            {
                method.parameters = new object[allParameterInfo.Length];
            }

            for (int i = 0; i < allParameterInfo.Length; i++)
            {
                ParameterInfo parameterInfo = allParameterInfo[i];

                if (parameterInfo.ParameterType == typeof(int))
                {
                    int value = (int) method.parameters[i];

                    if (ShowInt(parameterInfo.Name, ref value))
                    {
                        method.parameters[i] = value;
                    }
                }
                else if (parameterInfo.ParameterType == typeof(string))
                {
                    string value = string.Empty;
                    if (method.parameters[i] != null)
                    {
                        value = method.parameters[i].ToString();
                    }

                    if (ShowString(parameterInfo.Name, ref value))
                    {
                        method.parameters[i] = value;
                    }
                }
            }



            if (GUILayout.Button(method.methodInfo.Name))
            {
                method.methodInfo.Invoke(classSerialize.monoBehaviour, method.parameters);
            }
        }

        public void ShowField(ClassSerialize classSerialize, Field field)
        {
            if (field.fsieldInfo.FieldType == typeof(int))
            {
                ShowField_int(field, classSerialize.monoBehaviour);
            }
            else if (field.fsieldInfo.FieldType == typeof(float))
            {
                ShowField_float(field, classSerialize.monoBehaviour);
            }
            else if (field.fsieldInfo.FieldType == typeof(string))
            {
                ShowField_string(field, classSerialize.monoBehaviour);
            }
            else if (field.fsieldInfo.FieldType == typeof(List<int>))
            {
                ShowField_List_int(field, classSerialize.monoBehaviour);
            }

        }


        public void ShowField_int(Field field, object obj)
        {
            int value = (int) field.fsieldInfo.GetValue(obj);

            if (ShowInt(field.fsieldInfo.Name, ref value))
            {
                field.fsieldInfo.SetValue(obj, value);
            }
        }

        public void ShowField_float(Field field, object obj)
        {
            float value = (float) field.fsieldInfo.GetValue(obj);

            if (ShowFloat(field.fsieldInfo.Name, ref value))
            {
                field.fsieldInfo.SetValue(obj, value);
            }
        }

        public void ShowField_string(Field field, object obj)
        {
            string value = string.Empty;

            if (field.fsieldInfo.GetValue(obj) != null)
            {
                value = field.fsieldInfo.GetValue(obj).ToString();
            }

            if (ShowString(field.fsieldInfo.Name, ref value))
            {
                field.fsieldInfo.SetValue(obj, value);
            }
        }


        public void ShowField_List_int(Field field, object obj)
        {
            List<int> value = new List<int>();
            List<int> newValue = new List<int>();
            if (field.fsieldInfo.GetValue(obj) != null)
            {
                value = field.fsieldInfo.GetValue(obj) as List<int>;
            }

            bool isChange = false;

            field.foldout = EditorGUILayout.Foldout(field.foldout, field.fsieldInfo.Name);
            if (field.foldout)
            {
                int size = value.Count;

                if (ShowInt("Size", ref size))
                {
                    for (int i = 0; i < size; i++)
                    {
                        int tempValue = 0;
                        if (value.Count - 1 >= i)
                        {
                            tempValue = value[i];
                        }

                        newValue.Add(tempValue);
                    }

                    field.fsieldInfo.SetValue(obj, newValue);
                    return;
                }

                for (int i = 0; i < value.Count; i++)
                {
                    int tempValue = value[i];

                    if (ShowInt("Element " + i.ToString(), ref tempValue))
                    {
                        isChange = true;

                        value[i] = tempValue;
                    }

                    newValue.Add(tempValue);
                }

                if (isChange == true)
                {
                    field.fsieldInfo.SetValue(obj, newValue);
                }
            }
        }
        

        public bool ShowInt(string text, ref int value)
        {
            string label = EditorGUILayout.TextField(text, value.ToString());

            if (label != value.ToString())
            {
                int.TryParse(label, out value);
                return true;
            }

            return false;
        }

        public bool ShowFloat(string text, ref float value)
        {
            string label = EditorGUILayout.TextField(text, value.ToString());

            if (label != value.ToString())
            {
                float.TryParse(label, out value);
                return true;
            }

            return false;
        }

        public bool ShowString(string text, ref string value)
        {
            if (value == null)
            {
                value = string.Empty;
            }

            string label = EditorGUILayout.TextField(text, value);

            if (label != value.ToString())
            {
                value = label;
                return true;
            }

            return false;
        }
    }
}