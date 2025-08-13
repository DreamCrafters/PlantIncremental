/// <summary>
/// Сервис игровых параметров (для применения эффектов навыков)
/// </summary>
public interface IGameParametersService
{
    float GetParameter(string key);
    void ApplyModifier(string key, float value, bool isMultiplier);
    void ResetModifiers();
}