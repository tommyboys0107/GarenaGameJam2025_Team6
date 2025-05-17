using UnityEngine;

public class CraftingTable : MonoBehaviour
{
    public void CraftItem(Item item)
    {
        Debug.Log("CraftItem now!");
        Destroy(item.transform.parent.gameObject);
    }

}
