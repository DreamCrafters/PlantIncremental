/// <summary>
/// Фабрика для создания механик растений
/// </summary>
public interface IPlantMechanicsFactory
{
    /// <summary>
    /// Создает экземпляр механик для указанного растения
    /// </summary>
    /// <param name="plantData">Данные растения с конфигурацией механик</param>
    /// <returns>Экземпляр механик растения</returns>
    IPlantMechanics CreateMechanics(PlantData plantData);
}