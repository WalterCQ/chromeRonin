using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Editor utility to create blood splatter decal prefabs.
/// Access via menu: Tools > Blood Effects > Create Decal Prefabs
/// </summary>
public class BloodDecalPrefabCreator : EditorWindow
{
    private Color decalColor = new Color(0.5f, 0.02f, 0.02f, 0.9f);
    private int numberOfPrefabs = 4;
    private string savePath = "Assets/Prefab/Effects/BloodDecals";
    
    [MenuItem("Tools/Blood Effects/Create Decal Prefabs")]
    public static void ShowWindow()
    {
        GetWindow<BloodDecalPrefabCreator>("Blood Decal Creator");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Blood Decal Prefab Creator", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        decalColor = EditorGUILayout.ColorField("Decal Color", decalColor);
        numberOfPrefabs = EditorGUILayout.IntSlider("Number of Prefabs", numberOfPrefabs, 1, 8);
        savePath = EditorGUILayout.TextField("Save Path", savePath);
        
        GUILayout.Space(20);
        
        if (GUILayout.Button("Create Decal Prefabs", GUILayout.Height(40)))
        {
            CreateDecalPrefabs();
        }
        
        GUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "This will create simple placeholder blood decal prefabs using Unity's default sprite.\n\n" +
            "For better results, replace the sprites with actual blood splatter art assets.", 
            MessageType.Info);
    }
    
    private void CreateDecalPrefabs()
    {
        // Ensure directory exists
        if (!AssetDatabase.IsValidFolder(savePath))
        {
            string[] pathParts = savePath.Split('/');
            string currentPath = pathParts[0];
            for (int i = 1; i < pathParts.Length; i++)
            {
                string newPath = currentPath + "/" + pathParts[i];
                if (!AssetDatabase.IsValidFolder(newPath))
                {
                    AssetDatabase.CreateFolder(currentPath, pathParts[i]);
                }
                currentPath = newPath;
            }
        }
        
        for (int i = 0; i < numberOfPrefabs; i++)
        {
            CreateSingleDecalPrefab(i);
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"Created {numberOfPrefabs} blood decal prefabs in {savePath}");
        EditorUtility.DisplayDialog("Success", $"Created {numberOfPrefabs} blood decal prefabs!", "OK");
    }
    
    private void CreateSingleDecalPrefab(int index)
    {
        // Create a new GameObject
        GameObject decal = new GameObject($"BloodDecal_{index + 1}");
        
        // Add SpriteRenderer
        SpriteRenderer sr = decal.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite(index);
        sr.color = decalColor;
        sr.sortingLayerName = "Default";
        sr.sortingOrder = -10;
        
        // Save as prefab
        string prefabPath = $"{savePath}/BloodDecal_{index + 1}.prefab";
        PrefabUtility.SaveAsPrefabAsset(decal, prefabPath);
        
        // Clean up the scene object
        DestroyImmediate(decal);
    }
    
    private Sprite CreateCircleSprite(int index)
    {
        // Use Unity's built-in Knob sprite as a placeholder (it's a circle)
        // In production, replace with actual blood splatter sprites
        Sprite knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        return knob;
    }
}
#endif
