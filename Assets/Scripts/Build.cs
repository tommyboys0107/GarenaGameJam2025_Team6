using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class Build : MonoBehaviour
{
    [SerializeField]
    GameObject build = null;
    [SerializeField]
    GameObject item = null;
    // void OnCollisionEnter(Collision other)
    // {
    //     if (other.gameObject.CompareTag("Enemy"))
    //     {
    //         Collapse();
    //     }
    // }

    [Button]
    public void Collapse()
    {
        build.SetActive(false);
        PlayerStats.Instance.RecordBuilding();
        if (item != null)
        {
            item.SetActive(true);
        }
    }

}
