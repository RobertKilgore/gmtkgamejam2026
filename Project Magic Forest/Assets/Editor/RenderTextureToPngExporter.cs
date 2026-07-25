using System.IO;
using UnityEditor;
using UnityEngine;

public class RenderTextureToPngExporter : EditorWindow
{
    private RenderTexture targetTexture;
    private string outputPath;

    [MenuItem("Tools/Render Texture/Export to PNG")]
    private static void ShowWindow()
    {
        var window = GetWindow<RenderTextureToPngExporter>("RenderTexture PNG Exporter");
        window.minSize = new Vector2(360, 240);
    }

    private void OnEnable()
    {
        if (Selection.activeObject is RenderTexture renderTexture)
            targetTexture = renderTexture;

        if (string.IsNullOrEmpty(outputPath))
            outputPath = Path.Combine(Application.dataPath, "..", "Exports");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Export a RenderTexture to a PNG file", EditorStyles.wordWrappedLabel);

        EditorGUI.BeginChangeCheck();
        targetTexture = (RenderTexture)EditorGUILayout.ObjectField("Render Texture", targetTexture, typeof(RenderTexture), false);

        if (EditorGUI.EndChangeCheck() && targetTexture == null)
        {
            var selectedTexture = Selection.activeObject as RenderTexture;
            if (selectedTexture != null)
                targetTexture = selectedTexture;
        }

        if (GUILayout.Button("Use Selected Asset"))
        {
            var selectedTexture = Selection.activeObject as RenderTexture;
            if (selectedTexture != null)
                targetTexture = selectedTexture;
            else
                EditorUtility.DisplayDialog("No RenderTexture selected", "Select a RenderTexture in the Project or Hierarchy first.", "OK");
        }

        EditorGUILayout.Space();

        outputPath = EditorGUILayout.TextField("Output Folder", outputPath);
        if (GUILayout.Button("Choose Output Folder"))
        {
            string selected = EditorUtility.OpenFolderPanel("Choose output folder", outputPath, "");
            if (!string.IsNullOrEmpty(selected))
                outputPath = selected;
        }

        EditorGUILayout.Space();

        GUI.enabled = targetTexture != null && !string.IsNullOrEmpty(outputPath);
        if (GUILayout.Button("Export PNG"))
        {
            Export();
        }
        GUI.enabled = true;

        if (targetTexture != null)
        {
            EditorGUILayout.LabelField("Size", $"{targetTexture.width}x{targetTexture.height}");
            EditorGUILayout.LabelField("Format", targetTexture.format.ToString());
        }
    }

    private void Export()
    {
        if (targetTexture == null)
        {
            EditorUtility.DisplayDialog("No RenderTexture", "Select a RenderTexture first.", "OK");
            return;
        }

        if (string.IsNullOrEmpty(outputPath))
        {
            EditorUtility.DisplayDialog("No output path", "Choose an output folder first.", "OK");
            return;
        }

        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);

        string fileName = $"{targetTexture.name}_{targetTexture.width}x{targetTexture.height}.png";
        string fullPath = Path.Combine(outputPath, fileName);

        var texture = new Texture2D(targetTexture.width, targetTexture.height, TextureFormat.RGBA32, false);

        var previousActive = RenderTexture.active;
        RenderTexture.active = targetTexture;

        texture.ReadPixels(new Rect(0, 0, targetTexture.width, targetTexture.height), 0, 0);
        texture.Apply();

        RenderTexture.active = previousActive;

        File.WriteAllBytes(fullPath, texture.EncodeToPNG());
        DestroyImmediate(texture);

        AssetDatabase.Refresh();

        EditorUtility.RevealInFinder(fullPath);
        EditorUtility.DisplayDialog("Export complete", $"Saved PNG to:\n{fullPath}", "OK");
    }
}