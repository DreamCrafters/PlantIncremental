using System;
using UniRx;
using VContainer;
using VContainer.Unity;

public class CoinsPresenter : IInitializable, IDisposable
{
    private readonly IEconomyService _economy;
    private readonly CoinsView _view;
    private readonly CompositeDisposable _disposables = new();
    
    [Inject]
    public CoinsPresenter(IEconomyService economy, CoinsView view)
    {
        _economy = economy;
        _view = view;
    }
    
    public void Initialize()
    {
        _economy.Coins
            .Subscribe(coins => _view.UpdateCoins(coins))
            .AddTo(_disposables);
    }
    
    public void Dispose() => _disposables.Dispose();
}