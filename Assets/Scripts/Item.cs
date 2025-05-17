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

    public void PlayerGetItem()
    {
        boxCollider.enabled = false;
    }


    public void SetHint(bool enable)
    {
        hint.SetActive(enable);
    }
}
