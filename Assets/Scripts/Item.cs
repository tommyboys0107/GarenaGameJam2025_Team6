using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField]
    BoxCollider boxCollider = null;

    [SerializeField]
    GameObject hint = null;
    public enum ItemType
    {
        DeadBody,
        Water,
        Power,
    }
    
    [SerializeField]
    ItemType itemType = ItemType.DeadBody;

    public ItemType GetType()
    {
        return itemType;
    }

    public void PlayerGetItem()
    {
        SetHint(false);
        boxCollider.enabled = false;
    }


    public void SetHint(bool enable)
    {
        hint.SetActive(enable);
    }
}
