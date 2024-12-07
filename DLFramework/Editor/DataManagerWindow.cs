using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using com.dl.framework;

namespace com.dl.framework.Editor
{
    public class DataManagerWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private Dictionary<string, bool> foldouts = new Dictionary<string, bool>();
        private Dictionary<string, object> editedObjects = new Dictionary<string, object>();
        private string searchString = "";
        private bool isDirty = false;

        [MenuItem("Tools/DLFramework/Data Manager")]
        public static void ShowWindow()
        {
            var window = GetWindow<DataManagerWindow>("Data Manager");
            window.Show();
        }

        private void OnGUI()
        {
            DrawToolbar();

            EditorGUILayout.Space();

            GUI.SetNextControlName("SearchField");
            searchString = EditorGUILayout.TextField("Search", searchString);

            EditorGUILayout.Space();

            if (isDirty)
            {
                EditorGUILayout.HelpBox("You have unsaved changes!", MessageType.Warning);
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            string saveFolder = Path.Combine(Application.persistentDataPath, "SaveData");
            if (Directory.Exists(saveFolder))
            {
                var files = Directory.GetFiles(saveFolder, "*.json");
                if (files.Length == 0)
                {
                    EditorGUILayout.HelpBox("No save files found.", MessageType.Info);
                }
                else
                {
                    foreach (var file in files)
                    {
                        string fileName = Path.GetFileNameWithoutExtension(file);

                        if (!string.IsNullOrEmpty(searchString) &&
                            !fileName.ToLower().Contains(searchString.ToLower()))
                        {
                            continue;
                        }

                        if (SaveSystem.HasData(fileName))
                        {
                            DrawDataFile(fileName, file);
                        }
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Save data folder not found.", MessageType.Info);
            }

            EditorGUILayout.EndScrollView();

            // 自动聚焦搜索框
            if (Event.current.commandName == "OnLostFocus")
            {
                EditorGUI.FocusTextInControl("SearchField");
            }
        }

        private void DrawDataFile(string fileName, string filePath)
        {
            if (!foldouts.ContainsKey(fileName))
            {
                foldouts[fileName] = false;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = foldouts[fileName] ? Color.cyan : Color.white;
            foldouts[fileName] = EditorGUILayout.Foldout(foldouts[fileName], fileName, true);
            GUI.backgroundColor = Color.white;

            GUI.enabled = isDirty && editedObjects.ContainsKey(fileName);
            if (GUILayout.Button("Save", GUILayout.Width(60)))
            {
                SaveData(fileName);
            }
            GUI.enabled = true;

            if (GUILayout.Button("Delete", GUILayout.Width(60)))
            {
                if (EditorUtility.DisplayDialog("Delete Data",
                    $"Are you sure you want to delete {fileName}?", "Yes", "No"))
                {
                    SaveSystem.DeleteData(fileName);
                    editedObjects.Remove(fileName);
                    GUIUtility.ExitGUI();
                }
            }

            EditorGUILayout.EndHorizontal();

            if (foldouts[fileName])
            {
                try
                {
                    if (!editedObjects.ContainsKey(fileName))
                    {
                        LoadData(fileName, filePath);
                    }

                    EditorGUI.BeginChangeCheck();
                    DrawObject(fileName, editedObjects[fileName]);
                    if (EditorGUI.EndChangeCheck())
                    {
                        isDirty = true;
                    }
                }
                catch (Exception e)
                {
                    EditorGUILayout.HelpBox($"Error: {e.Message}", MessageType.Error);
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void LoadData(string fileName, string filePath)
        {
            try
            {
                string encryptedData = File.ReadAllText(filePath);
                string decryptedJson = SaveSystem.DecryptString(encryptedData);

                Type dataType = FindTypeByName(fileName);
                if (dataType != null)
                {
                    object obj = JsonUtility.FromJson(decryptedJson, dataType);
                    editedObjects[fileName] = obj;
                }
                else
                {
                    DLLogger.LogError($"Cannot find type for {fileName}");
                }
            }
            catch (Exception e)
            {
                DLLogger.LogError($"Error loading {fileName}: {e.Message}");
            }
        }

        private void SaveData(string fileName)
        {
            if (editedObjects.TryGetValue(fileName, out object obj))
            {
                try
                {
                    Type objType = obj.GetType();
                    var saveMethod = typeof(SaveSystem).GetMethod("SaveData");
                    var genericSaveMethod = saveMethod.MakeGenericMethod(objType);
                    genericSaveMethod.Invoke(null, new object[] { fileName, obj });

                    isDirty = false;
                    DLLogger.Log($"Saved {fileName} successfully!");
                }
                catch (Exception e)
                {
                    DLLogger.LogError($"Error saving {fileName}: {e.Message}");
                }
            }
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
            {
                RefreshData();
            }

            GUILayout.FlexibleSpace();

            GUI.enabled = isDirty;
            if (GUILayout.Button("Save All", EditorStyles.toolbarButton))
            {
                SaveAllData();
            }
            GUI.enabled = true;

            if (GUILayout.Button("Clear All", EditorStyles.toolbarButton))
            {
                if (EditorUtility.DisplayDialog("Clear All Data",
                    "Are you sure you want to delete all saved data?", "Yes", "No"))
                {
                    ClearAllData();
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void RefreshData()
        {
            if (isDirty)
            {
                if (EditorUtility.DisplayDialog("Unsaved Changes",
                    "You have unsaved changes. Would you like to save them first?", "Yes", "No"))
                {
                    SaveAllData();
                }
            }
            editedObjects.Clear();
            isDirty = false;
            Repaint();
        }

        private void SaveAllData()
        {
            foreach (var kvp in editedObjects.ToList())
            {
                SaveData(kvp.Key);
            }
            isDirty = false;
        }

        private void ClearAllData()
        {
            string saveFolder = Path.Combine(Application.persistentDataPath, "SaveData");
            if (Directory.Exists(saveFolder))
            {
                try
                {
                    string[] files = Directory.GetFiles(saveFolder, "*.json");
                    foreach (string file in files)
                    {
                        string fileName = Path.GetFileNameWithoutExtension(file);
                        SaveSystem.DeleteData(fileName);
                    }

                    editedObjects.Clear();
                    isDirty = false;
                    AssetDatabase.Refresh();
                    DLLogger.Log("All data cleared successfully!");
                }
                catch (Exception e)
                {
                    DLLogger.LogError($"Error clearing data: {e.Message}");
                }
            }
        }

        private void DrawObject(string path, object obj)
        {
            if (obj == null) return;

            Type type = obj.GetType();
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite).ToArray();

            EditorGUI.indentLevel++;

            foreach (var field in fields)
            {
                DrawField(path, obj, field);
            }

            foreach (var prop in properties)
            {
                DrawProperty(path, obj, prop);
            }

            EditorGUI.indentLevel--;
        }

        private void DrawField(string path, object obj, FieldInfo field)
        {
            object value = field.GetValue(obj);
            object newValue = DrawValue(field.Name, value, field.FieldType);

            if (!Equals(value, newValue))
            {
                field.SetValue(obj, newValue);
                isDirty = true;
            }
        }

        private void DrawProperty(string path, object obj, PropertyInfo prop)
        {
            object value = prop.GetValue(obj);
            object newValue = DrawValue(prop.Name, value, prop.PropertyType);

            if (!Equals(value, newValue))
            {
                prop.SetValue(obj, newValue);
                isDirty = true;
            }
        }

        private object DrawValue(string name, object value, Type type)
        {
            if (type == typeof(string))
            {
                return EditorGUILayout.TextField(name, (string)value ?? "");
            }
            else if (type == typeof(int))
            {
                return EditorGUILayout.IntField(name, value != null ? (int)value : 0);
            }
            else if (type == typeof(float))
            {
                return EditorGUILayout.FloatField(name, value != null ? (float)value : 0f);
            }
            else if (type == typeof(bool))
            {
                return EditorGUILayout.Toggle(name, value != null ? (bool)value : false);
            }
            else if (type == typeof(Vector2))
            {
                return EditorGUILayout.Vector2Field(name, value != null ? (Vector2)value : Vector2.zero);
            }
            else if (type == typeof(Vector3))
            {
                return EditorGUILayout.Vector3Field(name, value != null ? (Vector3)value : Vector2.zero);
            }
            else if (type == typeof(Color))
            {
                return EditorGUILayout.ColorField(name, value != null ? (Color)value : Color.white);
            }
            else if (type.IsEnum)
            {
                return EditorGUILayout.EnumPopup(name, value as Enum);
            }
            else if (type.IsArray)
            {
                return DrawArray(name, value as Array, type);
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                return DrawList(name, value as IList, type);
            }
            else if (!type.IsPrimitive && type != typeof(string))
            {
                EditorGUILayout.LabelField(name + " (Object)");
                if (value == null) return null;
                DrawObject(name, value);
                return value;
            }

            EditorGUILayout.LabelField(name, value?.ToString() ?? "null");
            return value;
        }

        private object DrawArray(string name, Array array, Type type)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(name + " (Array)", EditorStyles.boldLabel);

            if (array == null)
            {
                array = Array.CreateInstance(type.GetElementType(), 0);
            }

            if (GUILayout.Button("+", GUILayout.Width(20)))
            {
                Array newArray = Array.CreateInstance(type.GetElementType(), array.Length + 1);
                Array.Copy(array, newArray, array.Length);
                array = newArray;
                isDirty = true;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;
            for (int i = 0; i < array.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();

                var element = DrawValue($"Element {i}", array.GetValue(i), type.GetElementType());
                array.SetValue(element, i);

                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    Array newArray = Array.CreateInstance(type.GetElementType(), array.Length - 1);
                    Array.Copy(array, 0, newArray, 0, i);
                    Array.Copy(array, i + 1, newArray, i, array.Length - i - 1);
                    array = newArray;
                    isDirty = true;
                    GUIUtility.ExitGUI();
                }

                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.EndVertical();
            return array;
        }

        private object DrawList(string name, IList list, Type type)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(name + " (List)", EditorStyles.boldLabel);

            Type elementType = type.GetGenericArguments()[0];

            if (list == null)
            {
                Type listType = typeof(List<>).MakeGenericType(elementType);
                list = (IList)Activator.CreateInstance(listType);
            }

            if (GUILayout.Button("+", GUILayout.Width(20)))
            {
                object newElement = elementType.IsValueType ? Activator.CreateInstance(elementType) : null;
                list.Add(newElement);
                isDirty = true;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;
            for (int i = 0; i < list.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                var element = DrawValue($"Element {i}", list[i], elementType);
                list[i] = element;

                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    list.RemoveAt(i);
                    isDirty = true;
                    GUIUtility.ExitGUI();
                }

                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.EndVertical();
            return list;
        }

        private Type FindTypeByName(string typeName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetTypes()
                    .FirstOrDefault(t => t.Name == typeName);
                if (type != null)
                    return type;
            }
            return null;
        }

        private void OnDestroy()
        {
            if (isDirty)
            {
                if (EditorUtility.DisplayDialog("Unsaved Changes",
                    "You have unsaved changes. Would you like to save them?", "Yes", "No"))
                {
                    SaveAllData();
                }
            }
        }
    }
}
