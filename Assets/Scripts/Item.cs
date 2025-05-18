using System;
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
        Food,
        WaterEnergy,
        PowerEnergy,
    }

    [SerializeField]
    ItemType itemType = ItemType.DeadBody;

    public ItemType GetType()
    {
        return itemType;
    }

    public void PlayerGetItem()
    {
        switch (itemType)
        {
            case ItemType.DeadBody:
            case ItemType.Water:
            case ItemType.Power:
                boxCollider.enabled = false;
                break;
            default:
                break;
        }

        SetHint(false);
    }

    public void PlayerThrowItem()
    {
        boxCollider.enabled = true;
    }


    public void SetHint(bool enable)
    {
        hint.SetActive(enable);
    }
}