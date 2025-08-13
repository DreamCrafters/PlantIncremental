using System.Collections.Generic;

/// <summary>
/// Данные для сохранения прогресса
/// </summary>
[System.Serializable]
public class SkillTreeSaveData
{
    [System.Serializable]
    public struct SkillProgress
    {
        public string SkillId;
        public int Level;
    }
    
    public List<SkillProgress> Skills = new();
}