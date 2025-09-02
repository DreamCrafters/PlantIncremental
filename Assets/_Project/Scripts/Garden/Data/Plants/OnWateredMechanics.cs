using UnityEngine;

/// <summary>
/// Абстрактный ScriptableObject для механик, срабатывающих при поливе растения
/// </summary>
public abstract class OnWateredMechanics : ScriptableObject
{
    [Header("Watered Mechanics Info")]
    [TextArea(2, 4)]
    public string Description = "Описание механики при поливе";

    /// <summary>
    /// Вызывается когда растение поливают
    /// </summary>
    /// <param name="plant">Растение которое поливают</param>
    public abstract void Execute(IPlantEntity plant);
}