using System;
using UniRx;

public interface ISaveService
{
    void Save(SaveData data);
    SaveData Load();
    IObservable<Unit> OnAutoSave { get; }
}
