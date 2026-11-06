using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Unity.AI.Navigation;

public sealed class PZVisualBuilder
{
    private readonly IPZPrefabFactory factory;
    private readonly IPZPrefabVisualSection[] sections;

    public PZVisualBuilder(IPZPrefabFactory factory, IPZPrefabVisualSection[] sections)
    {
        this.factory = factory;
        this.sections = sections;
    }

    public GameObject Build(string rootName)
    {
        GameObject root = new GameObject(rootName);
        Undo.RegisterCreatedObjectUndo(root, "Bilim Kurgu Prefab Ortami");
        NavMeshModifier modifier = Undo.AddComponent<NavMeshModifier>(root);
        modifier.ignoreFromBuild = true;
        modifier.applyToChildren = true;

        foreach(IPZPrefabVisualSection section in sections)
        {
            GameObject group = new GameObject(section.GroupName);
            group.transform.SetParent(root.transform, false);
            Undo.RegisterCreatedObjectUndo(group, "Gorsel Bolum Olustur");
            section.Build(factory, group.transform);
        }
        return root;
    }
}

public static class PZRealPrefabBuilder
{
    private const string RootName = "PZ_RealPrefabVisuals";
    private const string PrefabPath = "Assets/Barking_Dog/3D Free Modular Kit/Prefabs/Wall_Simple_01.prefab";
    [MenuItem("Tools/Protocol Zero/Gercek Prefab Ortamini Olustur")]
    public static void Build()
    {
        Debug.Log("[PZ TEST] Olusturucu calisti!");
        Scene scene = SceneManager.GetActiveScene();
        if (!CanEdit(scene))
            return;
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null)
        {
            Debug.LogError("[PROTOCOL ZERO] Wall_Simple_01.prefab bulunamadi. " + "Assets/Barking_Dog klasorunu kontrol et.");
            return;
        }

        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Protocol Zero Gercek Prefab Ortami");
        RestoreOriginalWalls();
        GameObject oldRoot = FindRoot(scene);
        if (oldRoot != null)
            Undo.DestroyObjectImmediate(oldRoot);
        IPZPrefabFactory factory = new PZPrefabFactory(prefab);
        IPZPrefabVisualSection[] sections =
        {
            new PZObstacleVisualSection(),
            new PZKeycardRoomVisualSection()
        };

        PZVisualBuilder builder = new PZVisualBuilder(factory, sections);
        GameObject root = builder.Build(RootName);
        Selection.activeGameObject = root;
        EditorSceneManager.MarkSceneDirty(scene);
        Undo.CollapseUndoOperations(undoGroup);
        Debug.Log("[PROTOCOL ZERO] Gercek prefab ortami olusturuldu." + "Olusturulan prefab sayisi: " + factory.CreatedCount);
    }
    [MenuItem("Tools/Protocol Zero/Gercek Prefab Ortamini Geri Al")]
    public static void Restore()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!CanEdit(scene))
            return;
        RestoreOriginalWalls();
        GameObject root = FindRoot(scene);
        if (root != null)
            Undo.DestroyObjectImmediate(root);
        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("[PROTOCOL ZERO] Orijinal duvar gorunumleri geri getirildi.");
    }
    private static void RestoreOriginalWalls()
    {
        foreach(Renderer renderer in PZOriginalWalls.FindAll())
        {
            if (renderer == null || renderer.enabled)
                continue;
            Undo.RecordObject(renderer, "Orijinal Gorunumu Geri Ac");
            renderer.enabled = true;
        }
    }
    private static GameObject FindRoot(Scene scene)
    {
        GameObject[] roots = scene.GetRootGameObjects();
        foreach(GameObject obj in roots)
        {
            if (obj.name == RootName)
                return obj;
        }
        return null;
    }
    private static bool CanEdit(Scene scene)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.Log("[PROTOCOL ZERO] Once Play modunu kapat.");
            return false;
        }
        if (scene.name != "MainScene")
        {
            Debug.LogWarning("[PROTOCOL ZERO] MainScene sahnesini ac.");
            return false;
        }
        return true;
    }
}

