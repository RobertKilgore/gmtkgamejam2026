using System.Collections.Generic;
using UnityEngine;

public class MapResetManager : MonoBehaviour
{
    [Header("Cabin Placement")]
    [SerializeField] private GameObject cabinPrefab;
    [SerializeField] private Transform[] poiPlaceMats;
    [SerializeField] private bool disableSelectedMatSpawner = true;

    [Header("POI Reset")]
    [SerializeField] private bool destroyAllPoiBehaviours = true;
    [SerializeField] private Transform poiRoot;

    public void ResetMap()
    {
        ResetTrees();
        ClearSpawnedPois();
        ResetSpawners();
        SelectRandomPoiPlaceMat();
    }

    [ContextMenu("Reset Map")]
    private void ResetMapContextMenu()
    {
        ResetMap();
    }

    private void ResetTrees()
    {
        var trees = FindObjectsOfType<TreeInteractable>(true);
        foreach (var tree in trees)
        {
            tree.ResetState();
        }

        Debug.Log($"[MapResetManager] Reset {trees.Length} trees.");
    }

    private void ClearSpawnedPois()
    {
        if (!destroyAllPoiBehaviours)
        {
            return;
        }

        IEnumerable<PoiBehaviour> pois = poiRoot != null
            ? poiRoot.GetComponentsInChildren<PoiBehaviour>(true)
            : FindObjectsOfType<PoiBehaviour>(true);

        int count = 0;
        foreach (var poi in pois)
        {
            if (poi != null)
            {
                DestroyImmediate(poi.gameObject);
                count++;
            }
        }

        Debug.Log($"[MapResetManager] Removed {count} spawned POI prefab(s).");
    }

    private void ResetSpawners()
    {
        var spawners = FindObjectsOfType<PoiSpawner>(true);
        foreach (var spawner in spawners)
        {
            spawner.ResetSpawner();
        }

        Debug.Log($"[MapResetManager] Reset {spawners.Length} POI spawner(s).");
    }

    private void SelectRandomPoiPlaceMat()
    {
        if (poiPlaceMats == null || poiPlaceMats.Length == 0)
        {
            Debug.LogWarning("[MapResetManager] No POI place mats assigned.");
            return;
        }

        Transform selectedMat = poiPlaceMats[Random.Range(0, poiPlaceMats.Length)];
        if (selectedMat == null)
        {
            Debug.LogWarning("[MapResetManager] Selected POI place mat is null.");
            return;
        }

        if (disableSelectedMatSpawner)
        {
            var selectedSpawner = selectedMat.GetComponent<PoiSpawner>() ?? selectedMat.GetComponentInChildren<PoiSpawner>(true);
            if (selectedSpawner != null)
            {
                selectedSpawner.SetSpawningEnabled(false);
                Debug.Log($"[MapResetManager] Disabled spawning for selected place mat '{selectedMat.name}'.");
            }
            else
            {
                Debug.LogWarning($"[MapResetManager] Selected place mat '{selectedMat.name}' has no PoiSpawner.");
            }
        }

        if (cabinPrefab != null)
        {
            cabinPrefab.transform.position = selectedMat.position;
            cabinPrefab.transform.rotation = selectedMat.rotation;
            Debug.Log($"[MapResetManager] Moved cabin prefab to '{selectedMat.name}'.");
        }
        else
        {
            Debug.LogWarning("[MapResetManager] Cabin prefab is not assigned.");
        }
    }
}
