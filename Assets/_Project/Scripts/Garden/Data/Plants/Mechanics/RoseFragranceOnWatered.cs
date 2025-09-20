using UnityEngine;

/// <summary>
/// Механика розы: создает ароматический эффект при поливе, дающий временный бонус к доходу
/// </summary>
[CreateAssetMenu(fileName = "RoseFragranceOnWatered", menuName = "Game/Plant Mechanics/Watered/Rose Fragrance")]
public class RoseFragranceOnWatered : OnWateredMechanics
{
    [Header("Fragrance Effect Settings")]
    [SerializeField] private float fragranceDuration = 30f;
    [SerializeField] private float incomeBoostMultiplier = 1.2f;
    [SerializeField] private int affectedRadius = 3;
    [SerializeField] private bool showDebugLog = true;

    public override void Execute(PlantEntity plant)
    {
        if (showDebugLog)
        {
            Debug.Log($"🌹 Роза полита! Распространяет чарующий аромат на {fragranceDuration} секунд...");
        }

        ApplyFragranceEffect(plant);
    }

    private void ApplyFragranceEffect(PlantEntity rose)
    {
        // TODO: Реализовать систему временных эффектов
        // В полной реализации здесь будет:
        // 1. Создание временного эффекта через систему баффов
        // 2. Применение эффекта ко всем растениям в радиусе
        // 3. Автоматическое снятие эффекта через fragranceDuration секунд

        if (showDebugLog)
        {
            Debug.Log($"💖 Аромат розы повышает доходность растений в радиусе {affectedRadius} клеток " +
                     $"на {(incomeBoostMultiplier - 1f) * 100}% в течение {fragranceDuration} секунд!");
        }

        // Показываем визуальный эффект ароматических волн
        ShowFragranceVisualEffect(rose);
    }

    private void ShowFragranceVisualEffect(PlantEntity rose)
    {
        // TODO: Показать визуальный эффект распространения аромата
        // Волны, частицы, изменение цвета соседних растений и т.д.

        if (showDebugLog)
        {
            Debug.Log("✨ Ароматические волны распространяются от розы...");
        }
    }
}