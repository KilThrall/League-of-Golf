using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshCombiner : MonoBehaviour
{
    public PhysicMaterial physicMaterial;

    private void Start()
    {
        CombineMeshes(gameObject);
    }

    public void CombineMeshes(GameObject obj)
    {
        //Temporarily set position to zero to make matrix math easier
        Vector3 position = obj.transform.position;
        obj.transform.position = Vector3.zero;

        //Get all mesh filters and combine
        MeshFilter[] meshFilters = obj.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        int i = 1;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].GetComponent<MeshCollider>().enabled = false;
            i++;
        }

        obj.transform.GetComponent<MeshFilter>().mesh = new Mesh();
        obj.transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine, false, true);
        obj.transform.gameObject.SetActive(true);

        //Return to original position
        obj.transform.position = position;

        //Add collider to mesh (if needed)
        obj.AddComponent<MeshCollider>();
        obj.GetComponent<MeshCollider>().material = physicMaterial;
    }

}
