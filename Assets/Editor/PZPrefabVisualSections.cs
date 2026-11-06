using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public interface IPZPrefabFactory
{
    int CreatedCount { get; }
    bool Cover(Renderer source, Transform parent);
}
public interface IPZPrefabVisualSection
{
    string GroupName { get; }
    void Build(IPZPrefabFactory factory, Transform parent);
}

public static class PZOriginalWalls
{
    public static IEnumerable<Renderer> FindObstacles()
    {
        string[] names = { "NewObstacle_L_1", "NewObstacle_L_2" };

        foreach(string name in names)
        {
            GameObject obj = GameObject.Find(name);
            if (obj == null)
                continue;
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
                yield return renderer;
        }
    }

    public static IEnumerable<Renderer> FindKeycardWalls()
    {
        GameObject room = GameObject.Find("KeycardRoom");
        if (room == null)
            yield break;

        foreach(Transform child in room.transform)
        {
            if (!child.name.StartsWith("KeycardWall_"))
                continue;
            Renderer renderer = child.GetComponent<Renderer>();
            if (renderer != null)
                yield return renderer;
        }
    }

    public static IEnumerable<Renderer> FindAll()
    {
        foreach (Renderer renderer in FindObstacles())
            yield return renderer;
        foreach (Renderer renderer in FindKeycardWalls())
            yield return renderer;
    }
}

public sealed class PZPrefabFactory : IPZPrefabFactory
{
    private readonly GameObject wallPrefab;
    public int CreatedCount { get; private set; }
    public PZPrefabFactory(GameObject prefab)
    {
        wallPrefab = prefab;
    }
    public bool Cover(Renderer source,Transform parent)
    {
        if (source == null || wallPrefab == null)
            return false;
        Bounds bounds = source.bounds;
        bool alongX = bounds.size.x >= bounds.size.z;
        float length = alongX
            ? bounds.size.x
            : bounds.size.z;

        int sectionCount = Mathf.Max(1, Mathf.CeilToInt(length / 3f));
        float sectionLength = length / sectionCount;
        int successfulPieces = 0;
        for (int i = 0; i < sectionCount; i++)
        {
            Vector3 sectionCenter = bounds.center;
            if (alongX)
            {
                sectionCenter.x = bounds.min.x + (i + 0.5f) * sectionLength;
            }
            else
            {
                sectionCenter.z=bounds.min.z + (i + 0.5f) * sectionLength;
            }
            for (int side = -1; side <= 1; side += 2)
            {
                if(PlacePiece(
                    source.gameObject.name,
                    parent,
                    bounds,
                    sectionCenter,
                    sectionLength,
                    alongX,
                    side,
                    i))
                {
                    successfulPieces++;
                }
            }
        }

        if (successfulPieces == 0)
            return false;
        Undo.RecordObject(source, "Orijinal Duvar Gorunumunu Gizle");
        source.enabled = false;
        return true;
    }
    private bool PlacePiece(
        string sourceName,
        Transform parent,
        Bounds sourceBounds,
        Vector3 sectionCenter,
        float sectionLength,
        bool alongX,
        int side,
        int index)
    {
        GameObject visual = PrefabUtility.InstantiatePrefab(wallPrefab, parent) as GameObject;
        if (visual == null)
            return false;
        Undo.RegisterCreatedObjectUndo(visual, "Gercek Duvar Prefabi Olustur");
        visual.name = sourceName + "_RealWall_" + index + "_" + side;
        visual.transform.rotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one;

        if(!TryGetBounds(visual,out Bounds initialBounds))
        {
            Undo.DestroyObjectImmediate(visual);
            return false;
        }
        bool prefabAlongX = initialBounds.size.x >= initialBounds.size.z;
        float rotationY = alongX == prefabAlongX
            ? 0f
            : 90f;
        if (side < 0)
            rotationY += 180f;
        visual.transform.rotation = Quaternion.Euler(0f, rotationY, 0f);
        if (!TryGetBounds(visual, out Bounds rotatedBounds))
        {
            Undo.DestroyObjectImmediate(visual);
            return false;
        }
        float prefabLength = alongX
            ? rotatedBounds.size.x
            : rotatedBounds.size.z;
        float widthScale = sectionLength / Mathf.Max(prefabLength, 0.001f);
        float heightScale = sourceBounds.size.y / Mathf.Max(rotatedBounds.size.y, 0.001f);
        Vector3 scale = Vector3.one;
        if (prefabAlongX)
            scale.x = widthScale;
        else
            scale.z = widthScale;
        scale.y = heightScale;
        visual.transform.localScale = scale;
        if (!TryGetBounds(visual, out Bounds fittedBounds))
        {
            Undo.DestroyObjectImmediate(visual);
            return false;
        }

        Vector3 normal = alongX
            ? Vector3.forward
            : Vector3.right;
        float originalHalfThickness = alongX
            ? fittedBounds.extents.z
            : fittedBounds.extents.x;
        float visualHalfThickness = alongX
            ? fittedBounds.extents.z
            : fittedBounds.extents.x;
        Vector3 desiredCenter = sectionCenter + normal * side * (originalHalfThickness + visualHalfThickness);
        visual.transform.position += desiredCenter - fittedBounds.center;
        Collider[] colliders = visual.GetComponentsInChildren<Collider>(true);
        foreach(Collider collider in colliders)
        {
            Undo.RecordObject(collider, "Dekor Collider Kapat");
            collider.enabled = false;
        }
        CreatedCount++;
        return true;
    }
    private static bool TryGetBounds(GameObject obj,out Bounds bounds)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            bounds = new Bounds();
            return false;
        }
        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);
        return true;
    }
}
public sealed class PZObstacleVisualSection : IPZPrefabVisualSection
{
    public string GroupName => "01_GercekEngelleri";
    public void Build(IPZPrefabFactory factory, Transform parent)
    {
        foreach (Renderer renderer in PZOriginalWalls.FindObstacles())
        {
            factory.Cover(renderer, parent);
        }
    }
}
public sealed class PZKeycardRoomVisualSection : IPZPrefabVisualSection
{
    public string GroupName => "02_GercekKeycardOdasi";
    public void Build(IPZPrefabFactory factory, Transform parent)
    {
        foreach (Renderer renderer in PZOriginalWalls.FindKeycardWalls())
        {
            factory.Cover(renderer, parent);
        }
    }
}