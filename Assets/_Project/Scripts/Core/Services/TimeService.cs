using System;
using UniRx;
using UnityEngine;

public class TimeService : ITimeService
{
    public float DeltaTime => Time.deltaTime;
    public float TimeScale { get; set; } = 1f;
    
    public IObservable<float> EverySecond => 
        Observable.Interval(TimeSpan.FromSeconds(1))
            .Select(_ => 1f);
}