using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.Generation.Terrain
{
    public class TerrainPool : MonoBehaviour
    {
        private static Dictionary<GameObject, Stack<GameObject>> freeObjects = new Dictionary<GameObject, Stack<GameObject>>();
        private static Dictionary<GameObject, GameObject> usedObjects = new Dictionary<GameObject, GameObject>();

        private static TerrainPool instance;

        private void Awake()
        {
            freeObjects = new Dictionary<GameObject, Stack<GameObject>>();
            usedObjects = new Dictionary<GameObject, GameObject>();
            instance = this;
        }

        private void OnDestroy()
        {
            freeObjects = new Dictionary<GameObject, Stack<GameObject>>();
            usedObjects = new Dictionary<GameObject, GameObject>();
            instance = this;
        }

        public static void DestroyTerrainObject(GameObject go)
        {
            if (!Application.isPlaying)
            {
                Destroy(go);
                return;
            }

            GameObject prefab = usedObjects[go];
            usedObjects.Remove(go);

            if (!freeObjects.ContainsKey(prefab))
                freeObjects.Add(prefab, new Stack<GameObject>());

            freeObjects[prefab].Push(go);

            go.transform.parent = instance.transform;

            go.SetActive(false);
        }

        public static GameObject CreateTerrainObject(TerrainObject terrainObject, Block block)
        {
            if (!Application.isPlaying)
            {
                GameObject go = Instantiate(terrainObject.prefab);
                go.tag = "Spawned";

                SetGameObjectValues(terrainObject, go, block);
                return go;
            }

            if (!freeObjects.ContainsKey(terrainObject.prefab) || freeObjects[terrainObject.prefab].Count <= 0)
            {
                GameObject go = Instantiate(terrainObject.prefab);
                go.tag = "Spawned";

                SetGameObjectValues(terrainObject, go, block);

                usedObjects.Add(go, terrainObject.prefab);

                return go;
            } else
            {
                GameObject go = freeObjects[terrainObject.prefab].Pop();
                go.tag = "Spawned";

                SetGameObjectValues(terrainObject, go, block);

                go.SetActive(true);

                usedObjects.Add(go, terrainObject.prefab);

                return go;
            }
        }

        private static void SetGameObjectValues(TerrainObject terrainObject, GameObject go, Block block)
        {
            go.transform.position = terrainObject.position + block.transform.position;
            go.transform.rotation = terrainObject.rotation;
            go.transform.localScale = Vector3.one * terrainObject.scale;
            go.transform.parent = block.transform;
        }
    }
}