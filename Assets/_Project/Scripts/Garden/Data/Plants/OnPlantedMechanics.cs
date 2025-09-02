using UnityEngine;

/// <summary>
/// Абстрактный ScriptableObject для механик, срабатывающих при посадке растения
/// </summary>
public abstract class OnPlantedMechanics : ScriptableObject
{
    [Header("Planted Mechanics Info")]
    [TextArea(2, 4)]
    public string Description = "Описание механики при посадке";

    /// <summary>
    /// Вызывается когда растение посажено
    /// </summary>
    /// <param name="plant">Растение которое было посажено</param>
    /// <param name="gridPosition">Позиция на сетке где посажено растение</param>
    public abstract void Execute(IPlantEntity plant, Vector2Int gridPosition);
}