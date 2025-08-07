using System;
using UniRx;

public class SaveService : ISaveService
{
    public void Save(SaveData data)
    {
    }

    public SaveData Load()
    {
        return new SaveData();
    }

    public IObservable<Unit> OnAutoSave => Observable.Empty<Unit>();
}