using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] private GameSettings _gameSettings;
    
    protected override void Configure(IContainerBuilder builder)
    {
        // Регистрация настроек
        builder.RegisterInstance(_gameSettings);
        
        // Регистрация сервисов
        builder.Register<ITimeService, TimeService>(Lifetime.Singleton);
        builder.Register<ISaveService, SaveService>(Lifetime.Singleton);
        builder.Register<IInputService, InputService>(Lifetime.Singleton);
        
        // Регистрация фабрик
        builder.Register<IPlantFactory, PlantFactory>(Lifetime.Singleton);
        
        // Entry point
        builder.RegisterEntryPoint<GameInitializer>();
    }
}