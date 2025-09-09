using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

public abstract class Skill : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Inject] private readonly IEconomyService _economyService;

    [SerializeField] private string _name;
    [SerializeField, TextArea] private string _description;
    [SerializeField, Min(1)] private int _maxLevel = 1;
    [SerializeField] private int _coinsUpgradeCost;
    [SerializeField] private List<PetalsUpgradeCost> _petalsUpgradeCosts = new();
    [SerializeField] private List<Skill> _children = new();

    private int _currentLevel = 0;

    public IEconomyService EconomyService => _economyService;
    public IReadOnlyList<Skill> Children => _children;
    public string Name => _name;
    public string Description => _description;
    public int MaxLevel => _maxLevel;
    public int CurrentLevel => _currentLevel;

    public event Action OnHover;
    public event Action OnHoverExit;
    public event Action OnLock;
    public event Action OnUnlock;
    public event Action<int> OnUpgrade;

    private void Start()
    {
        OnUpgrade?.Invoke(_currentLevel);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        print($"Clicked on skill: {_name}");
        TryUpgrade();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        print($"Hovered on skill: {_name}");
        OnHover?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnHoverExit?.Invoke();
    }

    public void LockAll(bool isRoot = true)
    {
        _children.ForEach(child => child.LockAll(false));

        if (isRoot)
        {
            Unlock();
        }
        else
        {
            Lock();
        }
    }

    public void Unlock()
    {
        OnUnlock?.Invoke();
    }

    public void Lock()
    {
        OnLock?.Invoke();
    }

    public bool TryUpgrade()
    {
        if (CanUpgrade())
        {
            if (_economyService.TrySpendCoins(_coinsUpgradeCost) == false)
            {
                throw new Exception("CanUpgrade returned true, but couldn't spend coins.");
            }

            foreach (var petalsCost in _petalsUpgradeCosts)
            {
                if (_economyService.TrySpendPetals(petalsCost.PlantType, petalsCost.Cost) == false)
                {
                    throw new Exception("CanUpgrade returned true, but couldn't spend petals.");
                }
            }

            if (_currentLevel == 0)
            {
                _children.ForEach(child => child.Unlock());
            }

            _currentLevel++;
            OnUpgrade?.Invoke(_currentLevel);
            UpgradeHandle();
            return true;
        }

        return false;
    }
    
    public bool CanUpgrade()
    {
        foreach (var petalsCost in _petalsUpgradeCosts)
        {
            if (_economyService.GetPetalsAmount(petalsCost.PlantType) < petalsCost.Cost)
            {
                return false;
            }
        }

        bool hasEnoughCoins = _economyService.Coins.Value >= _coinsUpgradeCost;

        return _currentLevel < _maxLevel && hasEnoughCoins;
    }

    protected abstract void UpgradeHandle();

    public struct PetalsUpgradeCost
    {
        public PlantType PlantType;
        public int Cost;
    }
}
