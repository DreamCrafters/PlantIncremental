using System;
using UniRx;
using UnityEngine;

/// <summary>
/// Реализация сервиса времени, использующая Unity Time API
/// </summary>
public class TimeService : ITimeService
{
    public float CurrentTime => Time.time;
    public float DeltaTime => Time.deltaTime;

    public IObservable<long> CreateTimer(TimeSpan interval)
    {
        return Observable.Timer(interval);
    }

    public IObservable<long> CreateInterval(TimeSpan interval)
    {
        return Observable.Interval(interval);
    }
}