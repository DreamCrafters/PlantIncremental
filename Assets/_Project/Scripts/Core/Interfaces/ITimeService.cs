using System;

public interface ITimeService
{
    float DeltaTime { get; }
    float TimeScale { get; set; }
    IObservable<float> EverySecond { get; }
    // IObservable<DayCycle> OnDayChanged { get; }
}
