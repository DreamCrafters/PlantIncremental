using System;
using UniRx;
using UnityEngine;

public interface IGridService
{
    IReadOnlyReactiveProperty<GridCell[,]> Grid { get; }
    IObservable<GridCell> OnCellClicked { get; }
    IObservable<PlantHarvestedEvent> OnPlantHarvested { get; }
    bool TryPlantAt(Vector2Int position);
    bool TryHarvestAt(Vector2Int position);
    GridCell GetCell(Vector2Int position);
    GridCell[] GetNeighbors(Vector2Int position, int radius = 1);
}