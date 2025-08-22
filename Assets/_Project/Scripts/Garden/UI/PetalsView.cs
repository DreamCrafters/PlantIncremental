using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PetalsView : MonoBehaviour
{
    private readonly Dictionary<PlantType, int> _petals = new();

    [SerializeField] private TMP_Text _text;
    [SerializeField] private string _prefix = "Petals: ";

    public void UpdatePetals(PlantType plantType, int petals)
    {
        _petals[plantType] = petals;
        UpdateView();
    }

    private void UpdateView()
    {
        string displayText = _prefix;
        foreach (var kvp in _petals)
        {
            displayText += $"\n{kvp.Key}: {kvp.Value} ";
        }
        _text.text = displayText;
    }
}
