using UnityEngine;
using DG.Tweening;

/// <summary>
/// Конфигурация DOTween для безопасной работы
/// </summary>
public static class DOTweenConfiguration
{
    /// <summary>
    /// Инициализирует DOTween с безопасными настройками
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        // Инициализация DOTween
        DOTween.Init(true, true, LogBehaviour.ErrorsOnly);

        // Настройки безопасности
        DOTween.defaultAutoPlay = AutoPlay.All;
        DOTween.defaultUpdateType = UpdateType.Normal;
        DOTween.defaultTimeScaleIndependent = false;

        // Важно: включаем Safe Mode для предотвращения ошибок
        DOTween.useSafeMode = true;

        // Настройка пула твинов для оптимизации
        DOTween.SetTweensCapacity(200, 50);

        Debug.Log("[DOTween] Initialized with safe settings");
    }

    /// <summary>
    /// Очистка DOTween при завершении приложения
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void OnApplicationFocus()
    {
        // Очистка при сворачивании/разворачивании приложения
        Application.focusChanged += OnFocusChanged;
    }

    private static void OnFocusChanged(bool hasFocus)
    {
        if (!hasFocus)
        {
            // При потере фокуса приостанавливаем все твины
            DOTween.PauseAll();
        }
        else
        {
            // При возвращении фокуса возобновляем
            DOTween.PlayAll();
        }
    }

    /// <summary>
    /// Полная очистка DOTween (вызывается при завершении приложения)
    /// </summary>
    public static void Cleanup()
    {
        DOTween.KillAll();
        DOTween.Clear();

        Application.focusChanged -= OnFocusChanged;

        Debug.Log("[DOTween] Cleaned up");
    }
}
