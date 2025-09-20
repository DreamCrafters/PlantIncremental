using UniRx;
using VContainer;
using VContainer.Unity;

/// <summary>
/// Презентер для управления визуализацией полива
/// </summary>
public class WateringVisualizationPresenter : IInitializable, System.IDisposable
{
    private readonly WateringVisualizationService _wateringVisualizationService;
    private readonly CompositeDisposable _disposables = new();
    
    [Inject]
    public WateringVisualizationPresenter(
        WateringVisualizationService wateringVisualizationService)
    {
        _wateringVisualizationService = wateringVisualizationService;
    }
    
    public void Initialize()
    {
        // Здесь можно добавить дополнительную логику инициализации
        // Например, логирование событий для отладки
        
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        // _wateringVisualizationService.OnWateringVisualizationStarted
        //     .Subscribe(_ => UnityEngine.Debug.Log("Watering visualization started"))
        //     .AddTo(_disposables);
            
        // _wateringVisualizationService.OnWateringVisualizationStopped
        //     .Subscribe(_ => UnityEngine.Debug.Log("Watering visualization stopped"))
        //     .AddTo(_disposables);
        #endif
        
        // Можно добавить дополнительные подписки на события
        // Например, интеграцию с аудио системой, аналитикой и т.д.
    }
    
    public void Dispose()
    {
        _disposables?.Dispose();
    }
}
