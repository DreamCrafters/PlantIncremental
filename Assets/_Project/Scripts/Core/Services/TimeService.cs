using System;

public interface ITimeService
{
    float DeltaTime { get; }
    float TimeScale { get; set; }
    IObservable<float> EverySecond { get; }
    // IObservable<DayCycle> OnDayChanged { get; }
}

public class TimeService : ITimeService
{
    public float DeltaTime => throw new NotImplementedException();

    public float TimeScale { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public IObservable<float> EverySecond => throw new NotImplementedException();

    // public IObservable<DayCycle> OnDayChanged => throw new NotImplementedException();
}
