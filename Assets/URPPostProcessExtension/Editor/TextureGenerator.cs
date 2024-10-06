using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class TextureGeneratorEditor : EditorWindow
{
    Vector2Int size;
    int select;
    List<Type> types = new List<Type>();
    List<string> names = new List<string>();
    bool dirty = true;
    SerializedObject serializedObject;
    TextureGenerator generator;
    Texture2D texture;
    string savePath = "Texture.png";

    [MenuItem("Tools/TextureGenerator")]
    public static void OpenWindow()
    {
        TextureGeneratorEditor window = GetWindow<TextureGeneratorEditor>();
        window.dirty = true;
        window.select = 0;
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    public static void Reload()
    {
        if (HasOpenInstances<TextureGeneratorEditor>())
        {
            TextureGeneratorEditor window = GetWindow<TextureGeneratorEditor>();
            window.dirty = true;
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginVertical();
        if (dirty)
        {
            types.Clear();
            names.Clear();
            foreach (Type type in typeof(TextureGenerator).Assembly.GetTypes())
            {
                if (!type.IsAbstract && type.IsSubclassOf(typeof(TextureGenerator)))
                {
                    types.Add(type);
                    names.Add(type.Name);
                }
            }
            select = 0;
            if (names.Count > 0)
            {
                RefreshInstance();
            }
            dirty = false;
        }
        if (names.Count > 0)
        {
            int tempSelect = EditorGUILayout.Popup("GeneratorSelect", select, names.ToArray());
            if (tempSelect != select)
            {
                select = tempSelect;
                RefreshInstance();
            }
            size = EditorGUILayout.Vector2IntField("Size", size);
            serializedObject.Update();
            foreach (var field in generator.GetType().GetFields())
            {
                SerializedProperty sp = serializedObject.FindProperty(field.Name);
                EditorGUILayout.PropertyField(sp);
            }
            serializedObject.ApplyModifiedProperties();
            if (GUILayout.Button("Generate"))
            {
                texture = generator.Generate(size);
            }
            if (GUILayout.Button("Save"))
            {
                byte[] data = texture.EncodeToPNG();
                FileStream file = new FileStream(Application.dataPath + "/" + savePath, FileMode.OpenOrCreate);
                file.Write(data, 0, data.Length);
                file.Close();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label("Assets/", GUILayout.ExpandWidth(false));
            savePath = EditorGUILayout.TextField(savePath, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
            GUILayout.Box(texture, GUILayout.ExpandWidth(true));
        }
        else
        {
            GUILayout.Label("Please create a generator first");
        }
        GUILayout.EndVertical();
    }

    private void RefreshInstance()
    {
        generator = CreateInstance(types[select]) as TextureGenerator;
        serializedObject = new SerializedObject(generator);
    }
}

public abstract class TextureGenerator : UnityEngine.ScriptableObject
{
    public abstract Texture2D Generate(Vector2Int size);
}