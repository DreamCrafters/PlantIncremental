using UnityEngine;

/// <summary>
/// Абстрактный ScriptableObject для механик, срабатывающих при изменении стадии роста
/// </summary>
public abstract class OnGrowthStageChangedMechanics : ScriptableObject
{
    [Header("Growth Stage Changed Mechanics Info")]
    [TextArea(2, 4)]
    public string Description = "Описание механики при изменении стадии роста";

    /// <summary>
    /// Вызывается при изменении стадии роста растения
    /// </summary>
    /// <param name="plant">Растение у которого изменилась стадия</param>
    /// <param name="newState">Новая стадия роста</param>
    public abstract void Execute(IPlantEntity plant, PlantState newState);
}