using UnityEngine;

/// <summary>
/// Механика ромашки: исцеляет соседние растения при поливе
/// </summary>
[CreateAssetMenu(fileName = "ChamomileHealNearbyOnWatered", menuName = "Game/Plant Mechanics/Watered/Chamomile Heal Nearby")]
public class ChamomileHealNearbyOnWatered : OnWateredMechanics
{
    [Header("Healing Settings")]
    [SerializeField] private int healRadius = 1;
    [SerializeField] private float healEffect = 0.2f;
    [SerializeField] private bool showDebugLog = true;

    public override void Execute(PlantEntity plant)
    {
        if (showDebugLog)
        {
            Debug.Log($"💧 Ромашка полита! Излучает целительную ауру в радиусе {healRadius} клеток...");
        }

        // TODO: Реализовать логику исцеления соседних растений
        // Потребуется доступ к GridService для поиска соседних растений
        // Пока что только демонстрационное сообщение

        HealNearbyPlants(plant);
    }

    private void HealNearbyPlants(PlantEntity chamomile)
    {
        // Заглушка для демонстрации
        // В полной реализации здесь будет:
        // 1. Получение GridService через DI или статический доступ
        // 2. Поиск соседних растений в радиусе healRadius
        // 3. Применение эффекта исцеления (ускорение роста, восстановление увядших растений и т.д.)

        if (showDebugLog)
        {
            Debug.Log($"🌿 Исцеляющий эффект применен! Сила исцеления: {healEffect}");
        }
    }
}