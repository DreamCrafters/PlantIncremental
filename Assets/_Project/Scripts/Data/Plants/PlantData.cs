using UnityEngine;

[CreateAssetMenu(fileName = "PlantData", menuName = "Game/Plant")]
public class PlantData : ScriptableObject
{
    public string DisplayName;
    public Sprite[] GrowthStages;
    public float GrowthTime = 10f;
    public int SellPrice = 20;
    public PlantType Type;
    public PlantRarity Rarity;
}