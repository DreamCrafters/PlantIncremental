using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

/// <summary>
/// Фабрика навыков
/// </summary>
public class SkillFactory : ISkillFactory, ISkillRegistry
{
    private readonly IObjectResolver _resolver;
    private readonly Dictionary<SkillType, Type> _skillTypes = new();
    
    [Inject]
    public SkillFactory(IObjectResolver resolver)
    {
        _resolver = resolver;
        
        RegisterSkillType<ParameterSkill>(SkillType.Parameter);
        RegisterSkillType<UnlockSkill>(SkillType.Unlock);
    }
    
    public void RegisterSkillType<T>(SkillType type) where T : ISkill
    {
        _skillTypes[type] = typeof(T);
    }
    
    public ISkill CreateSkill(SkillData data)
    {
        if (!_skillTypes.TryGetValue(data.Type, out var skillType))
        {
            Debug.LogError($"Unknown skill type: {data.Type}");
            return null;
        }
        
        var skill = Activator.CreateInstance(skillType, data, _resolver.Resolve<IEconomyService>()) as ISkill;
        _resolver.Inject(skill);
        return skill;
    }
}