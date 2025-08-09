using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PlantData", menuName = "Game/Plant")]
public class PlantData : ScriptableObject
{
    public string DisplayName;
    public PlantStateInfo[] GrowthStages;
    public float GrowthTime = 10f;
    public int SellPrice = 20;
    public PlantType Type;
    public PlantRarity Rarity;

    private void OnValidate()
    {
        if (GrowthStages == null || GrowthStages.Length != Enum.GetValues(typeof(PlantState)).Length)
        {
            var newStages = new PlantStateInfo[Enum.GetValues(typeof(PlantState)).Length];

            for (int i = 0; i < newStages.Length; i++)
            {
                newStages[i].State = (PlantState)i;
                newStages[i].Sprite = i < GrowthStages.Length ? GrowthStages[i].Sprite : null;
            }

            GrowthStages = newStages;
        }
    }

    [Serializable]
    public struct PlantStateInfo
    {
        public PlantState State;
        public Sprite Sprite;
    }
}