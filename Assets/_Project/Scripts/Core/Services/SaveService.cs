using System;
using UniRx;

public interface ISaveService
{
    void Save(SaveData data);
    SaveData Load();
    IObservable<Unit> OnAutoSave { get; }
}

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

public class SaveData
{
}