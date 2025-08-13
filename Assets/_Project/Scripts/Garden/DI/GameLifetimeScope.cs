using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [Header("Configurations")]
    [SerializeField] private GameSettings _gameSettings;
    [Header("Views")]
    [SerializeField] private CoinsView _coinsView;
    [SerializeField] private GridView _gridView;
    [SerializeField] private PetalsView _petalsView;
    [Header("Skill Tree")]
    [SerializeField] private Transform _skillsParent;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterGardenSystems(_gameSettings, _coinsView, _gridView, _petalsView);
        builder.RegisterSkillTreeSystems(autoInjectGameObjects, _skillsParent);
    }
} 