using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

public abstract class Skill : MonoBehaviour, IPointerClickHandler
{
    [Inject] private readonly IEconomyService _economyService;

    [SerializeField] private int _maxLevel;
    [SerializeField] private int _coinsUpgradeCost;
    [SerializeField] private List<PetalsUpgradeCost> _petalsUpgradeCosts = new();
    [SerializeField] private List<Skill> _children = new();

    private int _currentLevel = 0;

    public IReadOnlyList<Skill> Children => _children;

    public event Action OnLock;
    public event Action OnUnlock;
    public event Action<int> OnUpgrade;

    public void OnPointerClick(PointerEventData eventData)
    {
        TryUpgrade();
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

    protected abstract void UpgradeHandle();

    private bool CanUpgrade()
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

    public struct PetalsUpgradeCost
    {
        public PlantType PlantType;
        public int Cost;
    }
}
