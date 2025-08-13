using System;
using UnityEngine;

[Serializable]
public struct PetalData
{
    [SerializeField] private PlantType _type;
    [SerializeField] private int _amount;

    public readonly PlantType Type => _type;
    public readonly int Amount => _amount;

    public PetalData(PlantType type, int amount)
    {
        _type = type;
        _amount = amount;
    }

    public readonly PetalData WithAmount(int newAmount)
    {
        return new PetalData(_type, newAmount);
    }

    public readonly PetalData AddAmount(int additionalAmount)
    {
        additionalAmount = Mathf.Max(0, additionalAmount);
        return new PetalData(_type, _amount + additionalAmount);
    }

    public readonly PetalData SubtractAmount(int subtractAmount)
    {
        subtractAmount = Mathf.Max(0, subtractAmount);
        return new PetalData(_type, Mathf.Max(0, _amount - subtractAmount));
    }

    public override readonly string ToString()
    {
        return $"{_type}: {_amount}";
    }
}
