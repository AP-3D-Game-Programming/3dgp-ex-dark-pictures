using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BatteryLife : MonoBehaviour
{
    private bool isEmpty = true;
    private Renderer rend;
    private ObjectColor color;

    public bool isDecreasingOverTime = false;

    [SerializeField] FlashIndicator flashIndicator;
    [SerializeField] AudioClip emptyClip;
    private AudioSource audioSource;

    void Start()
    {
        rend = GetComponent<Renderer>();
        audioSource = transform.GetComponent<AudioSource>();
    }
    private void SetBatteryColor(ObjectColor color)
    {
        Utils.SetObjectColor(color, rend);
        this.color = color;
    }
    public float GetBatteryLife()
    {
        return transform.localScale.z * 100;
    }
    // returns false if not enough battery percent to be decreased with decreasingPercent 
    public bool DecreaseBattery(float decreasingPercent)
    {
        float amount = decreasingPercent / 100;
        float newZ = transform.localScale.z - amount;

        if (newZ < -0.5f)
        {
            return false;
        }

        if (newZ <= 0f)
        {
            SetNewBatteryLife(-1);
            audioSource.PlayOneShot(emptyClip);
        }
        else SetNewBatteryLife(newZ);

        return true;
    }
    public IEnumerator DecreaseBatteryOverTime(float speedInSecond = 1f, float amountEachTime = 1f)
    {
        while (isDecreasingOverTime && GetBatteryLife() >= -0.5f)
        {
            DecreaseBattery(amountEachTime);
            yield return new WaitForSeconds(speedInSecond);
        }
    }

    // returns false if the battery is full
    public bool IncreaseBattery(float increasingPercent)
    {
        float amount = increasingPercent / 100;
        float z = transform.localScale.z < 0 ? 0 : transform.localScale.z;

        float newZ = z + amount;
        if (newZ < 0)
            return false;

        SetNewBatteryLife(newZ);
        return true;
    }
    public void RenewBattery()
    {
        SetNewBatteryLife(1);
    }
    public void SetNewBatteryLife(float newLife)
    {
        transform.localScale = new Vector3(
            transform.localScale.x,
            transform.localScale.y,
            newLife
        );
        HandleChangingState(newLife);
    }
    public void HandleChangingState(float newLife)
    {
        if (newLife <= 0)
        {
            SetBatteryColor(ObjectColor.RED);
            isEmpty = true;
            flashIndicator.SetColor(ObjectColor.RED);
        }
        else if (newLife > 0 && isEmpty == true)
        {
            SetBatteryColor(ObjectColor.GREEN);
            isEmpty = false;
            flashIndicator.SetColor(ObjectColor.GREEN);
        }
    }
}
