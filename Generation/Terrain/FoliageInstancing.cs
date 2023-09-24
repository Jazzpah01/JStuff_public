using JStuff.Generation.Terrain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class FoliageInstancing : MonoBehaviour
{
    public Camera camera;
    public Transform cameraTransform;

    public FoliageLODSettings foliageLODSettings;

    private List<(FoliageObject, List<List<Matrix4x4>>)> instancedFoliage;

    [Header("Debug")]
    public bool hasSettings = false;
    public bool hasFoliage = false;

    private Renderer renderer;

    private float chunkSize;

    public void SetSettings(FoliageLODSettings settings, float chunkSize, Transform cameraTransform)
    {
        if (settings == null || chunkSize == 0 || cameraTransform == null)
            throw new System.Exception($"Settings are incorrect! (LODSettings: {settings}, chunkSize: {chunkSize}, cameraTransform: {cameraTransform}");

        foliageLODSettings = settings;
        this.cameraTransform = cameraTransform;
        this.chunkSize = chunkSize;
        hasSettings = true;
        camera = cameraTransform.GetComponentInChildren<Camera>();
    }

    public void SetInstancedFoliage(List<(FoliageObject, List<List<Matrix4x4>>)>  instancedFoliage)
    {
        this.instancedFoliage = instancedFoliage;
        hasFoliage = (instancedFoliage != null && instancedFoliage.Count != 0);
    }

    private void Start()
    {
        camera = Camera.main;
        renderer = GetComponent<MeshRenderer>();
    }

    //static bool calculated = false;
    //static Plane[] planes;

    //private void Update()
    //{
    //    if (!calculated)
    //    {
    //        camera = Camera.main;
    //        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
    //    }
    //}

    private void Update()
    {
        if (!hasSettings)
            return;

        
        Vector3 cameraPosition = camera.transform.position;

        //Vector3 pos0 = transform.position;
        //Vector3 pos1 = transform.position + new Vector3(chunkSize, 0, 0);
        //Vector3 pos2 = transform.position + new Vector3(0, 0, chunkSize);
        //Vector3 pos3 = transform.position + new Vector3(chunkSize, 0, chunkSize);


        //Vector3 viewPos = camera.WorldToViewportPoint(pos0);
        //if (!(viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1 && viewPos.z > 0))
        //{
        //    viewPos = camera.WorldToViewportPoint(pos1);
        //    if (!(viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1 && viewPos.z > 0))
        //    {
        //        viewPos = camera.WorldToViewportPoint(pos2);
        //        if (!(viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1 && viewPos.z > 0))
        //        {
        //            viewPos = camera.WorldToViewportPoint(pos3);
        //            if (!(viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1 && viewPos.z > 0))
        //                return;
        //        }
        //    }
        //}

        if (hasFoliage)
        {
            if (foliageLODSettings.cullChunkOnDistance)
            {
                int cullCount = 0;

                if (foliageLODSettings.GetLOD(Vector3.Distance(transform.position, cameraPosition)) < 0)
                    cullCount++;
                if (foliageLODSettings.GetLOD(Vector3.Distance(transform.position + new Vector3(chunkSize, 0, 0), cameraPosition)) < 0)
                    cullCount++;
                if (foliageLODSettings.GetLOD(Vector3.Distance(transform.position + new Vector3(0, 0, chunkSize), cameraPosition)) < 0)
                    cullCount++;
                if (foliageLODSettings.GetLOD(Vector3.Distance(transform.position + new Vector3(chunkSize, 0, chunkSize), cameraPosition)) < 0)
                    cullCount++;

                if (cullCount < 4 && renderer.isVisible)
                {
                    RenderFoliage(cameraPosition);
                }
            }
            else if (renderer.isVisible)
            {
                RenderFoliage(cameraPosition);
            }
        }
    }

    private void RenderFoliage(Vector3 cameraPosition)
    {
        //if (!GeometryUtility.TestPlanesAABB(planes, renderer.bounds))
        //    return;

        int chunkMinLOD = foliageLODSettings.maxLOD;

        int cornorLOD = foliageLODSettings.GetLOD(Vector3.Distance(transform.position, cameraPosition));
        if (cornorLOD < chunkMinLOD && cornorLOD > -1)
            chunkMinLOD = cornorLOD;

        cornorLOD = foliageLODSettings.GetLOD(Vector3.Distance(transform.position + new Vector3(chunkSize, 0, 0), cameraPosition));
        if (cornorLOD < chunkMinLOD && cornorLOD > -1)
            chunkMinLOD = cornorLOD;

        cornorLOD = foliageLODSettings.GetLOD(Vector3.Distance(transform.position + new Vector3(0, 0, chunkSize), cameraPosition));
        if (cornorLOD < chunkMinLOD && cornorLOD > -1)
            chunkMinLOD = cornorLOD;

        cornorLOD = foliageLODSettings.GetLOD(Vector3.Distance(transform.position + new Vector3(chunkSize, 0, chunkSize), cameraPosition));
        if (cornorLOD < chunkMinLOD && cornorLOD > -1)
            chunkMinLOD = cornorLOD;

        for (int i = 0; i < instancedFoliage.Count; i++)
        {
            FoliageObject foliageObject = instancedFoliage[i].Item1;

            if (foliageObject.maxLOD < chunkMinLOD)
                continue;

            List<List<Matrix4x4>> batches = instancedFoliage[i].Item2;

            if (foliageObject.LODMeshes.Count == 1)
            {
                // Normal, don't rearrange foliage

                FoliageLODObject LODObject = foliageObject.LODMeshes[0];

                foreach (List<Matrix4x4> batch in batches)
                {
                    for (int j = 0; j < LODObject.mesh.subMeshCount; j++)
                    {
                        Graphics.DrawMeshInstanced(LODObject.mesh, j, LODObject.materials[j], batch, new MaterialPropertyBlock(), castShadows: UnityEngine.Rendering.ShadowCastingMode.Off);
                    }
                }

                //if (cull)
                //{
                //    int[] count = new int[batches.Count];

                //    float cullDistance = foliageLODSettings.GetCullDistanceOfLOD(LODObject.LOD);

                //    for (int b = 0; b < batches.Count; b++)
                //    {
                //        List<Matrix4x4> batch = batches[b];
                //        count[b] = batch.Count;

                //        for (int j = 0; j < batch.Count; j++)
                //        {
                //            Vector4 col = batch[j].GetColumn(3);
                //            Vector3 pos = (Vector3)col;

                //            if ((pos - cameraPosition).sqrMagnitude > cullDistance * cullDistance)
                //            {
                //                // Switch
                //                Matrix4x4 tmp = batch[count[b] - 1];
                //                batch[count[b] - 1] = batch[j];
                //                batch[j] = tmp;
                //                count[b] -= 1;
                //            }
                //        }
                //    }

                //    for (int b = 0; b < batches.Count; b++)
                //    {
                //        List<Matrix4x4> batch = batches[b];

                //        if (count[b] > 0)
                //        {
                //            for (int j = 0; j < LODObject.mesh.subMeshCount; j++)
                //            {
                //                Graphics.DrawMeshInstanced(LODObject.mesh, j, LODObject.materials[j], batch, count[b], new MaterialPropertyBlock(), castShadows: UnityEngine.Rendering.ShadowCastingMode.Off);
                //            }
                //        }
                //    }
                //} else
                //{
                //    foreach (List<Matrix4x4> batch in batches)
                //    {
                //        for (int j = 0; j < LODObject.mesh.subMeshCount; j++)
                //        {
                //            Graphics.DrawMeshInstanced(LODObject.mesh, j, LODObject.materials[j], batch, new MaterialPropertyBlock(), castShadows: UnityEngine.Rendering.ShadowCastingMode.Off);
                //        }
                //    }
                //}

                
            } else
            {
                // LOD foliage, create new batches
            }
        }
    }
}