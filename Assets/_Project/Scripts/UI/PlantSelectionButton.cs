using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Кнопка выбора растения
/// </summary>
public class PlantSelectionButton : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image _plantIcon;
    [SerializeField] private TMP_Text _plantName;
    [SerializeField] private TMP_Text _costText;
    [SerializeField] private TMP_Text _growthTimeText;
    [SerializeField] private Image _rarityBorder;
    [SerializeField] private Button _button;

    [Header("Rarity Colors")]
    [SerializeField] private Color _commonColor = Color.gray;
    [SerializeField] private Color _uncommonColor = Color.green;
    [SerializeField] private Color _rareColor = Color.blue;
    [SerializeField] private Color _epicColor = new Color(0.5f, 0, 0.5f);
    [SerializeField] private Color _legendaryColor = new Color(1f, 0.5f, 0);

    private PlantData _plantData;

    private void Awake()
    {
        if (_button == null)
            _button = GetComponent<Button>();
    }

    /// <summary>
    /// Настраивает кнопку для растения
    /// </summary>
    public void Setup(PlantData plantData, Action onClick)
    {
        _plantData = plantData;

        // Настраиваем визуал
        if (plantData.GrowthStages != null && plantData.GrowthStages.Length > 0)
        {
            _plantIcon.sprite = plantData.GrowthStages[plantData.GrowthStages.Length - 1];
        }

        _plantName.text = plantData.DisplayName;
        _costText.text = $"{GetPlantCost(plantData)} <sprite=0>"; // sprite 0 = монеты
        _growthTimeText.text = $"{plantData.GrowthTime:F0}s";

        // Цвет редкости
        _rarityBorder.color = GetRarityColor(plantData.Rarity);

        // Подписка на клик
        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => onClick?.Invoke());
    }

    private int GetPlantCost(PlantData plantData)
    {
        return plantData.Rarity switch
        {
            PlantRarity.Common => 10,
            PlantRarity.Uncommon => 25,
            PlantRarity.Rare => 50,
            PlantRarity.Epic => 100,
            PlantRarity.Legendary => 500,
            _ => 10
        };
    }

    private Color GetRarityColor(PlantRarity rarity)
    {
        return rarity switch
        {
            PlantRarity.Common => _commonColor,
            PlantRarity.Uncommon => _uncommonColor,
            PlantRarity.Rare => _rareColor,
            PlantRarity.Epic => _epicColor,
            PlantRarity.Legendary => _legendaryColor,
            _ => _commonColor
        };
    }
}