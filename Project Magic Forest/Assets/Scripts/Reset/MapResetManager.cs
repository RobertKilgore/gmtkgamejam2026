using System.Collections.Generic;
using UnityEngine;

public class MapResetManager : MonoBehaviour
{
    [Header("Cabin")]
    [SerializeField] private GameObject cabinObject;
    [SerializeField] private Transform playerCharacter;

    [Header("POI Placements")]
    [SerializeField] private Transform[] poiPlaceMats;

    private Transform selectedPlaceMat;
    private Transform originalPlayerParent;

    public void ResetMap()
    {
        ResetDirtyTrees();
        DestroyAllPoiObjects();
        DisableAllPoiSpawners();
        SelectRandomPoiPlaceMat();
        ParentPlayerToCabin();
        MoveCabinToSelectedPlace();
        EnableAllPoiSpawnersExceptSelected();
        UnparentPlayerFromCabin();
    }

    private void ResetDirtyTrees()
    {
        var dirtyTrees = new List<TreeInteractable>(TreeInteractable.DirtyTrees);
        foreach (var tree in dirtyTrees)
        {
            if (tree != null)
            {
                tree.ResetState();
            }
        }
    }

    private void DestroyAllPoiObjects()
    {
        var poiObjects = GameObject.FindGameObjectsWithTag("POI");
        foreach (var poiObject in poiObjects)
        {
            if (poiObject != null)
            {
                Destroy(poiObject);
            }
        }
    }

    private void DisableAllPoiSpawners()
    {
        var spawners = FindObjectsByType<PoiSpawner>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var spawner in spawners)
        {
            if (spawner != null)
            {
                spawner.SetSpawningEnabled(false);
            }
        }
    }

    private void SelectRandomPoiPlaceMat()
    {
        var placeMats = GetPoiPlaceMats();
        if (placeMats == null || placeMats.Length == 0)
        {
            selectedPlaceMat = null;
            return;
        }

        selectedPlaceMat = placeMats[Random.Range(0, placeMats.Length)];
    }

    private void ParentPlayerToCabin()
    {
        if (playerCharacter == null)
        {
            playerCharacter = FindPlayerCharacter();
        }

        if (playerCharacter == null || cabinObject == null)
        {
            return;
        }

        originalPlayerParent = playerCharacter.parent;
        playerCharacter.SetParent(cabinObject.transform, true);
    }

    private void MoveCabinToSelectedPlace()
    {
        if (cabinObject == null || selectedPlaceMat == null)
        {
            return;
        }

        cabinObject.transform.position = selectedPlaceMat.position;
        cabinObject.transform.rotation = selectedPlaceMat.rotation;
    }

    private void EnableAllPoiSpawnersExceptSelected()
    {
        var spawners = FindObjectsByType<PoiSpawner>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var spawner in spawners)
        {
            if (spawner == null)
            {
                continue;
            }

            if (spawner.transform == selectedPlaceMat || spawner.gameObject == cabinObject || spawner.gameObject == playerCharacter?.gameObject)
            {
                continue;
            }

            spawner.SetSpawningEnabled(true);
        }

        if (selectedPlaceMat != null)
        {
            var selectedSpawner = selectedPlaceMat.GetComponent<PoiSpawner>() ?? selectedPlaceMat.GetComponentInChildren<PoiSpawner>(true);
            if (selectedSpawner != null)
            {
                selectedSpawner.SetSpawningEnabled(false);
            }
        }
    }

    private void UnparentPlayerFromCabin()
    {
        if (playerCharacter == null)
        {
            playerCharacter = FindPlayerCharacter();
        }

        if (playerCharacter == null)
        {
            return;
        }

        if (originalPlayerParent != null)
        {
            playerCharacter.SetParent(originalPlayerParent, true);
        }
        else
        {
            playerCharacter.SetParent(null, true);
        }
    }

    private Transform FindPlayerCharacter()
    {
        var playerObject = GameObject.FindGameObjectWithTag("Player");
        return playerObject != null ? playerObject.transform : null;
    }

    private Transform[] GetPoiPlaceMats()
    {
        if (poiPlaceMats != null && poiPlaceMats.Length > 0)
        {
            return poiPlaceMats;
        }

        var discoveredSpawners = FindObjectsByType<PoiSpawner>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        var discoveredPlaceMats = new List<Transform>();
        foreach (var spawner in discoveredSpawners)
        {
            if (spawner != null)
            {
                discoveredPlaceMats.Add(spawner.transform);
            }
        }

        return discoveredPlaceMats.ToArray();
    }
}
