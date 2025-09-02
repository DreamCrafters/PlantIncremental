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
    
    [Header("Plant Mechanics - Modular System")]
    [Tooltip("Механики, срабатывающие при посадке растения")]
    public OnPlantedMechanics[] PlantedMechanics;
    
    [Tooltip("Механики, срабатывающие при поливе растения")]  
    public OnWateredMechanics[] WateredMechanics;
    
    [Tooltip("Механики, срабатывающие при сборе урожая")]
    public OnHarvestedMechanics[] HarvestedMechanics;
    
    [Tooltip("Механики, срабатывающие при изменении стадии роста")]
    public OnGrowthStageChangedMechanics[] GrowthStageChangedMechanics;

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