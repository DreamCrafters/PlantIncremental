using System;
using UnityEngine;

public interface IGridInputHandler
{
    IObservable<Vector2Int> OnCellSelected { get; }
}