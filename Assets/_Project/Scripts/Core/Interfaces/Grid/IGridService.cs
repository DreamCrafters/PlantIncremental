using System;
using UniRx;
using UnityEngine;

public interface IGridService
{
    IReadOnlyReactiveProperty<GridCell[,]> Grid { get; }
    IObservable<GridCell> OnCellClicked { get; }
    bool TryPlantAt(Vector2Int position, PlantData data);
    bool TryHarvestAt(Vector2Int position);
    GridCell GetCell(Vector2Int position);
}