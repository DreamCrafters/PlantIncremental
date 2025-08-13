using TMPro;
using UnityEngine;

public class SkillDescription : MonoBehaviour
{
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _descriptionText;

    public void Show(string name, string description)
    {
        _nameText.text = name;
        _descriptionText.text = description;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
