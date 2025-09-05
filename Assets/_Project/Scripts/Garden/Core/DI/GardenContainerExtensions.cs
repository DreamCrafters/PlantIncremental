using VContainer;
using VContainer.Unity;

public static class GardenContainerExtensions
{
    public static void RegisterGardenSystems(this IContainerBuilder builder, GameSettings gameSettings, CoinsView coinsView, GridView gridView, PetalsView petalsView)
    {
        // Register game settings
        builder.RegisterInstance(gameSettings);

        // Register input services
        builder.Register<InputService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();

        // Register core services
        builder.Register<ITimeService, TimeService>(Lifetime.Singleton);
        builder.Register<ISaveService, SaveService>(Lifetime.Singleton);
        builder.Register<IEconomyService, EconomyService>(Lifetime.Singleton);
        builder.Register<IRewardService, RewardService>(Lifetime.Singleton);
        builder.Register<IGridService, GridService>(Lifetime.Singleton);

        // Register plant management services
        builder.Register<IWateringManager, WateringManager>(Lifetime.Singleton);
        builder.Register<IPlantMechanicsFactory, PlantMechanicsFactory>(Lifetime.Singleton);

        // Register input and visualization services
        builder.Register<IWateringVisualizationService, WateringVisualizationService>(Lifetime.Singleton);

        // Register views
        builder.RegisterInstance(coinsView).AsSelf();
        builder.RegisterInstance(gridView).AsSelf();
        builder.RegisterInstance(petalsView).AsSelf();

        // Register factories
        builder.Register<IPlantFactory, PlantFactory>(Lifetime.Singleton);

        // Entry points
        builder.UseEntryPoints(entryPoint =>
        {
            entryPoint.Add<GameInitializer>();
            entryPoint.Add<EconomyPresenter>();
            entryPoint.Add<GridPresenter>();
            entryPoint.Add<WateringVisualizationPresenter>();
        });
    }
}
