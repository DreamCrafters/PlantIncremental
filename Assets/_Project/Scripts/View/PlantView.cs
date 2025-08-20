using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Визуальное представление растения с анимациями и эффектами
/// </summary>
public class PlantView : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Transform _visualTransform;

    [Header("Effects")]
    [SerializeField] private ParticleSystem _growthParticles;
    [SerializeField] private ParticleSystem _harvestParticles;
    [SerializeField] private ParticleSystem _passiveEffectParticles;
    [SerializeField] private GameObject _witherOverlay;

    [Header("Animation Settings")]
    [SerializeField] private float _growthAnimationDuration = 0.5f;
    [SerializeField] private float _harvestAnimationDuration = 0.3f;

    // Кэшированные компоненты
    private Vector3 _originalScale;
    private Color _originalColor;

    // Состояние
    private bool _isAnimating;
    private Coroutine _passiveEffectCoroutine;

    // Отслеживание твинов
    private readonly List<Tween> _activeTweens = new();

    private void Awake()
    {
        CacheComponents();
        _originalScale = transform.localScale;
        _originalColor = _spriteRenderer.color;
    }

    private void OnDestroy()
    {
        KillAllTweens();

        if (_passiveEffectCoroutine != null)
        {
            StopCoroutine(_passiveEffectCoroutine);
        }
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
    /// Добавляет твин в список активных для отслеживания
    /// </summary>
    private void AddTween(Tween tween)
    {
        if (tween != null)
        {
            _activeTweens.Add(tween);

            // Автоматически удаляем из списка по завершении
            tween.OnComplete(() =>
            {
                if (tween != null)
                {
                    _activeTweens.Remove(tween);
                }
            });

            // Дополнительная безопасность - удаляем из списка при убийстве
            tween.OnKill(() =>
            {
                if (tween != null)
                {
                    _activeTweens.Remove(tween);
                }
            });
        }
    }

    /// <summary>
    /// Обновляет спрайт растения с анимацией
    /// </summary>
    public void UpdateSprite(Sprite newSprite)
    {
        if (newSprite == null || _spriteRenderer.sprite == newSprite) return;

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
    /// Воспроизводит эффект завершения роста
    /// </summary>
    public void PlayGrowthCompleteEffect()
    {
        // Партиклы роста
        if (_growthParticles != null)
        {
            _growthParticles.Play();
        }

        if (_visualTransform == null || _spriteRenderer == null) return;

        // Анимация "радости"
        var sequence = DOTween.Sequence();
        sequence.SetTarget(_visualTransform);

        var rotate1 = _visualTransform.DORotate(new Vector3(0, 0, -10), 0.1f).SetTarget(_visualTransform);
        var rotate2 = _visualTransform.DORotate(new Vector3(0, 0, 10), 0.2f).SetTarget(_visualTransform);
        var rotate3 = _visualTransform.DORotate(new Vector3(0, 0, -5), 0.1f).SetTarget(_visualTransform);
        var rotate4 = _visualTransform.DORotate(new Vector3(0, 0, 0), 0.1f).SetTarget(_visualTransform);

        sequence.Append(rotate1);
        sequence.Append(rotate2);
        sequence.Append(rotate3);
        sequence.Append(rotate4);

        // Пульсация масштаба
        var scale1 = _visualTransform.DOScale(_originalScale * 1.3f, 0.3f)
            .SetEase(Ease.OutElastic)
            .SetTarget(_visualTransform);
        var scale2 = _visualTransform.DOScale(_originalScale, 0.2f)
            .SetTarget(_visualTransform);

        sequence.Join(scale1);
        sequence.Append(scale2);

        // Вспышка цвета
        var color1 = _spriteRenderer.DOColor(Color.white, 0.1f).SetTarget(_spriteRenderer);
        var color2 = _spriteRenderer.DOColor(_originalColor, 0.3f).SetTarget(_spriteRenderer);

        sequence.Join(color1);
        sequence.Append(color2);

        AddTween(sequence);
    }

    /// <summary>
    /// Анимация сбора урожая
    /// </summary>
    public void PlayHarvestAnimation()
    {
        if (_harvestParticles != null)
        {
            _harvestParticles.Play();
        }

        if (_visualTransform == null || _spriteRenderer == null) return;

        // Подпрыгивание и исчезновение
        var sequence = DOTween.Sequence();
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
        if (_passiveEffectParticles != null)
        {
            _passiveEffectParticles.Play();
        }

        // Запускаем периодическую анимацию
        if (_passiveEffectCoroutine != null)
        {
            StopCoroutine(_passiveEffectCoroutine);
        }
        _passiveEffectCoroutine = StartCoroutine(PassiveEffectAnimation());
    }

    /// <summary>
    /// Устанавливает визуал увядшего растения
    /// </summary>
    public void SetWitheredVisual()
    {
        if (_witherOverlay != null)
        {
            _witherOverlay.SetActive(true);
        }

        // Затемняем и обесцвечиваем спрайт
        _spriteRenderer.color = new Color(0.4f, 0.3f, 0.2f, 0.8f);

        // Останавливаем все эффекты
        StopPassiveEffect();
    }

    /// <summary>
    /// Воспроизводит эффект увядания
    /// </summary>
    public void PlayWitherEffect()
    {
        if (_visualTransform == null || _spriteRenderer == null) return;

        // Анимация увядания
        var sequence = DOTween.Sequence();
        sequence.SetTarget(_visualTransform);

        // Растение "опускается"
        var rotateTween = _visualTransform.DORotate(new Vector3(0, 0, -15), 0.5f)
            .SetTarget(_visualTransform);
        var scaleTween = _visualTransform.DOScale(_originalScale * 0.8f, 0.5f)
            .SetTarget(_visualTransform);
        var colorTween = _spriteRenderer.DOColor(new Color(0.4f, 0.3f, 0.2f, 0.8f), 1f)
            .SetTarget(_spriteRenderer);

        sequence.Append(rotateTween);
        sequence.Join(scaleTween);
        sequence.Join(colorTween);

        // Активируем оверлей
        sequence.OnComplete(() =>
        {
            if (this != null)
            {
                SetWitheredVisual();
            }
        });

        AddTween(sequence);
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

        if (_passiveEffectParticles != null)
        {
            _passiveEffectParticles.Stop();
        }

        if (_visualTransform == null) return;

        // Возвращаем оригинальный масштаб
        var scaleTween = _visualTransform.DOScale(_originalScale, 0.3f)
            .SetTarget(_visualTransform);
        AddTween(scaleTween);
    }

    /// <summary>
    /// Подсветка при наведении
    /// </summary>
    public void SetHighlight(bool active)
    {
        if (_spriteRenderer == null || _visualTransform == null) return;

        if (active)
        {
            _spriteRenderer.color = _originalColor * 1.2f;
            var scaleTween = _visualTransform.DOScale(_originalScale * 1.05f, 0.1f)
                .SetTarget(_visualTransform);
            AddTween(scaleTween);
        }
        else
        {
            _spriteRenderer.color = _originalColor;
            var scaleTween = _visualTransform.DOScale(_originalScale, 0.1f)
                .SetTarget(_visualTransform);
            AddTween(scaleTween);
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

    private void CacheComponents()
    {
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_visualTransform == null)
            _visualTransform = transform;

        // Создаем оверлей для увядания если его нет
        if (_witherOverlay == null)
        {
            CreateWitherOverlay();
        }
    }

    /// <summary>
    /// Анимация смены спрайта при росте
    /// </summary>
    private void AnimateSpriteChange(Sprite newSprite)
    {
        if (_visualTransform == null || _spriteRenderer == null) return;

        _isAnimating = true;

        // Уменьшаем масштаб
        var scaleTween1 = _visualTransform.DOScale(_originalScale * 0.8f, _growthAnimationDuration * 0.3f)
            .SetTarget(_visualTransform)
            .OnComplete(() =>
            {
                if (_spriteRenderer != null && newSprite != null)
                {
                    // Меняем спрайт
                    _spriteRenderer.sprite = newSprite;

                    if (_visualTransform != null)
                    {
                        // Возвращаем масштаб с эффектом bounce
                        var scaleTween2 = _visualTransform.DOScale(_originalScale * 1.1f, _growthAnimationDuration * 0.4f)
                            .SetEase(Ease.OutBack)
                            .SetTarget(_visualTransform)
                            .OnComplete(() =>
                            {
                                if (_visualTransform != null)
                                {
                                    var scaleTween3 = _visualTransform.DOScale(_originalScale, _growthAnimationDuration * 0.3f)
                                        .SetEase(Ease.OutBounce)
                                        .SetTarget(_visualTransform)
                                        .OnComplete(() => _isAnimating = false);
                                    AddTween(scaleTween3);
                                }
                            });
                        AddTween(scaleTween2);
                    }
                }
            });
        AddTween(scaleTween1);
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

            yield return new WaitForSeconds(1f);

            if (_visualTransform == null) break;

            var scaleTween2 = _visualTransform.DOScale(_originalScale, 1f)
                .SetEase(Ease.InOutSine)
                .SetTarget(_visualTransform);
            AddTween(scaleTween2);

            yield return new WaitForSeconds(1f);

            // Добавляем легкое свечение каждые 3 секунды
            if (Random.Range(0f, 1f) > 0.5f && _spriteRenderer != null)
            {
                var glowSequence = DOTween.Sequence();
                glowSequence.SetTarget(_spriteRenderer);

                var glow1 = _spriteRenderer.DOColor(_originalColor * 1.3f, 0.5f)
                    .SetTarget(_spriteRenderer);
                var glow2 = _spriteRenderer.DOColor(_originalColor, 0.5f)
                    .SetTarget(_spriteRenderer);

                glowSequence.Append(glow1);
                glowSequence.Append(glow2);

                AddTween(glowSequence);
            }

            yield return new WaitForSeconds(1f);
        }
    }

    /// <summary>
    /// Создает оверлей для эффекта увядания
    /// </summary>
    private void CreateWitherOverlay()
    {
        _witherOverlay = new GameObject("WitherOverlay");
        _witherOverlay.transform.SetParent(transform);
        _witherOverlay.transform.localPosition = Vector3.zero;
        _witherOverlay.transform.localScale = Vector3.one;

        var overlaySprite = _witherOverlay.AddComponent<SpriteRenderer>();
        overlaySprite.sortingOrder = _spriteRenderer.sortingOrder + 1;
        overlaySprite.color = new Color(0.2f, 0.1f, 0f, 0.3f);

        _witherOverlay.SetActive(false);
    }
}