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
        var requiredLength = Enum.GetValues(typeof(PlantState)).Length;
        if (GrowthStages == null || GrowthStages.Length != requiredLength)
        {
            var oldStages = GrowthStages;
            var newStages = new PlantStateInfo[requiredLength];
            int oldLen = oldStages?.Length ?? 0;

            for (int i = 0; i < newStages.Length; i++)
            {
                newStages[i].State = (PlantState)i;
                newStages[i].Sprite = i < oldLen ? oldStages[i].Sprite : null;
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