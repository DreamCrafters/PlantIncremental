using System.Linq;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class SkillTreeLifetimeScope : LifetimeScope
{
    [SerializeField] private Transform _skillsParent;

    protected override void Configure(IContainerBuilder builder)
    {
        autoInjectGameObjects.AddRange(_skillsParent.GetComponentsInChildren<Skill>().Select(skill => skill.gameObject));
        
        builder.Register<IEconomyService, EconomyService>(Lifetime.Singleton);
    }
}
