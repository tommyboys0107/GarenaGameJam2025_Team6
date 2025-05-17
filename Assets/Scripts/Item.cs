using UnityEngine;

public class Item : MonoBehaviour
{
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
        
    }

}
