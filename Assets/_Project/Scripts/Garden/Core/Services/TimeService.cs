using System;
using UniRx;
using UnityEngine;

public class TimeService : ITimeService
{
    public float DeltaTime => Time.deltaTime * TimeScale;
    public float TimeScale { get; set; } = 1f;
    
    public IObservable<float> EverySecond => 
        Observable.Interval(TimeSpan.FromSeconds(1))
            .ObserveOnMainThread()
            .Select(_ => 1f * TimeScale);
}