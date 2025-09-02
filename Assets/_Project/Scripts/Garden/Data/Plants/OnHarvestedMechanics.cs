using UnityEngine;

/// <summary>
/// Абстрактный ScriptableObject для механик, срабатывающих при сборе урожая
/// </summary>
public abstract class OnHarvestedMechanics : ScriptableObject
{
    [Header("Harvested Mechanics Info")]
    [TextArea(2, 4)]
    public string Description = "Описание механики при сборе урожая";

    /// <summary>
    /// Вызывается когда растение собирают
    /// </summary>
    /// <param name="plant">Растение которое собирают</param>
    /// <param name="result">Результат сбора урожая, можно модифицировать</param>
    public abstract void Execute(IPlantEntity plant, PlantHarvestResult result);
}