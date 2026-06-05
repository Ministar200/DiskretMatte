using System;
using TMPro;
using UnityEngine;

public class PhysicalNodes : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private TextMeshProUGUI gCostText;
    [SerializeField] private TextMeshProUGUI hCostText;
    [SerializeField] private TextMeshProUGUI fCostText;

    public void ChangeColor(ColorState state)
    {
        switch (state)
        {
            case ColorState.NonSearchedColorState:
                meshRenderer.material.color = Color.white;
                break;
            
            case ColorState.SearchedColorState:
                meshRenderer.material.color = Color.orange; 
                break;
            
            case ColorState.PathColorState:
                meshRenderer.material.color = Color.black;
                break;
            
            case ColorState.BlockadeColorState:
                meshRenderer.material.color = Color.red;
                break;
            
            case ColorState.ObstacleColorState:
                meshRenderer.material.color = Color.blue;
                break;
        }
    }

    public void ChangeText(int gCost, int hCost, int fCost)
    {
        gCostText.text = "GCost: \n" + gCost;
        hCostText.text = "HCost: \n" + hCost;
        fCostText.text = "FCost: \n" + fCost;
    }
}

public enum ColorState
{
    NonSearchedColorState,
    SearchedColorState,
    PathColorState,
    BlockadeColorState,
    ObstacleColorState
}