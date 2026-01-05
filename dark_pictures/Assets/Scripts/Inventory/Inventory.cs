using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Constraints;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class Inventory : MonoBehaviour
{
    public Dictionary<string, List<GameObject>> items;
    private Dictionary<string, KeyCode> itemsKeys;
    private int startKeyCode => 48;

    // public Dictionary<string, GameObject> ItemUI;
    [SerializeField] GameObject InventoryUI;
    [SerializeField] GameObject ItemUIPrefab;
    [SerializeField] PlayerPhoneSystem PhoneSystem;
    [SerializeField] BatteryLife BatteryLife;
    [SerializeField] AudioClip batteryUsingClip;
    private AudioSource audioSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        items = new Dictionary<string, List<GameObject>>();
        itemsKeys = new Dictionary<string, KeyCode>();
        audioSource = transform.GetComponent<AudioSource>();
    }
    void Update()
    {
        if (!PhoneSystem.isPhoneUp)
            return;

        KeyCode input = KeyCode.None;
        foreach (var item in itemsKeys)
        {
            if (Input.GetKeyDown(item.Value))
                input = item.Value;
        }

        if (input == KeyCode.None) return;

        if (itemsKeys.TryGetValue("Battery", out var key)) if (input == key) UseBattery();
    }
    public void AddObject(Transform other)
    {
        if (other.tag == "Key")
            other = other.parent;

        other.SetParent(transform, false);

        other.position = new Vector3(0, 0, 0);
        other.gameObject.SetActive(false);

        List<GameObject> list;

        if (!Contains(other.gameObject.tag, out list))
        {
            list = new List<GameObject>();
            items.Add(other.gameObject.tag, list);

            // Key's don't need key to use
            var keycode = other.tag != "Key"? startKeyCode + items.Count: 0;
            itemsKeys.Add(other.tag, (KeyCode) keycode);
        }

        list.Add(other.gameObject);

        AddToUI(other);
    }
    /// <summary>
    /// Uses trans.gameObject.tag to find the Image in the folder Images/
    /// If UI with the same tag is exist then the amount increases by 1
    /// </summary>
    /// <param name="trans"></param>
    private void AddToUI(Transform trans)
    {
        var obj = trans.gameObject;

        GameObject itemUI;

        itemUI = InventoryUI.transform.
            Find($"{obj.tag}UI")?.gameObject;

        if (itemUI != null)
        {
            items.TryGetValue(obj.tag, out var list);

            var amount1 = itemUI.transform
                .Find("ItemAndAmountHolderUI")
                .Find("ItemAmount").GetComponent<TextMeshProUGUI>();

            amount1.text = $"x{list.Count}";

            itemUI.transform.SetParent(InventoryUI.transform);
            return;
        }

        itemUI = Instantiate(ItemUIPrefab, InventoryUI.transform, false);

        // reset transform
        var it = itemUI.transform;
        it.localScale = new Vector3(1, 1, 1);
        it.localPosition = new Vector3(it.position.x, it.position.y, 0);

        var holder = itemUI.transform.Find("ItemAndAmountHolderUI");
        var keyForUse = itemUI.transform.Find("ItemKeyToUse").GetComponent<TextMeshProUGUI>();

        var image = holder.Find("ItemImage").GetComponent<RawImage>();
        var amount = holder.Find("ItemAmount").GetComponent<TextMeshProUGUI>();

        // button.text = $"[{}]"
        amount.text = "x1";
        image.texture = Resources.Load<Texture2D>($"Images/{obj.tag}");

        // if it is key no need for  keyForUse
        var keyForUseText = $"";
        if (obj.tag != "Key")
            keyForUseText = $"[{GetItemUseKey(obj.tag)}]";
        keyForUse.text = keyForUseText;

        // Rename it so we can distinct and find it when adjustment needed.
        itemUI.name = $"{obj.tag}UI";
    }
    private bool RemoveObject(string tag)
    {
        if (!items.TryGetValue(tag, out var list) || list == null)
            return false;
        return RemoveObject(list[0]);
    }

    private bool RemoveObject(GameObject gameObject)
    {
        var tag = gameObject.tag;

        if (!items.TryGetValue(tag, out var list) || list == null)
            return false;


        if (list.Count > 0)
        {
            list.RemoveAt(0);
            GetAmountUI(tag).text = $"x{list.Count}";

            Destroy(gameObject);
        }

        if (list.Count <= 0)
        {
            var itemUI = InventoryUI.transform.Find($"{tag}UI");

            items.Remove(tag);
            itemsKeys.Remove(tag);
            Destroy(itemUI.gameObject);
        }

        return true;
    }

    private TextMeshProUGUI GetAmountUI(string tag)
    {
        var itemUI = InventoryUI.transform.Find($"{tag}UI");
        var amount = itemUI.Find("ItemAndAmountHolderUI")
            .Find("ItemAmount").GetComponent<TextMeshProUGUI>();

        return amount;
    }

    private int GetItemUseKey(string tag)
    {
        itemsKeys.TryGetValue(tag, out var num);
        return (int)num - startKeyCode;
    }
    /// <summary>
    /// Checks if the an el with the tag exist in Inventory
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="list">returns the list if it exist otherwise null</param>
    /// <returns></returns>
    public bool Contains(string tag, out List<GameObject> list)
    {
        if (items.TryGetValue(tag, out list))
            return list.Count > 0;

        list = null;
        return false;
    }
    public bool Contains(GameObject gameObject) => Contains(gameObject.tag, out _);
    public bool Contains(string tag) => Contains(tag, out _);

    public void Print()
    {
        foreach (var item in items)
        {
            Debug.Log($"{item.Key}: ");
            Utils.PrintTags(item.Value);
        }
    }

    public bool UseBattery()
    {
        if (!RemoveObject("Battery"))
            return false;

        audioSource.PlayOneShot(batteryUsingClip);
        BatteryLife.RenewBattery();
        return true;
    }

    public bool UseItem(string tag) => RemoveObject(tag);
}