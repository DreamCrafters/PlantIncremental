using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [Header("Configurations")]
    [SerializeField] private GameSettings _gameSettings;
    [SerializeField] private SkillTreeConfig _skillTreeConfig;
    [Header("Views")]
    [SerializeField] private CoinsView _coinsView;
    [SerializeField] private GridView _gridView;
    [SerializeField] private PetalsView _petalsView;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterSkillTreeSystem(_skillTreeConfig);
        builder.RegisterGardenSystems(_gameSettings, _coinsView, _gridView, _petalsView);
    }
}