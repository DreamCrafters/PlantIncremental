using TMPro;
using UnityEngine;

public class CoinsView : MonoBehaviour
{
    [SerializeField] private TMP_Text _coinsText;
    [SerializeField] private string _prefix = "Coins: ";
    
    public void UpdateCoins(int coins)
    {
        _coinsText.text = _prefix + coins.ToString();
    }
}