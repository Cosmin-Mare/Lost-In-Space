using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public class OreAndPlanetTests
{
    [Test]
    public void IronOrePrefabIsACollectibleResource()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Ores/IronOre.prefab");
        Assert.IsNotNull(prefab, "IronOre prefab should exist");

        Collectible collectible = prefab.GetComponent<Collectible>();
        Assert.IsNotNull(collectible, "IronOre must have Collectible so click-to-mine works");

        SerializedObject so = new SerializedObject(collectible);
        Assert.IsNotNull(so.FindProperty("resourceData").objectReferenceValue, "IronOre Collectible needs resource data");
    }

    [Test]
    public void LithiumOrePrefabIsACollectibleResource()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Ores/LithiumOre.prefab");
        Assert.IsNotNull(prefab, "LithiumOre prefab should exist");

        Collectible collectible = prefab.GetComponent<Collectible>();
        Assert.IsNotNull(collectible, "LithiumOre must have Collectible so click-to-mine works");

        SerializedObject so = new SerializedObject(collectible);
        Assert.IsNotNull(so.FindProperty("resourceData").objectReferenceValue, "LithiumOre Collectible needs resource data");
    }

    [Test]
    public void FindFromHitResolvesCollectibleOnOreAndParents()
    {
        var parent = new GameObject("OreWrapper");
        var ore = new GameObject("IronOre");
        ore.transform.SetParent(parent.transform);

        Collectible onParent = parent.AddComponent<Collectible>();
        Assert.AreEqual(onParent, Collectible.FindFromHit(ore.transform));

        Object.DestroyImmediate(parent);
    }

    [Test]
    public void FindFromHitResolvesCollectibleOnTheOreItself()
    {
        var ore = new GameObject("LithiumOre");
        Collectible onOre = ore.AddComponent<Collectible>();
        Assert.AreEqual(onOre, Collectible.FindFromHit(ore.transform));
        Object.DestroyImmediate(ore);
    }

    [Test]
    public void PlanetAlignStaysStableWhenRightWouldBeParallelToUp()
    {
        Vector3 edgePosition = new Vector3(80f, 20f, 0f);
        Vector3 up = PlanetMotor.SurfaceUp(edgePosition, Vector3.zero);
        Assert.Greater(up.sqrMagnitude, 0.9f);

        Quaternion start = Quaternion.LookRotation(Vector3.forward, Vector3.up);
        Quaternion aligned = PlanetMotor.AlignToUp(start, up);
        Assert.False(float.IsNaN(aligned.x) || float.IsNaN(aligned.y) || float.IsNaN(aligned.z) || float.IsNaN(aligned.w));

        Vector3 alignedUp = aligned * Vector3.up;
        Assert.Greater(Vector3.Dot(alignedUp.normalized, up), 0.99f);
    }

    [Test]
    public void TangentMoveOnSphereEdgeDoesNotIncludeRadialComponent()
    {
        Vector3 up = PlanetMotor.SurfaceUp(new Vector3(60f, 60f, 0f), Vector3.zero);
        Vector3 move = PlanetMotor.TangentMove(Vector3.forward, up, 2f);
        Assert.Less(Mathf.Abs(Vector3.Dot(move, up)), 0.001f);
    }

    [UnityTest]
    public IEnumerator OreHitIsCollectibleAfterEnteringPlayMode()
    {
        yield return new EnterPlayMode();

        var ore = new GameObject("IronOre");
        Collectible collectible = ore.AddComponent<Collectible>();
        BoxCollider collider = ore.AddComponent<BoxCollider>();
        collider.size = Vector3.one;

        yield return null;

        Collectible found = Collectible.FindFromHit(collider.transform);
        Assert.AreEqual(collectible, found);

        Object.Destroy(ore);
        yield return new ExitPlayMode();
    }

    [UnityTest]
    public IEnumerator PlanetEdgeOrientationDoesNotNaNInPlayMode()
    {
        yield return new EnterPlayMode();

        Quaternion rotation = Quaternion.identity;
        Vector3 planetCenter = Vector3.zero;
        Vector3 position = new Vector3(6f, 85f, 0f);

        for (int i = 0; i < 30; i++)
        {
            position = Quaternion.AngleAxis(2f, Vector3.forward) * position;
            Vector3 up = PlanetMotor.SurfaceUp(position, planetCenter);
            rotation = PlanetMotor.AlignToUp(rotation, up);
            Assert.False(float.IsNaN(rotation.w), "Planet alignment produced NaN at edge step " + i);
            yield return null;
        }

        yield return new ExitPlayMode();
    }
}
