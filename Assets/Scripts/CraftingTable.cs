using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class CraftingTable : MonoBehaviour
{
    Transform itemParent;
    [SerializeField]
    GameObject foodPrefab = null;
    [SerializeField]
    GameObject skillEnergy1Prefab = null;
    [SerializeField]
    GameObject skillEnergy2Prefab = null;

    float duration = 1.0f;
    bool isUsing = false;
    Item.ItemType itemType;

    public void CraftItem(Item item)
    {
        itemType = item.GetType();
        Debug.Log("CraftItem now!");
        StartCoroutine(UseCraftingTable(item));
    }

    IEnumerator UseCraftingTable(Item item)
    {
        isUsing = true;
        itemParent = item.transform.parent;
        itemParent.parent = transform;
        Vector3 startPos = itemParent.localPosition;
        Vector3 controlPoint = new Vector3((startPos.x + 0f) / 2f, startPos.y * 1.5f, (startPos.z + 0f) / 2f);
        Vector3[] path = new Vector3[] { controlPoint, Vector3.zero };

        float timer = 0;
        // 使用 DOTween 的 DOLocalPath 搭配動畫曲線
        duration = 1.0f;
        itemParent.DOLocalPath(path, duration, PathType.CatmullRom).SetEase(Ease.OutQuad);
        // itemParent.DOLocalMove(Vector3.zero, duration);
        yield return new WaitForSeconds(duration * 0.66f);
        itemParent.DOScale(Vector3.zero, duration * 0.33f);
        yield return new WaitForSeconds(duration * 0.33f);
        
        Destroy(itemParent.gameObject);
        
        // TODO:製作東西的動畫
        Vector3 originPos = transform.localPosition;
        transform.DOShakePosition(1.0f, 0.3f);
        yield return new WaitForSeconds(1.0f);

        // 回到原本位置
        transform.localPosition = originPos;
        // 開始生成道具
        switch (itemType)
        {
            case Item.ItemType.DeadBody:
                var food = Instantiate(foodPrefab).transform;
                CreateNewItem(food);
                break;
            case Item.ItemType.Water:
                var energy1 = Instantiate(skillEnergy1Prefab).transform;
                CreateNewItem(energy1);
                break;
            case Item.ItemType.Power:
                var energy2 = Instantiate(skillEnergy2Prefab).transform;
                CreateNewItem(energy2);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void CreateNewItem(Transform newItem)
    {
        newItem.localPosition = transform.parent.localPosition;
        newItem.localScale = Vector3.zero;
        // 隨機移動到附近距離單位 2 的地方
        Vector3 randomDirection = Random.onUnitSphere;
        randomDirection.y = 0;
        randomDirection = randomDirection.normalized * 2;
        Vector3 startPos = newItem.localPosition;
        Vector3 controlPoint = new Vector3(startPos.x + randomDirection.x / 4f, 2, startPos.z + randomDirection.z / 4f);
        Vector3[] path = new Vector3[] { controlPoint, startPos + randomDirection};
        
        newItem.DOLocalPath(path, duration, PathType.CatmullRom).SetEase(Ease.OutQuad);
        newItem.DOScale(Vector3.one, duration * 0.33f);
    }

}
