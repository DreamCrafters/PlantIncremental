using VContainer;
using VContainer.Unity;

public static class GardenContainerExtensions
{
    public static void RegisterGardenSystems(this IContainerBuilder builder, GameSettings gameSettings, CoinsView coinsView, GridView gridView, PetalsView petalsView)
    {
        // Регистрация настроек
        builder.RegisterInstance(gameSettings);

        // Регистрация основных сервисов
        builder.Register<ITimeService, TimeService>(Lifetime.Singleton);
        builder.Register<ISaveService, SaveService>(Lifetime.Singleton);
        builder.Register<IEconomyService, EconomyService>(Lifetime.Singleton);
        builder.Register<IRewardService, RewardService>(Lifetime.Singleton);
        builder.Register<IGridService, GridService>(Lifetime.Singleton);
        
        // Регистрация новых сервисов управления растениями
        builder.Register<IWateringManager, WateringManager>(Lifetime.Singleton);
        builder.Register<IPlantMechanicsFactory, PlantMechanicsFactory>(Lifetime.Singleton);
        
        // Регистрация старых сервисов для совместимости (если они еще используются)
        builder.Register<IWateringSystem, WateringSystem>(Lifetime.Singleton);

        // Регистрация представлений
        builder.RegisterInstance(coinsView).AsSelf();
        builder.RegisterInstance(gridView).AsSelf();
        builder.RegisterInstance(petalsView).AsSelf();

        // Регистрация фабрик
        builder.Register<IPlantFactory, PlantFactory>(Lifetime.Singleton);

        // Entry points
        builder.UseEntryPoints(entryPoint =>
        {
            entryPoint.Add<GameInitializer>();
            entryPoint.Add<EconomyPresenter>();
            entryPoint.Add<GridPresenter>();
        });
    }
}
