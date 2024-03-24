/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/9/14 14:40:45
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;
using System;

#if UNITY_EDITOR

using UnityEditor;
using UnityEditorInternal;

#endif

namespace AliveCell
{
    /// <summary>
    /// LevelController
    /// </summary>
    public class LevelController : MonoBehaviour
    {
        [Serializable]
        public class TransformInfo
        {
            public Vector3 position;
            public float yAngle;

            public Quaternion rotation => Quaternion.Euler(0f, yAngle, 0f);
        }

        [Serializable]
        public class EnemyInfo : TransformInfo
        {
            public int enemyType;
        }

        [SerializeField]
        public Transform colliderMeshRoot;

        [SerializeField]
        public List<TransformInfo> playerBirthPoints = new List<TransformInfo>();

        [SerializeField]
        public List<EnemyInfo> enemyBirthPoints = new List<EnemyInfo>();

        public Dictionary<int, int> GetPreCreatePrefabCount()
        {
            Dictionary<int, int> results = new Dictionary<int, int>();

            int colliderCount = 0;
            foreach (var item in enemyBirthPoints)
            {
                colliderCount++;
                AliveCell.EnemyInfo enemyInfo = App.res.LoadEnemyInfo(item.enemyType);
                if (!results.TryGetValue(enemyInfo.prefabID, out int count))
                {
                    count = 0;
                }
                results[enemyInfo.prefabID] = ++count;
            }
            results[20000001] = 1; colliderCount++;
            results[20000002] = 1; colliderCount++;
            results[10000001] = colliderCount;

            return results;
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(LevelController))]
    public class LevelControllerEditor : Editor
    {
        public SerializedProperty colliderMeshRoot;
        public SerializedProperty playerBirthPoints;
        public SerializedProperty enemyBirthPoints;

        private ReorderableList player;
        private ReorderableList enemy;

        private bool showHandles = false;

        private void OnEnable()
        {
            colliderMeshRoot =serializedObject.FindProperty("colliderMeshRoot");
            playerBirthPoints = serializedObject.FindProperty("playerBirthPoints");
            player = new ReorderableList(serializedObject, playerBirthPoints);
            player.elementHeight = 2 * EditorGUIUtility.singleLineHeight;
            player.drawHeaderCallback = (rt) => OnDrawHeader("Player Points", rt);
            player.drawElementCallback = (rect, index, isActive, isFocused) => OnDrawElement(player, rect, index, isActive, isFocused);

            enemyBirthPoints = serializedObject.FindProperty("enemyBirthPoints");
            enemy = new ReorderableList(serializedObject, enemyBirthPoints);
            enemy.elementHeight = 3 * EditorGUIUtility.singleLineHeight;
            enemy.drawHeaderCallback = (rt) => OnDrawHeader("Enemy Points", rt);
            enemy.drawElementCallback = (rect, index, isActive, isFocused) => OnDrawElement(enemy, rect, index, isActive, isFocused);
        }

        private void OnDrawElement(ReorderableList list, Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = list.serializedProperty.GetArrayElementAtIndex(index);

            var position = element.FindPropertyRelative("position");
            var yAngle = element.FindPropertyRelative("yAngle");
            var enemyType = element.FindPropertyRelative("enemyType");

            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), position);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight, rect.width, EditorGUIUtility.singleLineHeight), yAngle);
            if (enemyType != null)
            {
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight * 2, rect.width, EditorGUIUtility.singleLineHeight), enemyType);
            }
        }

        private void OnDisable()
        {
        }

        private void OnDrawHeader(string title, Rect rect)
        {
            GUI.Label(rect, title);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            EditorGUILayout.PropertyField(colliderMeshRoot);

            player.DoLayoutList();
            enemy.DoLayoutList();

            if (serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Editor Tools");
            showHandles = GUILayout.Toggle(showHandles, "Show Handles");
            EditorGUILayout.EndVertical();
        }

        public void DrawList(SerializedProperty property, ReorderableList list, Color color)
        {
            Color oldColor = Handles.color;
            Handles.color = color;
            for (int i = 0; i < property.arraySize; i++)
            {
                var item = property.GetArrayElementAtIndex(i);
                var position = item.FindPropertyRelative("position");
                var yAngle = item.FindPropertyRelative("yAngle");

                Vector3 val1 = position.vector3Value;
                Quaternion val2 = Quaternion.Euler(0, yAngle.floatValue, 0);

                if (showHandles)
                {
                    Vector3 _val1 = val1;
                    Quaternion _val2 = val2;

                    Handles.TransformHandle(ref val1, ref val2);
                    if (_val1 != val1 || _val2 != val2)
                    {
                        list.index = i;

                        position.vector3Value = val1;
                        yAngle.floatValue = val2.eulerAngles.y;
                    }
                }

                Handles.SphereHandleCap(GUIUtility.GetControlID(FocusType.Passive), val1, val2, 1f, EventType.Repaint);
            }
            Handles.color = oldColor;
        }

        public void OnSceneGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            DrawList(playerBirthPoints, player, Color.blue);
            DrawList(enemyBirthPoints, enemy, Color.red);

            if (serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }

#endif
}