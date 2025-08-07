using System;
using UniRx;
using UnityEngine;

public class GridService : IGridService
{
    public IReadOnlyReactiveProperty<GridCell[,]> Grid => throw new NotImplementedException();

    public IObservable<GridCell> OnCellClicked => throw new NotImplementedException();

    public GridCell GetCell(Vector2Int position)
    {
        throw new NotImplementedException();
    }

    public bool TryHarvestAt(Vector2Int position)
    {
        throw new NotImplementedException();
    }

    public bool TryPlantAt(Vector2Int position, PlantData data)
    {
        throw new NotImplementedException();
    }
}
