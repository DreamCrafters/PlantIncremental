using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Визуальное представление растения с анимациями и эффектами
/// </summary>
public class PlantView : MonoBehaviour
{
    private static readonly WaitForSeconds _waitForSeconds1 = new(1f);

    [Header("Components")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Transform _visualTransform;
    [SerializeField] private GameObject _wateringIcon; // Иконка полива
    [SerializeField] private GameObject _witheredIcon; // Иконка увядшего растения

    [Header("Animation Settings")]
    [SerializeField] private float _harvestAnimationDuration = 0.3f;

    // Кэшированные компоненты
    private Vector3 _originalScale;
    private Color _originalColor;
    private bool _isWithered = false;

    // Состояние
    private bool _isAnimating;
    private Coroutine _passiveEffectCoroutine;

    // Отслеживание твинов
    private readonly List<Tween> _activeTweens = new();
    
    // Переиспользуемые последовательности для оптимизации памяти
    private Sequence _reusableSequence;
    private bool _sequenceInUse;

    private void Awake()
    {
        CacheComponents();
        _originalScale = transform.localScale;
        _originalColor = _spriteRenderer.color;

        // Инициализируем переиспользуемую последовательность
        InitializeReusableSequence();

        // Скрываем иконки по умолчанию
        if (_wateringIcon != null)
        {
            _wateringIcon.SetActive(false);
        }

        if (_witheredIcon != null)
        {
            _witheredIcon.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        KillAllTweens();

        if (_passiveEffectCoroutine != null)
        {
            StopCoroutine(_passiveEffectCoroutine);
        }

        // Очищаем переиспользуемую последовательность
        if (_reusableSequence != null)
        {
            _reusableSequence.Kill();
            _reusableSequence = null;
        }
    }

    /// <summary>
    /// Обновляет спрайт растения с анимацией
    /// </summary>
    public void UpdateSprite(Sprite newSprite)
    {
        // Если newSprite == null, скрываем растение
        if (newSprite == null)
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.sprite = null;
            }
            return;
        }

        if (_spriteRenderer.sprite == newSprite) return;

        if (_isAnimating == false)
        {
            AnimateSpriteChange(newSprite);
        }
        else
        {
            // Если идет анимация, просто меняем спрайт
            _spriteRenderer.sprite = newSprite;
        }
    }

    /// <summary>
    /// Анимация сбора урожая
    /// </summary>
    public void PlayHarvestAnimation()
    {
        if (_visualTransform == null || _spriteRenderer == null) return;

        // Подпрыгивание и исчезновение
        var sequence = GetSequence();
        sequence.SetTarget(_visualTransform);

        var jumpTween = _visualTransform.DOJump(
            _visualTransform.position + Vector3.up * 0.5f,
            1f, 1, _harvestAnimationDuration)
            .SetTarget(_visualTransform);

        var fadeTween = _spriteRenderer.DOFade(0, _harvestAnimationDuration)
            .SetTarget(_spriteRenderer);

        var scaleTween = _visualTransform.DOScale(Vector3.zero, _harvestAnimationDuration)
            .SetEase(Ease.InBack)
            .SetTarget(_visualTransform);

        sequence.Append(jumpTween);
        sequence.Join(fadeTween);
        sequence.Join(scaleTween);

        sequence.OnComplete(() =>
        {
            ReleaseSequence(sequence);
            if (gameObject != null)
            {
                gameObject.SetActive(false);
            }
        });

        AddTween(sequence);
    }

    /// <summary>
    /// Анимация уничтожения увядшего растения
    /// </summary>
    public void PlayDestroyAnimation()
    {
        if (_visualTransform == null || _spriteRenderer == null) return;

        // Анимация уничтожения с эффектом разрушения
        var sequence = GetSequence();
        sequence.SetTarget(_visualTransform);

        // Быстрое дрожание
        var shakeTween = _visualTransform.DOShakePosition(0.3f, strength: 0.1f, vibrato: 20)
            .SetTarget(_visualTransform);

        // Изменение цвета на красный
        var colorTween = _spriteRenderer.DOColor(Color.red, 0.2f)
            .SetTarget(_spriteRenderer);

        // Уменьшение и исчезновение
        var scaleTween = _visualTransform.DOScale(Vector3.zero, 0.4f)
            .SetEase(Ease.InExpo)
            .SetTarget(_visualTransform);

        var fadeTween = _spriteRenderer.DOFade(0, 0.4f)
            .SetTarget(_spriteRenderer);

        sequence.Append(shakeTween);
        sequence.Join(colorTween);
        sequence.Append(scaleTween);
        sequence.Join(fadeTween);

        sequence.OnComplete(() =>
        {
            ReleaseSequence(sequence);
            if (gameObject != null)
            {
                gameObject.SetActive(false);
            }
        });

        AddTween(sequence);
    }

    /// <summary>
    /// Показывает визуальный эффект пассивной способности
    /// </summary>
    public void ShowPassiveEffect()
    {
        // Запускаем периодическую анимацию
        if (_passiveEffectCoroutine != null)
        {
            StopCoroutine(_passiveEffectCoroutine);
        }
        _passiveEffectCoroutine = StartCoroutine(PassiveEffectAnimation());
    }

    /// <summary>
    /// Воспроизводит эффект увядания
    /// </summary>
    public void PlayWitherEffect()
    {
        if (_visualTransform == null || _spriteRenderer == null) return;

        SetWitheredVisual();

        // Показываем иконку увядшего растения
        ShowWitheredIcon();

        // Анимация увядания
        var sequence = GetSequence();
        sequence.SetTarget(_visualTransform);

        // Растение "опускается"
        var rotateTween = _visualTransform.DORotate(new Vector3(0, 0, -15), 0.5f)
            .SetTarget(_visualTransform);
        var scaleTween = _visualTransform.DOScale(_originalScale * 0.8f, 0.5f)
            .SetTarget(_visualTransform);

        sequence.Append(rotateTween);
        sequence.Join(scaleTween);
        sequence.OnComplete(() => ReleaseSequence(sequence));

        AddTween(sequence);
    }

    /// <summary>
    /// Показывает иконку полива
    /// </summary>
    public void ShowWateringIcon()
    {
        if (_wateringIcon != null)
        {
            _wateringIcon.SetActive(true);
        }
    }

    /// <summary>
    /// Скрывает иконку полива
    /// </summary>
    public void HideWateringIcon()
    {
        if (_wateringIcon != null)
        {
            _wateringIcon.SetActive(false);
        }
    }

    /// <summary>
    /// Показывает иконку увядшего растения
    /// </summary>
    public void ShowWitheredIcon()
    {
        if (_witheredIcon != null)
        {
            _witheredIcon.SetActive(true);
        }

        // Скрываем иконку полива, если она была видна
        HideWateringIcon();
    }

    /// <summary>
    /// Скрывает иконку увядшего растения
    /// </summary>
    public void HideWitheredIcon()
    {
        if (_witheredIcon != null)
        {
            _witheredIcon.SetActive(false);
        }
    }

    /// <summary>
    /// Останавливает пассивный эффект
    /// </summary>
    public void StopPassiveEffect()
    {
        if (_passiveEffectCoroutine != null)
        {
            StopCoroutine(_passiveEffectCoroutine);
            _passiveEffectCoroutine = null;
        }

        if (_visualTransform == null) return;

        // Возвращаем оригинальный масштаб
        var scaleTween = _visualTransform.DOScale(_originalScale, 0.3f)
            .SetTarget(_visualTransform);
        AddTween(scaleTween);
    }

    /// <summary>
    /// Показывает эффект успешного полива (минимальный эффект)
    /// </summary>
    public void PlayWaterSuccessEffect()
    {
    }

    /// <summary>
    /// Подсветка при наведении
    /// </summary>
    public void SetHighlight(bool active)
    {
        if (_spriteRenderer == null || _visualTransform == null || _isWithered) return;

        if (active)
        {
        }
        else
        {
        }
    }

    /// <summary>
    /// Публичный метод для остановки всех анимаций
    /// </summary>
    public void StopAllAnimations()
    {
        KillAllTweens();
        _isAnimating = false;
    }

    /// <summary>
    /// Безопасно убивает все активные твины
    /// </summary>
    private void KillAllTweens()
    {
        // Отменяем все активные твины из списка
        for (int i = _activeTweens.Count - 1; i >= 0; i--)
        {
            if (_activeTweens[i] != null && _activeTweens[i].IsActive())
            {
                _activeTweens[i].Kill();
            }
        }
        _activeTweens.Clear();

        // Дополнительная очистка по объектам
        DOTween.Kill(transform);
        if (_spriteRenderer != null) DOTween.Kill(_spriteRenderer);
        if (_visualTransform != null) DOTween.Kill(_visualTransform);
    }

    /// <summary>
    /// Устанавливает визуал увядшего растения
    /// </summary>
    private void SetWitheredVisual()
    {
        _isWithered = true;

        // Останавливаем все эффекты
        StopPassiveEffect();
    }

    /// <summary>
    /// Добавляет твин в список активных для отслеживания
    /// </summary>
    private void AddTween(Tween tween)
    {
        if (tween != null)
        {
            _activeTweens.Add(tween);

            // Используем слабую ссылку для избежания утечек памяти
            var weakRef = new System.WeakReference(tween);
            
            tween.OnComplete(() => RemoveTweenFromList(weakRef));
            tween.OnKill(() => RemoveTweenFromList(weakRef));
        }
    }

    /// <summary>
    /// Безопасно удаляет твин из списка активных через слабую ссылку
    /// </summary>
    private void RemoveTweenFromList(System.WeakReference tweenRef)
    {
        if (tweenRef.Target is Tween tween)
        {
            _activeTweens.Remove(tween);
        }
    }

    /// <summary>
    /// Инициализирует переиспользуемую последовательность для оптимизации памяти
    /// </summary>
    private void InitializeReusableSequence()
    {
        _reusableSequence = DOTween.Sequence()
            .SetAutoKill(false)
            .Pause();
    }

    /// <summary>
    /// Получает переиспользуемую последовательность или создает новую если занята
    /// </summary>
    private Sequence GetSequence()
    {
        if (!_sequenceInUse && _reusableSequence != null && !_reusableSequence.IsActive())
        {
            _sequenceInUse = true;
            _reusableSequence.Rewind();
            return _reusableSequence;
        }
        
        // Если основная последовательность занята, создаем новую
        return DOTween.Sequence();
    }

    /// <summary>
    /// Освобождает переиспользуемую последовательность
    /// </summary>
    private void ReleaseSequence(Sequence sequence)
    {
        if (sequence == _reusableSequence)
        {
            _sequenceInUse = false;
        }
    }

    private void CacheComponents()
    {
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_visualTransform == null)
            _visualTransform = transform;
    }

    /// <summary>
    /// Анимация смены спрайта при росте
    /// </summary>
    private void AnimateSpriteChange(Sprite newSprite)
    {
        if (_spriteRenderer == null) return;

        _spriteRenderer.sprite = newSprite;
    }

    /// <summary>
    /// Периодическая анимация для пассивного эффекта
    /// </summary>
    private IEnumerator PassiveEffectAnimation()
    {
        while (gameObject.activeInHierarchy && _visualTransform != null && _spriteRenderer != null)
        {
            // Мягкая пульсация
            var scaleTween1 = _visualTransform.DOScale(_originalScale * 1.05f, 1f)
                .SetEase(Ease.InOutSine)
                .SetTarget(_visualTransform);
            AddTween(scaleTween1);

            yield return _waitForSeconds1;

            if (_visualTransform == null) break;

            var scaleTween2 = _visualTransform.DOScale(_originalScale, 1f)
                .SetEase(Ease.InOutSine)
                .SetTarget(_visualTransform);
            AddTween(scaleTween2);

            yield return _waitForSeconds1;

            // Добавляем легкое свечение каждые 3 секунды
            if (Random.Range(0f, 1f) > 0.5f && _spriteRenderer != null)
            {
                var glowSequence = GetSequence();
                glowSequence.SetTarget(_spriteRenderer);

                var glow1 = _spriteRenderer.DOColor(_originalColor * 1.3f, 0.5f)
                    .SetTarget(_spriteRenderer);
                var glow2 = _spriteRenderer.DOColor(_originalColor, 0.5f)
                    .SetTarget(_spriteRenderer);

                glowSequence.Append(glow1);
                glowSequence.Append(glow2);
                glowSequence.OnComplete(() => ReleaseSequence(glowSequence));

                AddTween(glowSequence);
            }

            yield return _waitForSeconds1;
        }
    }
}