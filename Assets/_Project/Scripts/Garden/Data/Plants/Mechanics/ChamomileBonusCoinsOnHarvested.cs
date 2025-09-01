using UnityEngine;
using Garden.Data.Plants;

namespace Garden.Data.Plants.Mechanics
{
    /// <summary>
    /// Механика ромашки: дает бонусные монеты при сборе урожая
    /// </summary>
    [CreateAssetMenu(fileName = "ChamomileBonusCoinsOnHarvested", menuName = "Game/Plant Mechanics/Harvested/Chamomile Bonus Coins")]
    public class ChamomileBonusCoinsOnHarvested : OnHarvestedMechanics
    {
        [Header("Bonus Coins Settings")]
        [SerializeField] private int bonusCoins = 5;
        [SerializeField] private bool showDebugLog = true;

        public override void Execute(PlantEntity plant, PlantHarvestResult result)
        {
            // Добавляем бонусные монеты к результату
            result.Coins += bonusCoins;

            if (showDebugLog)
            {
                Debug.Log($"🌼 Ромашка собрана! Получен бонус: +{bonusCoins} монет! (Общий доход: {result.Coins})");
            }
        }
    }
}