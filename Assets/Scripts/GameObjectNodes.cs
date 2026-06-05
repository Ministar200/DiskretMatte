using TMPro;
using UnityEngine;

public class GameObjectNodes : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private TextMeshProUGUI gCostText;
    [SerializeField] private TextMeshProUGUI hCostText;
    [SerializeField] private TextMeshProUGUI fCostText;

    //The function which changes the colors of the Game Object Nodes.
    public void ChangeColor(ColorState state)
    {
        //A switch case which decides the color based on the ColorState state parameter. 
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

    //The function which changes the text of the text components of each node. This function takes in three int parameters and adds them to the strings.
    public void ChangeText(int gCost, int hCost, int fCost)
    {
        gCostText.text = "GCost: \n" + gCost;
        hCostText.text = "HCost: \n" + hCost;
        fCostText.text = "FCost: \n" + fCost;
    }
}

//An enum which allows us to easily decide which color we want each node to become.
public enum ColorState
{
    NonSearchedColorState,
    SearchedColorState,
    PathColorState,
    BlockadeColorState,
    ObstacleColorState
}