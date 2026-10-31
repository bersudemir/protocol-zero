using UnityEngine;

public sealed class PZObstacleAndRoomSection : IPZEnvironmentSection
{
    public void Build(PZObjectFactory f,Transform parent)
    {
        Transform root = f.Group(parent, "05_EngellerVeKeycardOdasi");
        BuildObstacles(f, root);
        BuildKeycardRoom(f, root);
    }

    private void BuildObstacles(PZObjectFactory f,Transform parent)
    {
        Transform group = f.Group(parent, "Teknolojik_Engeller");
        string[] obstacleNames =
        {
            "NewObstacle_L_1",
            "NewObstacle_L_2",
            "TestBarrier_Right",
            "TestBarrier_Center"
        };
        for(int i = 0; i < obstacleNames.Length; i++)
        {
            GameObject obstacle = GameObject.Find(obstacleNames[i]);
            if (obstacle == null)
                continue;
            Renderer renderer = obstacle.GetComponent<Renderer>();
            if (renderer == null)
                continue;
            Transform obstacleGroup = f.Group(group, obstacle.name + "_Visuals");
            CreateMetalCover(f, obstacleGroup, renderer.bounds, false);
        }
    }
    public void BuildKeycardRoom(PZObjectFactory f, Transform parent)
    {
        GameObject room = GameObject.Find("KeycardRoom");
        if (room == null)
        {
            Debug.LogWarning("[PZ] KeycardRoom bulunamadı.");
            return;
        }
        Transform group = f.Group(parent, "Keycard_Teknolojik_Oda");
        Transform roomTransform = room.transform;

        foreach(Transform child in roomTransform)
        {
            if (!child.name.StartsWith("KeycardWall_"))
                continue;
            Renderer renderer = child.GetComponent<Renderer>();
            if (renderer == null)
                continue;
            Transform wallGroup = f.Group(group, child.name + "_Visuals");
            CreateMetalCover(f, wallGroup, renderer.bounds, true);
        }

        Vector3 screenPosition = roomTransform.TransformPoint(new Vector3(0f, 1.5f, 11.27f));
        f.Box(group, "Security_Monitor_Frame",
                    screenPosition,
                    new Vector3(2.5f, 1.1f, 0.12f),
                    f.Materials.Silver);
        f.Box(group, "Security_Monitor",
                    screenPosition+new Vector3(0f, 0f, -0.075f),
                    new Vector3(2.2f, 0.8f, 0.055f),
                    f.Materials.PanelMetal);
        for (int i = 0; i < 4; i++)
        {
        f.Box(group, "Monitor_DataLine",
                    screenPosition + new Vector3(-0.25f, 0.25f-i*0.16f, -0.11f),
                    new Vector3(1.2f-i*0.15f, 0.035f, 0.025f),
                    f.Materials.CyanNeon);
    }
        Vector3 cardPosition = roomTransform.TransformPoint(new Vector3(0f, 0.12f, 10f));
        f.Box(group, "Keycard_Pedestal",
                     cardPosition,
                     new Vector3(1.4f, 0.24f, 1f),
                     f.Materials.DarkMetal);
        f.Box(group, "Keycard_Pedestal_Light",
                     cardPosition + new Vector3(0f, 0.14f, -0.45f),
                     new Vector3(1.2f, 0.045f, 0.055f),
                     f.Materials.CyanNeon);

        float[] beamPositions = { 9.2f, 10.6f, 11.8f };
        for (int i = 0; i < beamPositions.Length; i++)
        {
            Vector3 p = roomTransform.TransformPoint(new Vector3(0f, 2.8f, beamPositions[i]));
            f.Box(group, "Ceiling_TitaniumBeam",
                     p,
                     new Vector3(6.8f, 0.18f, 0.28f),
                     f.Materials.DarkMetal);
            f.Box(group, "Ceiling_BlueLight",
                     p + new Vector3(0f, -0.12f, 0f),
                     new Vector3(5.8f, 0.045f, 0.12f),
                     f.Materials.CyanNeon);
        }

        for (int side = -1; side <= 1; side += 2)
        {
            Vector3 entryPosition = roomTransform.TransformPoint(new Vector3(side * 0.85f, 1.3f, 8.78f));
            f.Box(group, "Restricted_AccessIndicator",
                    entryPosition,
                     new Vector3(0.08f, 1.6f, 0.06f),
                     f.Materials.OrangeNeon);
        }

        f.PointLight(group, "KeycardRoom_BlueLight",
                   roomTransform.TransformPoint(new Vector3(0f, 2.6f, 10f)),
                   new Color(0.15f, 0.7f, 1f),
                   2.5f, 5f);
    }

    private void CreateMetalCover(
        PZObjectFactory f,
        Transform parent,
        Bounds bounds,
        bool restricted)
    {
        var m = f.Materials;
        Vector3 center = bounds.center;
        Vector3 size = bounds.size;
        float height = Mathf.Max(0.1f, size.y - 0.08f);
        float widthX = Mathf.Max(0.1f, size.x - 0.05f);
        float widthZ = Mathf.Max(0.1f, size.z - 0.05f);

        Material indicator = restricted
            ? m.OrangeNeon
            : m.CyanNeon;
        for (int side = -1; side <= 1; side += 2)
        {
            float faceZ = center.z + side * (bounds.extents.z + 0.035f);
            f.Box(parent, "Titanium_WallSurface",
                    new Vector3(center.x, center.y, faceZ),
                    new Vector3(widthX, height, 0.055f),
                    m.DarkMetal);

            if (size.x > 0.8f)
            {
                f.Box(parent, "Silver_UpperTrim",
                   new Vector3(center.x, bounds.max.y-0.13f, faceZ),
                   new Vector3(widthX, 0.09f, 0.075f),
                   m.Silver);

                f.Box(parent, "Neon_HorizontalLine",
                   new Vector3(center.x, bounds.max.y - 0.32f, faceZ),
                   new Vector3(widthX-0.2f, 0.045f, 0.075f),
                   indicator);

                int sections = Mathf.Max(2, Mathf.CeilToInt(size.x / 1.5f));

                for(int i = 1; i < sections; i++)
                {
                    float x = bounds.min.x + size.x * i / sections;

                    f.Box(parent, "Vertical_MetalSupport",
                        new Vector3(x, center.y, faceZ),
                        new Vector3(0.065f, height, 0.075f),
                        m.PanelMetal);
                }
            }

            float faceX = center.x + side * (bounds.extents.x + 0.035f);

            f.Box(parent, "Titanium_SideSurface",
                   new Vector3(faceX, center.y, center.z),
                   new Vector3(0.055f, height, widthZ),
                   m.DarkMetal);

            if (size.z > 0.8f)
            {
                f.Box(parent, "Side_NeonLine",
                   new Vector3(faceX, bounds.max.y - 0.3f, center.z),
                   new Vector3(0.075f, 0.045f, widthZ-0.2f),
                   indicator);
            }
        }

        f.Box(parent, "Titanium_TopCover",
                   new Vector3(center.x, bounds.max.y + 0.03f, center.z),
                   new Vector3(widthX, 0.055f, widthZ),
                   m.PanelMetal);
    }
}
