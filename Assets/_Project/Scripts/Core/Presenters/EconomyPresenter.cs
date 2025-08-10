using System;
using UniRx;
using VContainer;
using VContainer.Unity;

public class EconomyPresenter : IInitializable, IDisposable
{
    private readonly IEconomyService _economy;
    private readonly CoinsView _view;
    private readonly PetalsView _petalsView;
    private readonly CompositeDisposable _disposables = new();

    [Inject]
    public EconomyPresenter(IEconomyService economy, CoinsView view, PetalsView petalsView)
    {
        _economy = economy;
        _view = view;
        _petalsView = petalsView;
    }

    public void Initialize()
    {
        _economy.Coins
            .Subscribe(coins => _view.UpdateCoins(coins))
            .AddTo(_disposables);

        _economy.OnPetalChanged
            .Subscribe(petalType => _petalsView.UpdatePetals(petalType, _economy.GetPetalsAmount(petalType)))
            .AddTo(_disposables);

        foreach (var petal in _economy.PetalsCollection.GetAllPetals())
        {
            _petalsView.UpdatePetals(petal.Type, petal.Amount);
        }
    }

    public void Dispose() => _disposables.Dispose();
}