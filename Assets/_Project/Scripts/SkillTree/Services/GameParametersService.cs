using System.Collections.Generic;
using UnityEngine;
using VContainer;

/// <summary>
/// Реализация сервиса параметров
/// </summary>
public class GameParametersService : IGameParametersService
{
    private readonly Dictionary<string, float> _baseValues = new();
    private readonly Dictionary<string, float> _additiveModifiers = new();
    private readonly Dictionary<string, float> _multiplicativeModifiers = new();
    
    [Inject]
    public GameParametersService()
    {
        // Инициализируем базовые значения
        _baseValues["growth_speed"] = 1f;
        _baseValues["coin_multiplier"] = 1f;
        _baseValues["petal_chance"] = 0.1f;
        _baseValues["mutation_chance"] = 0.05f;
        _baseValues["harvest_bonus"] = 1f;
    }
    
    public float GetParameter(string key)
    {
        if (!_baseValues.ContainsKey(key))
            return 0f;
        
        float baseValue = _baseValues[key];
        float additive = _additiveModifiers.GetValueOrDefault(key, 0f);
        float multiplier = _multiplicativeModifiers.GetValueOrDefault(key, 1f);
        
        return (baseValue + additive) * multiplier;
    }
    
    public void ApplyModifier(string key, float value, bool isMultiplier)
    {
        if (!_baseValues.ContainsKey(key))
        {
            Debug.LogWarning($"Unknown parameter: {key}");
            return;
        }
        
        if (isMultiplier)
        {
            _multiplicativeModifiers[key] = _multiplicativeModifiers.GetValueOrDefault(key, 1f) * value;
        }
        else
        {
            _additiveModifiers[key] = _additiveModifiers.GetValueOrDefault(key, 0f) + value;
        }
    }
    
    public void ResetModifiers()
    {
        _additiveModifiers.Clear();
        _multiplicativeModifiers.Clear();
    }
}