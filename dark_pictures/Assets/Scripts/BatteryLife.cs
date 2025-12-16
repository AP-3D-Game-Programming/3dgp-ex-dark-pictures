using System.Diagnostics;
using NUnit.Framework;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph;
using UnityEngine;
public class BatteryLife : MonoBehaviour {
    private bool isEmpty = true;
    private Renderer rend;
    private ObjectColor color;

    [SerializeField] FlashIndicator flashIndicator;
    void Start()
    {
        rend =  GetComponent<Renderer>();
    }
    private void SetBatteryColor(ObjectColor color)
    {
        Operations.SetObjectColor(color, rend);
        this.color = color;
    }
    public float GetBatteryLife()
    {
        return transform.localScale.z*100;
    }
    // returns false if not enough battery percent to be decreased with decreasingPercent 
    public bool DecreaseBattery(float decreasingPercent)
    {
        float amount =  decreasingPercent/100;
        float newZ = transform.localScale.z - amount;
        if (newZ < 0)
            return false;
        if (newZ == 0f)
            SetNewBatteryLife(-1);
        else SetNewBatteryLife(newZ);
        return true;
    }

    // returns false if the battery is full
    public bool IncreaseBattery(float increasingPercent)
    {
        float amount =  increasingPercent/100;
        float z = transform.localScale.z < 0 ? 0 : transform.localScale.z;
        
        float newZ =  z + amount;
        if (newZ < 0)
            return false;

        SetNewBatteryLife(newZ);
        return true;
    }
    public void SetNewBatteryLife(float newLife)
    {
        HandleChangingState(newLife);
        transform.localScale = new Vector3(
            transform.localScale.x,
            transform.localScale.y,
            newLife
        );
    }
    public void RenewBattery()
    {
        SetNewBatteryLife(1);
    }
    public void HandleChangingState(float newLife)
    {
        if (newLife <= 0)
        {
            SetBatteryColor(ObjectColor.RED);
            isEmpty = true;
            flashIndicator.SetColor(ObjectColor.RED);
        } else if (newLife > 0 && isEmpty == true)
        {
            SetBatteryColor(ObjectColor.GREEN);
            isEmpty = false;
            flashIndicator.SetColor(ObjectColor.GREEN);
        }
    }
}
