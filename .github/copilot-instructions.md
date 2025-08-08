# Инструкции для AI-агентов по коду PlantIncremental (Unity 2022.3, VContainer, UniRx)

Кратко: 2D инкрементальная игра про растения на сетке. Архитектура построена на DI (VContainer) и реактивных потоках (UniRx). Конфигурация — через ScriptableObject.

- Архитектура и роли
  - DI-композиция: `GameLifetimeScope` регистрирует настройки и сервисы: `ITimeService`, `IEconomyService`, `ISaveService`, `IGridService`, а также фабрику `IPlantFactory`. Точка входа — презентер `CoinsPresenter`.
  - Данные: `GameSettings` (размер сетки, интервалы), `PlantData` (спрайты стадий роста, время роста, цена, тип, редкость, префаб `PlantView`).
  - Сущности/представления: `IPlantEntity` (данные, прогресс роста, состояние), реализация `PlantEntity` пока заглушка. `PlantView`/`CellView` — MonoBehaviour-представления.

- DI (VContainer) — как подключать новое
  - Регистрируйте однотоновые сервисы в `GameLifetimeScope.Configure` через `builder.Register<IFoo, Foo>(Lifetime.Singleton)`.
  - Объекты сцены/инспектора передавайте через `builder.RegisterInstance(...)` (пример: `CoinsView`).
  - Для классов без MonoBehaviour используйте конструктор с `[Inject]`. Для созданных вручную экземпляров (см. фабрику) вызывайте `_resolver.Inject(entity)`.
  - Новые презентеры добавляйте в `UseEntryPoints(entryPoint.Add<YourPresenter>())`, реализуйте `IInitializable`/`IDisposable`.

- Реактивные паттерны (UniRx)
  - Состояния — через `ReactiveProperty<T>`/`IReadOnlyReactiveProperty<T>`; события — через `Subject<T>`/`IObservable<T>`.
  - Подписки освобождайте через `CompositeDisposable` и `.AddTo(_disposables)`. Пример: `Core/Presenters/CoinsPresenter.cs`.

- Сетка и растения
  - `GridService` инициализирует сетку из `GameSettings.GridSize`, генерирует тип почвы (`SoilType`), и предоставляет API: `TryPlantAt`, `TryHarvestAt`, `GetCell`, события `OnCellClicked` и `OnPlantHarvested` (вознаграждение в `PlantHarvestedEvent.Reward`).
  - Маппинг координат сетки в мир — `GridService.GridToWorldPosition` (шаг 1, смещение -2.5 для центровки 6x6).
  - Модификатор роста по почве — `GridCell.GetGrowthModifier()`.
  - Растения создаются через `PlantFactory`: инстанс префаба `PlantView`, затем создаётся `PlantEntity` и в него инжектятся зависимости.

- Экономика и лепестки
  - `IEconomyService`: реактивные монеты `Coins`, коллекция лепестков `PetalsCollection` с событиями `OnPetalChanged`, сериализация через `PetalsCollectionSaveData`.
  - Типы растений — `PlantType`, редкость — `PlantRarity`.

- Сохранения
  - `ISaveService`/`SaveService` — заглушка; `SaveData` пустой. В `GameSettings.AutoSaveInterval` задан желаемый интервал.
  - При реализации сохраняйте как минимум: монеты и лепестки (см. `EconomyService.GetPetalsSaveData()`), желательно — состояние сетки/растений.

- UI/Презентационный слой
  - `CoinsPresenter` подписывается на `IEconomyService.Coins` и обновляет `CoinsView` (TMP_Text). Новые UI-презентеры делайте по аналогии (реактивная подписка + DI).

- Принятые конвенции
  - Структура: `Assets/_Project/Scripts/{Core,Data,View,UI,DI}`. Интерфейсы — в `Core/Interfaces`, реализации — в `Core/Services|Factories|Entities|Presenters`.
  - Конфигурации и каталоги данных — ScriptableObject (`CreateAssetMenu`). Рантайм-объекты не создавайте через `new MonoBehaviour` — используйте префабы/фабрики.
  - Коммуникация между слоями — через сервисы и `IObservable`, а не прямые ссылки UI↔логика.

- Что важно при доработках (примеры задач)
  - Реализовать `PlantEntity`: хранить `GrowthProgress` [0..1] и `State` (`PlantState`), переключать спрайты в `PlantView`, учитывать `GridCell.GetGrowthModifier()` и `PlantData.GrowthTime`. Источник времени — `ITimeService` (например, тик раз в секунду `EverySecond`).
  - Реализовать `IPlantGrowthService` (если рост вынести из `PlantEntity`): регистрировать растения, завершение — триггерить состояние `FullyGrown`.
  - Соединить `GridService.OnPlantHarvested` с экономикой: подписка, добавление монет `EconomyService.AddCoins(event.Reward)` и/или лепестков по `PlantType`.
  - Доделать `SaveService`: периодический автосейв по `AutoSaveInterval` (можно через `ITimeService`/`Observable.Interval`), сериализация в JSON в `Application.persistentDataPath`.

- Сборка/запуск
  - Открыть проект в Unity 2022.3.25f1, дождаться импорта пакетов (`Packages/manifest.json` содержит UniRx и VContainer), запустить сцену из `EditorBuildSettings` (настройте при необходимости).
  - Тесты: включён пакет Test Framework, но тестов пока нет.

Сомнительные/неполные места для уточнения: начальная сцена и конфигурация сцены; где создаётся/поддерживается `GridView` и обработка ввода (`IGridInputHandler` отсутствует); финальная схема роста (`IPlantGrowthService`) и сохранения. Сообщите, если нужно зафиксировать конкретные решения — обновлю инструкции.
