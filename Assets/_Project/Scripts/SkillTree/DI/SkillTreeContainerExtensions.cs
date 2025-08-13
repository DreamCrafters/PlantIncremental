using VContainer;
using VContainer.Unity;

/// <summary>
/// Расширение для регистрации в GameLifetimeScope
/// </summary>
public static class SkillTreeContainerExtensions
{
    public static void RegisterSkillTreeSystem(this IContainerBuilder builder, SkillTreeConfig config)
    {
        // Регистрируем конфигурацию
        builder.RegisterInstance(config);
        
        // Регистрируем сервисы
        builder.Register<IGameParametersService, GameParametersService>(Lifetime.Singleton);
        builder.Register<IUnlockService, UnlockService>(Lifetime.Singleton);
        
        // Регистрируем фабрику и реестр
        builder.Register<SkillFactory>(Lifetime.Singleton)
            .As<ISkillFactory>()
            .As<ISkillRegistry>();
        
        // Регистрируем основной сервис
        builder.Register<ISkillTreeService, SkillTreeService>(Lifetime.Singleton);
        
        // Регистрируем презентер
        builder.RegisterEntryPoint<SkillTreePresenter>();
    }
}