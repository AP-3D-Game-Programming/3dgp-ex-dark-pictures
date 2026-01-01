using System.Collections;
using Unity.InferenceEngine.Tokenization.Truncators;
using Unity.VisualScripting;
using UnityEngine;

public class FlashIndicator : MonoBehaviour
{
    float cooldownTime = 0;
    ObjectColor color;
    Renderer rend;
    void Start()
    {
        color = ObjectColor.GREEN;
        rend = gameObject.GetComponent<Renderer>();
    }

    public void SetCooldown(float seconds)
    {
        cooldownTime = seconds;
        StartCoroutine(WaitTillCooldownUp());
        SetColor(ObjectColor.RED);
    }
    private IEnumerator WaitTillCooldownUp()
    {
        while (cooldownTime-- > 0)
        {
            yield return new WaitForSeconds(1f);
        }
        SetColor(ObjectColor.GREEN);
    }

    public void SetColor(ObjectColor color)
    {
        this.color = Utils.SetObjectColor(color, rend);
    }
}
