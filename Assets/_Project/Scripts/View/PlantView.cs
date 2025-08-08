using System.Collections;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Визуальное представление растения с анимациями и эффектами
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
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
    
    private void Awake()
    {
        CacheComponents();
        _originalScale = transform.localScale;
        _originalColor = _spriteRenderer.color;
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
    /// Анимация смены спрайта при росте
    /// </summary>
    private void AnimateSpriteChange(Sprite newSprite)
    {
        _isAnimating = true;
        
        // Уменьшаем масштаб
        _visualTransform.DOScale(_originalScale * 0.8f, _growthAnimationDuration * 0.3f)
            .OnComplete(() =>
            {
                // Меняем спрайт
                _spriteRenderer.sprite = newSprite;
                
                // Возвращаем масштаб с эффектом bounce
                _visualTransform.DOScale(_originalScale * 1.1f, _growthAnimationDuration * 0.4f)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() =>
                    {
                        _visualTransform.DOScale(_originalScale, _growthAnimationDuration * 0.3f)
                            .SetEase(Ease.OutBounce)
                            .OnComplete(() => _isAnimating = false);
                    });
            });
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
        
        // Анимация "радости"
        var sequence = DOTween.Sequence();
        sequence.Append(_visualTransform.DORotate(new Vector3(0, 0, -10), 0.1f));
        sequence.Append(_visualTransform.DORotate(new Vector3(0, 0, 10), 0.2f));
        sequence.Append(_visualTransform.DORotate(new Vector3(0, 0, -5), 0.1f));
        sequence.Append(_visualTransform.DORotate(new Vector3(0, 0, 0), 0.1f));
        
        // Пульсация масштаба
        sequence.Join(_visualTransform.DOScale(_originalScale * 1.3f, 0.3f)
            .SetEase(Ease.OutElastic));
        sequence.Append(_visualTransform.DOScale(_originalScale, 0.2f));
        
        // Вспышка цвета
        sequence.Join(_spriteRenderer.DOColor(Color.white, 0.1f));
        sequence.Append(_spriteRenderer.DOColor(_originalColor, 0.3f));
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
        
        // Подпрыгивание и исчезновение
        var sequence = DOTween.Sequence();
        sequence.Append(_visualTransform.DOJump(
            _visualTransform.position + Vector3.up * 0.5f, 
            1f, 1, _harvestAnimationDuration));
        sequence.Join(_spriteRenderer.DOFade(0, _harvestAnimationDuration));
        sequence.Join(_visualTransform.DOScale(Vector3.zero, _harvestAnimationDuration)
            .SetEase(Ease.InBack));
        
        sequence.OnComplete(() => gameObject.SetActive(false));
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
    /// Периодическая анимация для пассивного эффекта
    /// </summary>
    private IEnumerator PassiveEffectAnimation()
    {
        while (gameObject.activeInHierarchy)
        {
            // Мягкая пульсация
            _visualTransform.DOScale(_originalScale * 1.05f, 1f)
                .SetEase(Ease.InOutSine);
            
            yield return new WaitForSeconds(1f);
            
            _visualTransform.DOScale(_originalScale, 1f)
                .SetEase(Ease.InOutSine);
            
            yield return new WaitForSeconds(1f);
            
            // Добавляем легкое свечение каждые 3 секунды
            if (Random.Range(0f, 1f) > 0.5f)
            {
                var glowSequence = DOTween.Sequence();
                glowSequence.Append(_spriteRenderer.DOColor(_originalColor * 1.3f, 0.5f));
                glowSequence.Append(_spriteRenderer.DOColor(_originalColor, 0.5f));
            }
            
            yield return new WaitForSeconds(1f);
        }
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
        // Анимация увядания
        var sequence = DOTween.Sequence();
        
        // Растение "опускается"
        sequence.Append(_visualTransform.DORotate(new Vector3(0, 0, -15), 0.5f));
        sequence.Join(_visualTransform.DOScale(_originalScale * 0.8f, 0.5f));
        
        // Цвет меняется на коричневый
        sequence.Join(_spriteRenderer.DOColor(new Color(0.4f, 0.3f, 0.2f, 0.8f), 1f));
        
        // Активируем оверлей
        sequence.OnComplete(() => SetWitheredVisual());
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
        
        // Возвращаем оригинальный масштаб
        _visualTransform.DOScale(_originalScale, 0.3f);
    }
    
    /// <summary>
    /// Эффект полива растения
    /// </summary>
    public void PlayWaterEffect()
    {
        // Капли воды (если есть система частиц)
        // Временная анимация "встряхивания от радости"
        var sequence = DOTween.Sequence();
        sequence.Append(_visualTransform.DOShakeRotation(0.3f, 10f, 10));
        sequence.Join(_visualTransform.DOScale(_originalScale * 1.1f, 0.15f));
        sequence.Append(_visualTransform.DOScale(_originalScale, 0.15f));
    }
    
    /// <summary>
    /// Подсветка при наведении
    /// </summary>
    public void SetHighlight(bool active)
    {
        if (active)
        {
            _spriteRenderer.color = _originalColor * 1.2f;
            _visualTransform.DOScale(_originalScale * 1.05f, 0.1f);
        }
        else
        {
            _spriteRenderer.color = _originalColor;
            _visualTransform.DOScale(_originalScale, 0.1f);
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
    
    private void OnDestroy()
    {
        // Очищаем все твины
        DOTween.Kill(transform);
        DOTween.Kill(_spriteRenderer);
        
        if (_passiveEffectCoroutine != null)
        {
            StopCoroutine(_passiveEffectCoroutine);
        }
    }
    
    private void OnMouseEnter()
    {
        SetHighlight(true);
    }
    
    private void OnMouseExit()
    {
        SetHighlight(false);
    }
}