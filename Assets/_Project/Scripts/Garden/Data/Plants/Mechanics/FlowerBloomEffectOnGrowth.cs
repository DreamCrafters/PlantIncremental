using UnityEngine;

/// <summary>
/// Механика цветения: создает визуальные эффекты при изменении стадий роста
/// </summary>
[CreateAssetMenu(fileName = "FlowerBloomEffectOnGrowth", menuName = "Game/Plant Mechanics/Growth/Flower Bloom Effect")]
public class FlowerBloomEffectOnGrowth : OnGrowthStageChangedMechanics
{
    [Header("Visual Effect Settings")]
    [SerializeField] private bool showParticleEffects = true;
    [SerializeField] private bool playSoundEffects = true;
    [SerializeField] private bool showDebugLog = true;

    public override void Execute(PlantEntity plant, PlantState newState)
    {
        switch (newState)
        {
            case PlantState.Seed:
                OnSeedStage(plant);
                break;

            case PlantState.Growing:
                OnGrowingStage(plant);
                break;

            case PlantState.FullyGrown:
                OnFullyGrownStage(plant);
                break;

            case PlantState.Withered:
                OnWitheredStage(plant);
                break;
        }
    }

    private void OnSeedStage(PlantEntity plant)
    {
        if (showDebugLog)
        {
            Debug.Log($"🌱 {plant.Data.DisplayName} - стадия семени: подготовка к росту...");
        }

        // TODO: Показать эффект появления семени
        PlayEffect(plant, "seed_appear");
    }

    private void OnGrowingStage(PlantEntity plant)
    {
        if (showDebugLog)
        {
            Debug.Log($"🌿 {plant.Data.DisplayName} - стадия роста: активное развитие!");
        }

        // TODO: Показать эффект роста
        PlayEffect(plant, "growth_sparkle");
    }

    private void OnFullyGrownStage(PlantEntity plant)
    {
        if (showDebugLog)
        {
            Debug.Log($"🌺 {plant.Data.DisplayName} - полностью расцвел! Великолепие природы!");
        }

        // TODO: Показать эффект цветения
        PlayEffect(plant, "bloom_burst");
    }

    private void OnWitheredStage(PlantEntity plant)
    {
        if (showDebugLog)
        {
            Debug.Log($"💀 {plant.Data.DisplayName} - увял... Цикл жизни завершен.");
        }

        // TODO: Показать эффект увядания
        PlayEffect(plant, "wither_fade");
    }

    private void PlayEffect(PlantEntity plant, string effectName)
    {
        // TODO: Реализовать систему эффектов
        // Пока что заглушка для демонстрации архитектуры

        if (showParticleEffects)
        {
            // Эффекты частиц
        }

        if (playSoundEffects)
        {
            // Звуковые эффекты
        }
    }
}