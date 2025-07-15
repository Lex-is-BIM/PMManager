
using Microsoft.Win32;
using Renga;
using System.IO;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using System.Reflection;

namespace PMManager
{
    public class PMManager : IPlugin
    {
        // 1. Константы и поля
        private string _pluginFolder;
        private const string MessageHeader = "PMManager";
        private ApplicationEventSource _appEvents;
        private readonly List<ActionEventSource> _eventSources = new();
        private readonly Renga.Application _app = new();
        private static readonly Dictionary<string, string> ObjectTypeNames = new()
        {
            ["67a0b42c-8c1e-47e8-b46e-78d8bb260de0"] = "3D-модели",
            ["47d0d93f-3c7b-4269-bf8a-de246e1724d0"] = "Аксессуары воздуховода",
            ["41e2788a-49ed-487f-9ae1-55b6e09ae6e5"] = "Аксессуары трубопровода",
            ["3ccb02f0-9f3c-4775-a01b-79dc2ded8d5e"] = "Арматурные изделия",
            ["9fabc932-590f-4068-89a8-ee6ee3d7cbbf"] = "Арматурные стержни",
            ["63478188-7c88-4a6d-b891-9725f04a5bc7"] = "Балки",
            ["de4420ce-02b6-4b12-9cd7-9322118be8fe"] = "Вентиляционное оборудование",
            ["06cc88ee-9a67-4626-9c34-dde03c331a74"] = "Воздуховоды",
            ["1cfba99c-01e7-4078-ae1a-3e2ff0673599"] = "Двери",
            ["77ffca60-b20e-49f0-b42f-4fdc9b1c825b"] = "Детали воздуховода",
            ["d31dc2e3-808e-4987-8481-7f86665a07fc"] = "Детали трубопровода",
            ["2aabe3a4-a29e-4534-a9f5-0f070fee240c"] = "Диаметральные размеры",
            ["165d15bc-fd8d-4bbb-b73c-56956d7cebf1"] = "Здания",
            ["857a042d-7d3c-4715-9ebf-95e2e9648adf"] = "Изображения",
            ["517a337a-58d5-46ff-81b8-65cf0389a191"] = "Изоляция",
            ["d9ee2442-e807-42fb-8fe5-9dcfe543035d"] = "Колонны",
            ["bac4470f-d560-4f57-a49e-faa5f6e5a279"] = "Крыши",
            ["d7dd0293-dd65-4229-a64c-8b528d4e226f"] = "Ленточные фундаменты",
            ["3f522f49-aee2-4d73-9866-9b07cf336a69"] = "Лестницы",
            ["dc82ca1a-a0c3-4a1a-aefb-a7d720dd3a09"] = "Линейные размеры",
            ["02bbebe8-e28b-4ee5-8916-11b514a35dca"] = "Линии модели",
            ["0abcb18f-0aaf-4509-bf89-5c5fad9d5d8b"] = "Материалы",
            ["0f0adba0-5c06-46c0-9c8a-b9d69ef1251f"] = "Многослойные материалы",
            ["5d2f3734-5a49-4504-90b1-0676f0f25da7"] = "Оборудование",
            ["a1aca786-78a4-4015-b412-9150baad71a9"] = "Ограждения",
            ["2b02b353-2ca5-4566-88bb-917ea8460174"] = "Окна",
            ["793d3f7c-905d-4d85-a351-b152241dd2e7"] = "Осветительные приборы",
            ["4b41ccf8-c969-4c55-a1f2-cced9c164f07"] = "Оси",
            ["ecef8f90-0cf9-4494-98de-91242a2a9f5c"] = "Отверстия",
            ["debde004-afcc-4da8-8dd0-4223ff836acd"] = "Пандусы",
            ["f5bd8bd8-39c1-47f8-8499-f673c580dfbe"] = "Перекрытия",
            ["62cf086e-5a39-4484-840c-ffa6a1c6e2b7"] = "Пластины",
            ["f1a805ff-573d-f46b-ffba-57f4bccaa6ed"] = "Помещения",
            ["9bd80f5a-9448-48de-a9ab-935a946dab65"] = "Проекты",
            ["fc443d5a-b76c-45e5-b91c-520ef0896109"] = "Проёмы",
            ["377c2fda-9411-43ac-a6c6-0e3b520be721"] = "Радиальные размеры",
            ["eafcc366-1483-44d5-881f-b4688d306da5"] = "Разделы",
            ["4166fd59-64c0-45ee-ae3b-49fae1257ef1"] = "Разрезы",
            ["b8c7155a-b462-4ff5-bc41-c9c17a9f48fa"] = "Санитарно-техническое оборудование",
            ["00799249-1824-4ebd-bf93-40bb92efa9e6"] = "Сборки",
            ["73c6bfe1-2c3d-4b16-b8d8-19d0eb4f64ef"] = "Слои",
            ["5cbf0016-32bc-4630-99ea-c7cc94dda8e3"] = "Спецификации",
            ["4329112a-6b65-48d9-9da8-abf1f8f36327"] = "Стены",
            ["6c671391-bfea-4e92-9753-8855c05640a0"] = "Стили аксессуара воздуховода",
            ["a31cf7ca-f17b-422a-886a-7a8c362cd49a"] = "Стили аксессуара трубопровода",
            ["608edb78-96f3-40a6-a0ec-71000105581b"] = "Стили арматурного стержня",
            ["7ee13bd6-7c0a-47d3-adce-35b8e0dae28a"] = "Стили арматурных изделий",
            ["cf2b8b04-f595-4432-98f4-8234c95adbdd"] = "Стили балки",
            ["d43c7509-a92c-4e32-bd2d-ba6dd8f5b7a1"] = "Стили вентиляционного оборудования",
            ["a999f05a-d730-42e7-bfc8-e4433ebace78"] = "Стили воздуховода",
            ["19d0649f-582a-488e-a52b-585c1151a5e4"] = "Стили двери",
            ["6c6821a0-ebb9-445b-84a2-ed9eb0938e4f"] = "Стили детали воздуховода",
            ["b1359bdc-f7ff-43a4-bca0-8d09bc974537"] = "Стили детали трубопровода",
            ["be49a354-19b7-435a-8957-9ef8782630c2"] = "Стили колонны",
            ["a369ad70-c1fe-41dd-af3d-bd659ea5b360"] = "Стили оборудования",
            ["fac43446-031c-413e-9993-6e9cf9f2306a"] = "Стили окна",
            ["1f85f676-bb99-4a6f-9f72-1789f2f7b362"] = "Стили осветительного прибора",
            ["83085c7b-16c4-473e-85bc-9aafa504ff7d"] = "Стили отверстия",
            ["9b60d6ad-3468-478e-94df-a535c5aeaa3e"] = "Стили пластины",
            ["fa7f1ae9-f4f4-4f95-b108-feea4d7efeb7"] = "Стили проводника",
            ["344299f5-7d7f-43e2-b0a2-1db8e06e8ac8"] = "Стили санитарно-технического оборудования",
            ["cb825bf3-15ae-4190-821c-8ad314951ada"] = "Стили сборки",
            ["e65c5fad-d4d3-4f43-bd01-b28d0eb95571"] = "Стили системы",
            ["9d6dffb9-4828-40d8-8529-bf5cd2b58c4e"] = "Стили трубы",
            ["861c0037-7797-43a9-96e7-833a7a2c6ea4"] = "Стили электрического распределительного щита",
            ["33fb4b37-83f9-422a-81d4-640a152c619e"] = "Стили электрической линии",
            ["a6e0ba72-acbd-4423-9afc-04d84a09211a"] = "Стили электроустановочного изделия",
            ["514a3ae7-f551-4d0f-b5ba-5d4f0ecf4e7a"] = "Стиль элемента",
            ["6063816c-89ff-4c8f-a814-3be6cb94128e"] = "Столбчатые фундаменты",
            ["ed1f87a1-5c9c-4994-969d-6d3854571193"] = "Таблицы",
            ["da557027-f243-4331-bb5b-853abc437cd7"] = "Тексты модели",
            ["ce93e320-7167-4cd1-92a8-5e42d546066b"] = "Точки трассировки",
            ["8b323bee-3882-4744-8838-24f45df714a9"] = "Трассы",
            ["838cc9f6-e3d8-4132-af6f-c58df0f8d037"] = "Трубы",
            ["96788994-b7fc-41d7-8a99-d674543e9237"] = "Угловые размеры",
            ["c3ce17ff-6f28-411f-b18d-74fe957b2ba8"] = "Уровни",
            ["56652d5b-536e-4ef6-a1cd-5ad69bb025ab"] = "Участки",
            ["8a49a9a8-a401-4ab1-8038-92093503c97a"] = "Фасады",
            ["a7dfe1e1-bf2c-4c4a-ba74-3f156b1bbf8f"] = "Чертежи",
            ["84b43087-d4a4-4cce-b34d-40e283d9e691"] = "Штриховки модели",
            ["83de45e6-4793-49ec-8b9e-65a2438f36de"] = "Электрические линии",
            ["96da9155-43c1-42b8-bba2-b4f61fa43acc"] = "Электрические распределительные щиты",
            ["b00d5c25-92a8-4409-a3b7-7c37ed792c06"] = "Электроустановочные изделия",
            ["e1e3bd66-2e13-4fa4-a9eb-677e03067c2f"] = "Элементы"
        };
        private static readonly Dictionary<PropertyType, string> TypeNames = new()
        {
            { PropertyType.PropertyType_Undefined, "Неопределенный" },
            { PropertyType.PropertyType_Double, "Действительное число" },
            { PropertyType.PropertyType_String, "Строка" },
            { PropertyType.PropertyType_Angle, "Угол" },
            { PropertyType.PropertyType_Area, "Площадь" },
            { PropertyType.PropertyType_Boolean, "Логическое" },
            { PropertyType.PropertyType_Enumeration, "Перечисление" },
            { PropertyType.PropertyType_Integer, "Целое" },
            { PropertyType.PropertyType_Length, "Длина" },
            { PropertyType.PropertyType_Logical, "Логическое" },
            { PropertyType.PropertyType_Mass, "Масса" },
            { PropertyType.PropertyType_Volume, "Объем" }
        };

        // 2. Основные методы плагина
        public bool Initialize(string pluginFolder)
        {
            _pluginFolder = pluginFolder;
            AppDomain.CurrentDomain.AssemblyResolve += (sender, e) =>
            {
                var missingName = new AssemblyName(e.Name);
                var missingPath = Path.Combine(_pluginFolder, missingName.Name + ".dll");

                if (File.Exists(missingPath))
                    return Assembly.LoadFrom(missingPath);
                else
                    return null;
            };            
            InitializeEvents();
            InitializeUI();
            return true;
        }
        public void Stop()
        {
            CleanupEvents();
        }

        // 3. Методы работы с событиями
        private void InitializeEvents()
        {
            _appEvents = new ApplicationEventSource(_app);
            _appEvents.ProjectCreated += OnProjectCreated;
            _appEvents.ProjectOpened += OnProjectOpened;
        }
        private void CleanupEvents()
        {
            foreach (var eventSource in _eventSources)
                eventSource.Dispose();
            _eventSources.Clear();

            if (_appEvents != null)
            {
                _appEvents.ProjectCreated -= OnProjectCreated;
                _appEvents.ProjectOpened -= OnProjectOpened;
                _appEvents.Dispose();
            }
        }

        // 4. Методы пользовательского интерфейса
        private void InitializeUI()
        {
            IUI ui = _app.UI;

            CreatePropertiesPanelExtension(_app, ui);
            //CreateMaterialsPanelExtension(ui);
        } 

        private IAction CreateAction(IUI ui, string displayName, Action handler)
        {
            var action = ui.CreateAction();
            action.DisplayName = displayName;

            var events = new ActionEventSource(action);
            events.Triggered += (_, _) => handler();
            _eventSources.Add(events);

            return action;
        }

        private void CreatePropertiesPanelExtension(IApplication app, IUI ui)
        {
            var panelExtension = ui.CreateUIPanelExtension();

            IImage propertyImage = LoadPluginImage(ui, "i24_PM_Propertis.png");

            // Создаем действия
            IAction removePropertiesAction = CreateAction(ui, "Удалить свойства", RemoveProperties);
            IAction exportPropertiesAction = CreateAction(ui, "Экспортировать свойства", ExportProperties);
            IAction importPropertiesAction = CreateAction(ui, "Импортировать свойства", ImportProperties);

            // Создаем DropDownButton 
            var dropDownButton = CreateDropDownButton(
                ui,
                "Операции со свойствами",
                propertyImage,
                removePropertiesAction,
                exportPropertiesAction,
                importPropertiesAction);

            // Добавляем DropDownButton на панель
            panelExtension.AddDropDownButton(dropDownButton);

            ui.AddExtensionToPrimaryPanel(panelExtension);
        }

        // 5. Методы для работы со свойствами
        private void RemoveProperties()
        {
            // Сначала проверяем наличие свойств в проекте
            var allProperties = GetAllProperties(_app);
            if (allProperties.Count == 0)
            {
                ShowMessage("Нет свойств", "В проекте нет доступных свойств для удаления");
                return;
            }

            // Получаем список свойств с возможностью выбора
            var selectedProperties = SelectProperties(allProperties);

            // Проверяем на отмену
            if (selectedProperties == null)
            {
                return; // Просто выходим без сообщения
            }

            if (selectedProperties.Count == 0)
            {
                ShowMessage("Отмена удаления", "Нет выбранных свойств для удаления");
                return;
            }

            // Добавляем подтверждение удаления
            string message = $"Будет удалено {selectedProperties.Count} {GetNounForm(selectedProperties.Count, "свойство", "свойства", "свойств")}";
            if (!ConfirmAction("Подтверждение удаления", message))
            {
                return; // Пользователь отменил удаление
            }
            ExecuteOperation(() =>
            {
                foreach (var prop in selectedProperties)
                {
                    _app.Project.PropertyManager.UnregisterPropertyS(prop.Guid);
                }
            });

            //ShowMessage("Удаление завершено",
            //    $"Удалено {selectedProperties.Count} {GetNounForm(selectedProperties.Count, "свойство", "свойства", "свойств")}");
        }

        private void ExportProperties()
        {
            try
            {
                // Сначала проверяем наличие свойств в проекте
                var allProperties = GetAllProperties(_app);
                if (allProperties.Count == 0)
                {
                    ShowMessage("Нет свойств", "В проекте нет доступных свойств для экспорта");
                    return;
                }

                // Получаем список свойств с возможностью выбора
                var selectedProperties = SelectProperties(allProperties);

                // Проверяем на отмену
                if (selectedProperties == null)
                {
                    return; // Просто выходим без сообщения
                }

                if (selectedProperties.Count == 0)
                {
                    ShowMessage("Отмена экспорта", "Нет выбранных свойств для экспорта");
                    return;
                }

                // Сохраняем только выбранные свойства
                bool success = SavePropertiesToFile(selectedProperties);

                if (success)
                {
                    ShowMessage("Экспорт завершен",
                        $"Экспортировано {selectedProperties.Count} {GetNounForm(selectedProperties.Count, "свойство", "свойства", "свойств")}");
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Ошибка экспорта",
                    $"Ошибка при экспорте свойств: {ex.Message}");
            }
        }

        private void ImportProperties()
        {
            try
            {
                List<Property> loadedProperties = LoadPropertiesFromFile();

                if (loadedProperties == null)
                {
                    // Пользователь отменил диалог
                    return;
                }

                if (loadedProperties.Count == 0)
                {
                    ShowMessage("Пустой файл",
                        "Файл не содержит свойств или данные не распознаны.");
                    return;
                }

                // Получаем текущие свойства проекта
                List<Property> currentProperties = GetAllProperties(_app);

                // Фильтруем только те свойства, которых нет в проекте
                List<Property> newProperties = loadedProperties
                    .Where(loadedProp => !currentProperties.Any(currentProp => currentProp.Guid == loadedProp.Guid))
                    .ToList();

                if (newProperties.Count == 0)
                {
                    ShowMessage("Нет новых свойств",
                        "Все свойства из файла уже существуют в проекте.");
                    return;
                }

                // Показываем список новых свойств пользователю для выбора
                List<Property>? selectedProperties = SelectProperties(newProperties);

                // Добавляем проверку на отмену
                if (selectedProperties == null)
                {
                    return; // Просто выходим без сообщения
                }

                if (selectedProperties.Count == 0)
                {
                    ShowMessage("Отмена импорта",
                        "Не выбрано ни одного свойства для добавления.");
                    return;
                }

                // Создаем только выбранные свойства
                CreateProperties(_app, selectedProperties);

                // Показываем информацию о пропущенных свойствах
                // Считаем только те свойства, которые пользователь выбрал, но которые уже существуют
                int skippedCount = loadedProperties
                    .Where(prop => selectedProperties.Any(selected => selected.Guid == prop.Guid)
                                 && currentProperties.Any(current => current.Guid == prop.Guid))
                    .Count();

                string message = $"Импортировано {selectedProperties.Count} {GetNounForm(selectedProperties.Count, "новое свойство", "новых свойства", "новых свойств")}";

                if (skippedCount > 0)
                {
                    message += $"\nПропущено {skippedCount} {GetNounForm(skippedCount, "свойство", "свойства", "свойств")} " +
                               "(уже существуют в проекте)";
                }

                ShowMessage("Импорт завершен", message);
            }
            catch (Exception ex)
            {
                ShowMessage("Ошибка импорта",
                    $"Ошибка при импорте свойств: {ex.Message}");
            }
        }

        private void CreateProperties(IApplication app, List<Property> properties)
        {
            IProject project = app.Project;
            IPropertyManager propertyManager = project.PropertyManager;
            ExecuteOperation(() =>
            {
                foreach (var property in properties)
                {
                    PropertyType propertyType = (PropertyType)Enum.Parse(typeof(PropertyType), property.Type);
                    IPropertyDescription propertyDescription = propertyManager.CreatePropertyDescription(property.Name, propertyType);
                    if (property.Enumerations.Length > 0)
                    {
                        Array enumArray = property.Enumerations.ToArray();
                        propertyDescription.SetEnumerationItems(enumArray);
                    }

                    propertyManager.RegisterPropertyS2(property.Guid, propertyDescription);

                    if (property.ObjectTypes.Length > 0)
                    {
                        foreach (var objType in property.ObjectTypes)
                        {
                            propertyManager.AssignPropertyToTypeS(property.Guid, objType.Guid);
                            propertyManager.SetCSVExportFlagS(property.Guid, objType.Guid, objType.CSVExportFlag);
                            if (objType.Expression != null)
                            {
                                propertyManager.SetExpressionS(property.Guid, objType.Guid, objType.Expression);
                            }
                        }
                    }
                }
            });

        }

        // 8. Вспомогательные методы
        private bool ConfirmAction(string title, string message)
        {
            return System.Windows.MessageBox.Show(message,
                $"{MessageHeader}: {title}",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes;
        }

        public static void ShowMessage(string title, string message)
        {
            System.Windows.MessageBox.Show(
                message,
                $"{MessageHeader}: {title}",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information
            );
        }

        private string GetNounForm(int number, string one, string twoFour, string fiveMore)
        {
            int lastDigit = number % 10;
            int lastTwoDigits = number % 100;

            if (lastTwoDigits >= 11 && lastTwoDigits <= 14)
                return fiveMore;

            if (lastDigit == 1)
                return one;

            if (lastDigit >= 2 && lastDigit <= 4)
                return twoFour;

            return fiveMore;
        }

        private void ExecuteOperation(Action action)
        {
            var operation = _app.Project.CreateOperation();
            operation.Start();
            try
            {
                action();
                operation.Apply();
            }
            catch
            {
                operation.Rollback();
                throw;
            }
        }

        private IImage LoadPluginImage(IUI ui, string fileName)
        {
            IImage image = ui.CreateImage();
            string imagePath = Path.Combine(_pluginFolder, fileName);
            image.LoadFromFile(imagePath);
            return image;
        }

        private IDropDownButton CreateDropDownButton(
            IUI ui,
            string tooltip,
            IImage icon,
            params IAction[] actions)
        {
            var button = ui.CreateDropDownButton();
            button.ToolTip = tooltip;
            button.Icon = icon;

            foreach (var action in actions)
            {
                button.AddAction(action);
            }

            return button;
        }

        // 9. Методы загрузки данных
        private void InitializeProjectData()
        {
            //_existingProperties = GetAllProperties(_app);
            //_existingMaterials = GetAllMaterials(_app);
        }

        public List<Property>? SelectProperties(List<Property> properties)
        {
            ObservableCollection<Property> observableCollection = new ObservableCollection<Property>(properties);
            var dialog = new PropertySelectorDialog(observableCollection);

            // Если пользователь нажал "Отмена", возвращаем null
            if (dialog.ShowDialog() == true)
            {
                return dialog.SelectedProperties.ToList();
            }

            return null;
        }

        public static bool SavePropertiesToFile(
            List<Property> properties,
            string defaultFileName = "properties")
        {
            if (properties.Count == 0)
                return false;

            var saveFileDialog = new SaveFileDialog
            {
                FileName = defaultFileName,
                DefaultExt = ".json",
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                AddExtension = true
            };

            // Возвращаем false если пользователь отменил диалог
            if (saveFileDialog.ShowDialog() != true)
                return false;

            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                string jsonString = JsonSerializer.Serialize(properties, options);
                File.WriteAllText(saveFileDialog.FileName, jsonString);
                return true;
            }
            catch (Exception ex)
            {
                // Добавляем обработку исключений
                ShowMessage("Ошибка сохранения",
                    $"Не удалось сохранить файл: {ex.Message}");
                return false;
            }
        }

        private void OnProjectCreated() => InitializeProjectData();

        private void OnProjectOpened(string filePath) => InitializeProjectData();

        // 10. Статические методы получения данных

        private List<Property> GetAllProperties(IApplication app)
        {
            var propertyManager = app.Project.PropertyManager;
            int count = propertyManager.PropertyCount;
            var propertiesList = new List<Property>(count); // Задаем начальную емкость

            var objectTypeGuids = ObjectTypeNames.Keys.ToArray();

            for (int i = 0; i < count; i++)
            {
                string propGuid = propertyManager.GetPropertyIdS(i);
                IPropertyDescription description = propertyManager.GetPropertyDescription2S(propGuid);

                // Обработка перечислений
                string[] enumerations = Array.Empty<string>();
                if (description.Type == PropertyType.PropertyType_Enumeration)
                {
                    Array enumArray = description.GetEnumerationItems();
                    enumerations = enumArray.Cast<string>().ToArray();
                }

                // Сбор связанных типов объектов
                var objectTypes = new List<Property.ObjectType>();
                foreach (string objectTypeGuid in objectTypeGuids)
                {
                    if (propertyManager.IsPropertyAssignedToTypeS(propGuid, objectTypeGuid))
                    {
                        objectTypes.Add(new Property.ObjectType
                        {
                            Guid = objectTypeGuid,
                            Name = ObjectTypeNames[objectTypeGuid],
                            Expression = propertyManager.GetExpressionS(propGuid, objectTypeGuid),
                            CSVExportFlag = propertyManager.GetCSVExportFlagS(propGuid, objectTypeGuid)
                        });
                    }
                }

                // Используем словарь для получения корректного названия типа
                propertiesList.Add(new Property
                {
                    Guid = propGuid.ToLower(),
                    Name = propertyManager.GetPropertyNameS(propGuid),
                    Type = TypeNames[description.Type], // Здесь происходит преобразование
                    Enumerations = enumerations,
                    ObjectTypes = objectTypes.ToArray()
                });
            }
            return propertiesList;
        }

        private List<Property> LoadPropertiesFromFile()
        {
            var openFileDialog = new OpenFileDialog
            {
                DefaultExt = ".json",
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Выберите файл свойств для импорта"
            };

            if (openFileDialog.ShowDialog() != true)
                return null;

            try
            {
                string jsonString = File.ReadAllText(openFileDialog.FileName);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                return JsonSerializer.Deserialize<List<Property>>(jsonString, options);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Ошибка чтения файла {openFileDialog.FileName}: {ex.Message}", ex);
            }
        }

        // Классы
        public sealed class Property : IEquatable<Property>, INotifyPropertyChanged
        {
            private bool _isSelected;

            // Все свойства только для чтения (кроме IsSelected)
            public required string Guid { get; init; }
            public required string Name { get; init; }
            public string Type { get; init; } = string.Empty;
            public string[] Enumerations { get; init; } = Array.Empty<string>();
            public ObjectType[] ObjectTypes { get; init; } = Array.Empty<ObjectType>();

            // Единственное редактируемое свойство
            [JsonIgnore]
            public bool IsSelected
            {
                get => _isSelected;
                set
                {
                    if (_isSelected != value)
                    {
                        _isSelected = value;
                        OnPropertyChanged();
                    }
                }
            }

            // Методы сравнения
            public override bool Equals(object? obj) =>
                obj is Property other && Guid == other.Guid;

            public bool Equals(Property? other) =>
                other is not null && Guid == other.Guid;

            public override int GetHashCode() => Guid.GetHashCode();

            public static bool operator ==(Property? left, Property? right) =>
                Equals(left, right);

            public static bool operator !=(Property? left, Property? right) =>
                !Equals(left, right);

            // Реализация INotifyPropertyChanged
            public event PropertyChangedEventHandler? PropertyChanged;

            protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            // Вложенный класс ObjectType без изменений
            public sealed class ObjectType
            {
                public required string Guid { get; init; }
                public required string Name { get; init; }
                public string? Expression { get; init; }
                public bool CSVExportFlag { get; init; }
            }
        }
    }
}
