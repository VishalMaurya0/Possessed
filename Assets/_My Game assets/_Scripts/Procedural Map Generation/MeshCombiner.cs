using UnityEngine;
using System.Collections.Generic;

public static class MeshCombiner
{
    private const float CHUNK_SIZE = 30f;

    public static void CombineContainerChunks(GameObject containerRoot, LayerMask layerMask)
    {
        if (containerRoot == null) return;

        // 1. CLEANUP: Destroy old chunks
        var allTransforms = containerRoot.GetComponentsInChildren<Transform>(true);
        foreach (var t in allTransforms)
        {
            if (t == null || t == containerRoot.transform) continue;
            // Delete old Chunks or old Combined objects
            if (t.name.StartsWith("Chunk_") || t.name.StartsWith("Mat_"))
            {
                if (Application.isEditor) Object.DestroyImmediate(t.gameObject);
                else Object.Destroy(t.gameObject);
            }
        }

        // 2. GATHER: Find all potential meshes
        MeshFilter[] filters = containerRoot.GetComponentsInChildren<MeshFilter>(true); // 'true' = include inactive
        List<MeshFilter> validFilters = new List<MeshFilter>();

        int foundCount = 0;

        foreach (var filter in filters)
        {
            // Skip if it's part of an old chunk (just in case)
            if (filter.transform.parent != null && filter.transform.parent.name.StartsWith("Chunk_")) continue;

            MeshRenderer rend = filter.GetComponent<MeshRenderer>();
            if (rend == null) continue;

            // --- CRITICAL FIX ---
            // Re-enable the renderer so we can read it (fixes the "Empty Chunk" bug on re-runs)
            rend.enabled = true;
            // --------------------

            if (filter.sharedMesh == null) continue;

            validFilters.Add(filter);
            foundCount++;
        }

        Debug.Log($"[MeshCombiner] Found {foundCount} valid meshes in {containerRoot.name}. Processing...");

        // 3. GROUP BY GRID POSITION
        Dictionary<Vector3Int, List<MeshFilter>> chunkGroups = new Dictionary<Vector3Int, List<MeshFilter>>();

        foreach (var filter in validFilters)
        {
            Vector3 pos = filter.transform.position;
            Vector3Int coord = new Vector3Int(
                Mathf.FloorToInt(pos.x / CHUNK_SIZE),
                Mathf.FloorToInt(pos.y / CHUNK_SIZE),
                Mathf.FloorToInt(pos.z / CHUNK_SIZE)
            );

            if (!chunkGroups.ContainsKey(coord))
                chunkGroups.Add(coord, new List<MeshFilter>());

            chunkGroups[coord].Add(filter);
        }

        // 4. GENERATE MESHES
        int chunksCreated = 0;
        foreach (var chunkEntry in chunkGroups)
        {
            CreateChunk(containerRoot, chunkEntry.Key, chunkEntry.Value, layerMask);
            chunksCreated++;
        }

        Debug.Log($"[MeshCombiner] Finished. Created {chunksCreated} chunks.");
    }
    private static void CreateChunk(GameObject root, Vector3Int coord, List<MeshFilter> filters, LayerMask layerMask)
    {
        // --- NEW: Convert LayerMask to Layer Index ---
        // Unity's GameObject.layer expects an int (0-31), not the bitmask value.
        // We find the index of the first bit set to 1 in the mask.
        int layerIndex = 0; // Default to "Default" layer
        int maskValue = layerMask.value;

        if (maskValue > 0)
        {
            for (int i = 0; i < 32; i++)
            {
                if ((maskValue & (1 << i)) != 0)
                {
                    layerIndex = i;
                    break;
                }
            }
        }
        // ---------------------------------------------

        GameObject chunkObj = new GameObject($"Chunk_{coord.x}_{coord.y}_{coord.z}");
        chunkObj.transform.SetParent(root.transform);
        chunkObj.transform.localPosition = Vector3.zero;
        chunkObj.transform.localRotation = Quaternion.identity;
        chunkObj.transform.localScale = Vector3.one;

        // Assign Layer to Parent
        chunkObj.layer = layerIndex;

        // Sort meshes by Material
        Dictionary<Material, List<CombineInstance>> matGroups = new Dictionary<Material, List<CombineInstance>>();

        foreach (var filter in filters)
        {
            MeshRenderer rend = filter.GetComponent<MeshRenderer>();
            Mesh mesh = filter.sharedMesh;

            // Transform matrix: Mesh Local -> World -> Chunk Local
            Matrix4x4 finalMatrix = chunkObj.transform.worldToLocalMatrix * filter.transform.localToWorldMatrix;

            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                if (i >= rend.sharedMaterials.Length) break;
                Material mat = rend.sharedMaterials[i];
                if (mat == null) continue;

                if (!matGroups.ContainsKey(mat))
                    matGroups.Add(mat, new List<CombineInstance>());

                CombineInstance ci = new CombineInstance();
                ci.mesh = mesh;
                ci.subMeshIndex = i;
                ci.transform = finalMatrix;

                matGroups[mat].Add(ci);
            }

            // HIDE ORIGINAL after processing
            rend.enabled = false;
        }

        // Create the combined mesh object for each material
        foreach (var entry in matGroups)
        {
            Material mat = entry.Key;
            List<CombineInstance> combines = entry.Value;

            GameObject meshObj = new GameObject($"Mat_{mat.name}");
            meshObj.transform.SetParent(chunkObj.transform);
            meshObj.transform.localPosition = Vector3.zero;
            meshObj.transform.localRotation = Quaternion.identity;
            meshObj.transform.localScale = Vector3.one;

            // Assign Layer to Child Mesh Object
            meshObj.layer = layerIndex;

            MeshFilter mf = meshObj.AddComponent<MeshFilter>();
            MeshRenderer mr = meshObj.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;

            Mesh newMesh = new Mesh();
            // Use UInt32 to support large meshes (over 65k vertices)
            newMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            newMesh.CombineMeshes(combines.ToArray(), true, true);

            mf.sharedMesh = newMesh;
            meshObj.isStatic = true;
        }
    }
}