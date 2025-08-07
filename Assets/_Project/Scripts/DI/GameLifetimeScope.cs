using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [Header("Configurations")]
    [SerializeField] private GameSettings _gameSettings;
    [Header("Views")]
    [SerializeField] private CoinsView _coinsView;

    protected override void Configure(IContainerBuilder builder)
    {
        // Регистрация настроек
        builder.RegisterInstance(_gameSettings);

        // Регистрация сервисов
        builder.Register<ITimeService, TimeService>(Lifetime.Singleton);
        builder.Register<IEconomyService, EconomyService>(Lifetime.Singleton);
        builder.Register<ISaveService, SaveService>(Lifetime.Singleton);
        builder.Register<IGridService, GridService>(Lifetime.Singleton);
        
        // Регистрация представлений
        builder.RegisterInstance(_coinsView).AsSelf();

        // Регистрация фабрик
        builder.Register<IPlantFactory, PlantFactory>(Lifetime.Singleton);

        // Entry points
        builder.UseEntryPoints(entryPoint =>
        {
            // entryPoint.Add<GameInitializer>();
            entryPoint.Add<CoinsPresenter>();
        });
    }
}