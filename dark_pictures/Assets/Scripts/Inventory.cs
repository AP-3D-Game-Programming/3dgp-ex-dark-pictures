using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<GameObject> items; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       items = new List<GameObject>();
    }

    public void AddObject(Transform other)
    {
        other = other.parent;
        other.SetParent(transform, false);
        other.position = new Vector3(0,0,0);
        other.gameObject.SetActive(false);
        items.Add(other.gameObject);
    }
}
