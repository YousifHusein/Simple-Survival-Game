using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Item")]
public class Item : ScriptableObject
{
    public Sprite sprite;
    public SlotTag itemTag;

    [Header("Item Model")]
    public GameObject itemModel;

    [Header("If the item can be equipped")]
    public GameObject equipmentPrefab;

    [Header("Animation")]
    public RuntimeAnimatorController controller;

    public bool isStackable = false;
    public int maxStackSize = 1;
    public int currentStack;

    public int amountIfCrafted;

    [Header("Recipe Items")]
    public Item item1;
    public Item item2;
    public Item item3;

    [Header("Recipe Amount Requirements")]
    public int item1Amount;
    public int item2Amount;
    public int item3Amount;

    [Header("Damage")]
    public int damage;
}
