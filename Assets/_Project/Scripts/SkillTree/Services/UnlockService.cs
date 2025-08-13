using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

/// <summary>
/// Реализация сервиса разблокировок
/// </summary>
public class UnlockService : IUnlockService
{
    private readonly HashSet<string> _unlockedFeatures = new();
    private readonly Subject<string> _onUnlocked = new();
    
    public IObservable<string> OnUnlocked => _onUnlocked;

    IObservable<string> IUnlockService.OnUnlocked => throw new NotImplementedException();

    public bool IsUnlocked(string featureId)
    {
        return _unlockedFeatures.Contains(featureId);
    }
    
    public void Unlock(string featureId)
    {
        if (_unlockedFeatures.Add(featureId))
        {
            _onUnlocked.OnNext(featureId);
            Debug.Log($"Feature unlocked: {featureId}");
        }
    }
}