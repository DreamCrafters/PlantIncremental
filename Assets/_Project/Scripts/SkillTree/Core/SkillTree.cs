using UnityEngine;

public class SkillTree : MonoBehaviour
{
    [SerializeField] private Skill _rootSkill;

    private void Start()
    {
        _rootSkill.LockAll();
    }
}
