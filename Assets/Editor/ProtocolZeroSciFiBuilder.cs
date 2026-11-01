using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using System.Data.SqlTypes;

public interface IPZEnvironmentSection
{
    void Build(PZObjectFactory factory, Transform parent);
}

public sealed class PZMaterialFactory
{
    public Material DarkMetal { get; }
    public Material PanelMetal { get; }
    public Material Silver { get; }
    public Material CyanNeon { get; }
    public Material OrangeNeon { get; }

    public PZMaterialFactory()
    {
        const string folder = "Assets/Materials/PZ_Generated";
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/Materials", "PZ_Generated");
        DarkMetal = Create(folder, "DarkTitanium", new Color(0.07f, 0.10f, 0.15f), 0.7f, 0.4f);
        PanelMetal = Create(folder, "MetalPanels", new Color(0.20f, 0.26f, 0.33f), 0.6f, 0.5f);
        Silver = Create(folder, "SilverEdges", new Color(0.50f, 0.60f, 0.70f), 0.7f, 0.6f);
        CyanNeon = Create(folder, "CyanNeon", new Color(0.03f, 0.75f, 1f), 0f, 0.3f,3f);
        OrangeNeon = Create(folder, "OrangeWarning", new Color(1f, 0.32f, 0.03f), 0f, 0.3f,3f);
    }
    private static Material Create(
        string folder,
        string name,
        Color color,
        float metallic,
        float smoothness,
        float emission = 0f)
    {
        string path = folder + "/" + name + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");
        if (mat == null)
        {
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color"))
            mat.SetColor("_Color", color);
        if (mat.HasProperty("_Metallic"))
            mat.SetFloat("_Metallic", metallic);
        if (mat.HasProperty("_Smoothness"))
            mat.SetFloat("_Smoothness", smoothness);
        if (emission > 0f)
        {
            mat.EnableKeyword("_EMISSION");
            if (mat.HasProperty("_EmissionColor"))
                mat.SetColor("_EmissionColor", color * emission);
        }
        else
        {
            mat.DisableKeyword("_EMISSION");
        }
        EditorUtility.SetDirty(mat);
        return mat;
    }
}
public sealed class PZObjectFactory
{
    public PZMaterialFactory Materials { get; }
    public PZObjectFactory(PZMaterialFactory materials)
    {
        Materials = materials;
    }
    public Transform Group(Transform parent, string name)
    {
        GameObject group = new GameObject(name);
        group.transform.SetParent(parent, false);
        return group.transform;
    }

    public void Box(
        Transform parent,
        string name,
        Vector3 position,
        Vector3 scale,
        Material material)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.SetParent(parent, true);
        obj.transform.position = position;
        obj.transform.rotation = Quaternion.identity;
        obj.transform.localScale = scale;
        Collider collider = obj.GetComponent<Collider>();
        if (collider != null)
            Object.DestroyImmediate(collider);
        obj.GetComponent<Renderer>().sharedMaterial = material;
    }
    public void PointLight(
        Transform parent,
        string name,
        Vector3 position,
        Color color,
        float intensity,
        float range)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, true);
        obj.transform.position = position;
        Light light = obj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.shadows = LightShadows.None;
    }
}

public sealed class PZCorridorSection : IPZEnvironmentSection
{
    public void Build(PZObjectFactory f, Transform parent)
    {
        Transform group = f.Group(parent, "01_Koridor");
        var m = f.Materials;
        for (int side = -1; side <= 1; side += 2)
        {

            for (int i = 0; i < 6; i++)
            {
                float x = side * (3.5f + i * 3.6f);
                f.Box(group, "TitaniumPanel",
                    new Vector3(x, 1.25f, 6.66f),
                    new Vector3(3.3f, 2.3f, 0.08f),
                    m.DarkMetal);
                f.Box(group, "UpperTrim",
                    new Vector3(x, 2.43f, 6.57f),
                    new Vector3(3.4f, 0.1f, 0.14f),
                    m.Silver);
                f.Box(group, "NeonStrip",
                    new Vector3(x, 2.25f, 6.54f),
                    new Vector3(3f, 0.055f, 0.08f),
                    m.CyanNeon);
                f.Box(group, "MetalSupport",
                    new Vector3(x-1.55f, 1.25f, 6.52f),
                    new Vector3(0.14f, 2.45f, 0.15f),
                    m.PanelMetal);

                if (i % 2 == 0)
                {
                    f.Box(group, "WarningLight",
                    new Vector3(x, 1.4f, 6.50f),
                    new Vector3(0.12f, 0.5f, 0.07f),
                    m.OrangeNeon);
                }
            }
        }

        float[] tavanKonumlari = { -9f, 0f, 15f };
        for (int i = 0; i < tavanKonumlari.Length; i++)
        {
            float z = tavanKonumlari[i];
            f.Box(group, "CeilingBeam",
                    new Vector3(0f, 3.5f, z),
                    new Vector3(15f, 0.22f, 0.5f),
                    m.DarkMetal);
            f.Box(group, "CeilingNeon",
                   new Vector3(0f, 3.36f, z),
                   new Vector3(13f, 0.05f, 0.18f),
                   m.CyanNeon);
        }
        f.PointLight(group, "CorridorBlueLight",
                   new Vector3(0f, 3f, -4f),
                   new Color(0.2f, 0.65f, 1f),
                   2f, 9f);
    }
}

public sealed class PZGateSection : IPZEnvironmentSection
{
    public void Build(PZObjectFactory f,Transform parent)
    {
        Transform group = f.Group(parent, "02_FirstGate");
        var m = f.Materials;
        for(int side = -1; side <= 1; side += 2)
        {
            float x = side * 1.7f;
            f.Box(group, "GateFrame",
                    new Vector3(x,1.4f, 6.45f),
                    new Vector3(0.25f, 2.8f, 0.3f),
                    m.Silver);
            f.Box(group, "GateNeon",
                    new Vector3(x-side*0.15f, 1.4f, 6.24f),
                    new Vector3(0.055f, 2.5f, 0.07f),
                    m.CyanNeon);
        }
        f.Box(group, "UpperFrame",
                    new Vector3(0f, 2.85f, 6.45f),
                    new Vector3(3.8f, 0.3f, 0.35f),
                    m.DarkMetal);
        f.Box(group, "UpperNeon",
                    new Vector3(0f, 2.65f, 6.22f),
                    new Vector3(3.3f, 0.06f, 0.07f),
                    m.CyanNeon);
        f.Box(group, "SecurityDisplay",
                    new Vector3(0f, 3.15f, 6.4f),
                    new Vector3(2.2f, 0.2f, 0.12f),
                    m.OrangeNeon);
        f.PointLight(group, "GateBlueLight",
                    new Vector3(0f, 2.7f, 6.1f),
                    new Color(0.1f, 0.8f, 1f),
                    3f,7f);
    }
}

public sealed class PZMissionSection : IPZEnvironmentSection
{
    public void Build(PZObjectFactory f, Transform parent)
    {
        Transform group = f.Group(parent, "03_GorevAlanlari");
        var m = f.Materials;
        float[] spawnKonumlari = { -6f, 0f, 6f };
        for (int i = 0; i < spawnKonumlari.Length; i++)
        {
            float x = spawnKonumlari[i];
            f.Box(group, "SpawnPlatform",
                    new Vector3(x, 0.04f, -12f),
                    new Vector3(2.4f, 0.025f, 2f),
                    m.PanelMetal);
            f.Box(group, "SpawnNeon",
                    new Vector3(x, 0.06f, -13f),
                    new Vector3(2.4f, 0.035f, 0.08f),
                    m.CyanNeon);
        }

        f.Box(group, "KeycardSecurityPanel",
                    new Vector3(0.8f, 2.9f, 11.5f),
                    new Vector3(2.7f, 0.3f, 0.2f),
                    m.DarkMetal);
        f.Box(group, "KeycardWarning",
                    new Vector3(0.8f, 2.7f, 11.35f),
                    new Vector3(2.4f, 0.07f, 0.07f),
                    m.OrangeNeon);
        f.Box(group, "PowerHousing",
                   new Vector3(10f, 2.45f, 19f),
                   new Vector3(2.5f, 0.3f, 0.5f),
                   m.DarkMetal);
        f.Box(group, "PowerNeon",
                   new Vector3(10f, 2.45f, 18.7f),
                   new Vector3(1.5f, 0.08f, 0.07f),
                   m.CyanNeon);
        f.PointLight(group, "KeycardWarningLight",
                    new Vector3(0f, 2.8f, 12f),
                    new Color(1f, 0.3f, 0.05f),
                    2f, 6f);
        f.PointLight(group, "PowerEnergyLight",
                    new Vector3(10f, 2.7f, 19f),
                    new Color(0.1f, 0.8f, 1f),
                    3f, 6f);
        f.PointLight(group, "SpawnBlueLight",
                    new Vector3(0f, 3f, -12f),
                    new Color(0.2f, 0.7f, 1f),
                    2f, 8f);
    }
}

public sealed class PZInteractionDecorSection:IPZEnvironmentSection
{
    public void Build(PZObjectFactory f, Transform parent)
    {
        Transform root = f.Group(parent, "04_EtkilesimNoktalari");
        BuildGateControl(f, root);
        BuildKeycardAccess(f, root);
        BuildPowerStation(f, root);
    }
        private void BuildGateControl(PZObjectFactory f, Transform root)
        {
            GameObject control = GameObject.Find("ControlPanel");
            if (control == null)
            {
                Debug.LogWarning("[PZ] ControlPanel bulunamadı.");
                return;
            }
        Transform group = f.Group(root, "Kapi_Kontrol_Terminali");
        Vector3 p = control.transform.position;
        Vector3 center = new Vector3(p.x, p.y + 0.55f, p.z - 0.35f);
        CreateTechFrame(f, group, center, 1.15f, 1.35f, f.Materials.CyanNeon);
        f.Box(group, "Terminal_StatusLight",
                  center + new Vector3(0f, 0.82f, -0.05f),
                  new Vector3(0.55f, 0.10f, 0.07f),
                  f.Materials.CyanNeon);

        for(int i = 0; i < 4; i++)
        {
            f.Box(group, "Terminal_DataIndicator",
                  center + new Vector3(0.72f, 0.4f- i*0.19f, -0.04f),
                  new Vector3(0.16f, 0.06f, 0.05f),
                  f.Materials.CyanNeon);
        }
    }

    private void BuildKeycardAccess(PZObjectFactory f,Transform root)
    {
        GameObject room = GameObject.Find("KeycardRoom");
        if (room == null)
        {
            Debug.LogWarning("[PZ] KeycardRoom bulunamadı.");
            return;
        }
        Transform group = f.Group(root, "Keycard_Guvenlik_Bolgesi");
        Vector3 center = room.transform.TransformPoint(new Vector3(1.5f, 1.5f, 8.6f));
        CreateTechFrame(f, group, center, 1.4f, 1.7f, f.Materials.OrangeNeon);
        f.Box(group, "Restricted_AccessHeader",
              center + new Vector3(0f, 1.12f, 0f),
              new Vector3(1.8f, 0.22f, 0.2f),
              f.Materials.DarkMetal);
        f.Box(group, "Restricted_WarningLight",
              center + new Vector3(0f, 1.12f, -0.13f),
              new Vector3(1.13f, 0.065f, 0.07f),
              f.Materials.OrangeNeon);

        for(int i = 0; i < 5; i++)
        {
            f.Box(group, "Keycard_ScannerIndicator",
              center + new Vector3(0.92f, 0.48f-i*0.22f, -0.08f),
              new Vector3(0.12f, 0.08f, 0.07f),
              f.Materials.OrangeNeon);
        }
    }
    private void BuildPowerStation(PZObjectFactory f,Transform root)
    {
        GameObject power = GameObject.Find("PowerPanel");
        if (power == null)
        {
            Debug.LogWarning("[PZ] PowerPanel bulunamadı.");
            return;
        }
        Transform group = f.Group(root, "Enerji_Kontrol_Istasyonu");
        Vector3 p = power.transform.position;
        Vector3 center=new Vector3(p.x, p.y+0.1f, p.z-0.35f);
        CreateTechFrame(f, group, center, 1.75f, 2.05f, f.Materials.CyanNeon);
        for (int side = -1; side <= 1; side += 2)
        {
            f.Box(group, "Power_EnergyConduit",
             center + new Vector3(side*1.08f, 0f, 0.12f),
             new Vector3(0.18f, 2.1f, 0.3f),
             f.Materials.PanelMetal);

            f.Box(group, "Power_EnergyLine",
             center + new Vector3(side*1.08f, 0f, -0.06f),
             new Vector3(0.065f, 1.75f, 0.06f),
             f.Materials.CyanNeon);
        }
        for(int i = 0; i < 5; i++)
        {
            f.Box(group, "Power_EnergyIndicator",
             center + new Vector3(1.3f, 0.65f-i*0.3f, -0.05f),
             new Vector3(0.2f, 0.07f, 0.07f),
             f.Materials.CyanNeon);
        }
    }
    private void CreateTechFrame(
        PZObjectFactory f,
        Transform parent,
        Vector3 center,
        float width,
        float height,
        Material neonMaterial)
    {
        float halfWidth = width / 2f;
        float halfHeight = height / 2f;
        for (int side = -1; side <= 1; side += 2)
        {
            f.Box(parent, "Metal_HorizontalFrame",
             center + new Vector3(0f, side*halfHeight, 0f),
             new Vector3(width+0.15f, 0.12f, 0.15f),
             f.Materials.Silver);

            f.Box(parent, "Metal_VerticalFrame",
             center + new Vector3(side * halfWidth, 0f,0f),
             new Vector3(0.12f, height, 0.15f),
             f.Materials.Silver);

            f.Box(parent, "Neon_VerticalLine",
             center + new Vector3(side * halfWidth, 0f, -0.10f),
             new Vector3(0.045f, height-0.12f, 0.045f),
             neonMaterial);
        }
    }
}

public static class ProtocolZeroSciFiBuilder
{
    private const string RootName = "PZ_SciFi_Environment";
    [MenuItem("Tools/Protocol Zero/Bilim Kurgu Ortamini Olustur")]
    public static void Build()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("Önce Play modunu kapat.");
            return;
        }
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name != "MainScene")
        {
            Debug.LogWarning("Bu araç yalnızca MainScene için hazırlanmıştır.");
            return;
        }
        GameObject[] rootObjects = scene.GetRootGameObjects();
        for(int i=0; i < rootObjects.Length; i++)
        {
            if (rootObjects[i].name == RootName)
            {
                Undo.DestroyObjectImmediate(rootObjects[i]);
                break;
            }
        }

        GameObject root = new GameObject(RootName);
        Undo.RegisterCreatedObjectUndo(root, "Bilim Kurgu Ortami");
        NavMeshModifier modifier = root.AddComponent<NavMeshModifier>();
        modifier.ignoreFromBuild = true;
        modifier.applyToChildren = true;
        PZMaterialFactory materials = new PZMaterialFactory();
        PZObjectFactory factory = new PZObjectFactory(materials);
        IPZEnvironmentSection[] sections =
        {
            new PZCorridorSection(),
            new PZGateSection(),
            new PZMissionSection(),
            new PZInteractionDecorSection(),
            new PZObstacleAndRoomSection()
        };
        for(int i = 0; i < sections.Length; i++)
        {
            sections[i].Build(factory, root.transform);
        }
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);
        Selection.activeGameObject = root;
        Debug.Log("[PROTOCOL ZERO] Bilim kurgu ortamı oluşturuldu");
    }
}