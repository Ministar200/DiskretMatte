using System;
using TMPro;
using UnityEngine;

public class PhysicalNodes : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private TextMeshProUGUI gCostText;
    [SerializeField] private TextMeshProUGUI hCostText;
    [SerializeField] private TextMeshProUGUI fCostText;

    public const int WHITENUM = 0;
    public const int ORANGENUM = 1;
    public const int BLACKNUM =2;
    public const int REDNUM = 3;
    public const int BLUENUM = 4;

    public void ChangeColor(int num)
    {
        if (num == 0)
        {
            meshRenderer.material.color = Color.white;
        }
        else if (num == 1)
        {
            meshRenderer.material.color = Color.orange;
        }
        else if (num == 2)
        {
            meshRenderer.material.color = Color.black;
        }
        else if (num == 3)
        {
            meshRenderer.material.color = Color.red;
        }
        else if (num == 4)
        {
            meshRenderer.material.color = Color.blue;
        }
    }

    public void ChangeText(int gCost, int hCost, int fCost)
    {
        gCostText.text = "GCost: \n" + gCost;
        hCostText.text = "HCost: \n" + hCost;
        fCostText.text = "FCost: \n" + fCost;
    }
}
