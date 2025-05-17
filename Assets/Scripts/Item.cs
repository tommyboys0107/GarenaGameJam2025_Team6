using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField]
    BoxCollider boxCollider = null;
    public enum ItemType
    {
        DeadBody,
        Water,
        Power,
    }
    
    [SerializeField]
    ItemType itemType = ItemType.DeadBody;

    public void PlayerGetItem()
    {
        boxCollider.enabled = false;
    }

}
