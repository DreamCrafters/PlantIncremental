using System;

/// <summary>
/// Сервис разблокировок
/// </summary>
public interface IUnlockService
{
    bool IsUnlocked(string featureId);
    void Unlock(string featureId);
    IObservable<string> OnUnlocked { get; }
}