
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

namespace PMManager
{
    public class PMManager
    {
        public sealed class PMStateKeeper : IPlugin
        {
            // 1. Константы и поля
            private string _pluginFolder;
            private const string MessageHeader = "PMStateKeeper";
            private ApplicationEventSource _appEvents;
            private readonly List<ActionEventSource> _eventSources = new();
            private readonly Renga.Application _app = new();
            private static readonly Dictionary<string, string> ObjectTypeNames = new()
            {
                // Object types
                ["96788994-b7fc-41d7-8a99-d674543e9237"] = "AngularDimension",
                ["00799249-1824-4ebd-bf93-40bb92efa9e6"] = "AssemblyInstance",
                ["4b41ccf8-c969-4c55-a1f2-cced9c164f07"] = "Axis",
                ["63478188-7c88-4a6d-b891-9725f04a5bc7"] = "Beam",
                ["d9ee2442-e807-42fb-8fe5-9dcfe543035d"] = "Column",
                ["2aabe3a4-a29e-4534-a9f5-0f070fee240c"] = "DiametralDimension",
                ["1cfba99c-01e7-4078-ae1a-3e2ff0673599"] = "Door",
                ["06cc88ee-9a67-4626-9c34-dde03c331a74"] = "Duct",
                ["47d0d93f-3c7b-4269-bf8a-de246e1724d0"] = "DuctAccessory",
                ["77ffca60-b20e-49f0-b42f-4fdc9b1c825b"] = "DuctFitting",
                ["96da9155-43c1-42b8-bba2-b4f61fa43acc"] = "ElectricDistributionBoard",
                ["e1e3bd66-2e13-4fa4-a9eb-677e03067c2f"] = "Element",
                ["8a49a9a8-a401-4ab1-8038-92093503c97a"] = "Elevation",
                ["5d2f3734-5a49-4504-90b1-0676f0f25da7"] = "Equipment",
                ["f5bd8bd8-39c1-47f8-8499-f673c580dfbe"] = "Floor",
                ["84b43087-d4a4-4cce-b34d-40e283d9e691"] = "Hatch",
                ["ecef8f90-0cf9-4494-98de-91242a2a9f5c"] = "Hole",
                ["f914251d-d5fa-48b2-b93b-074f442cbf3b"] = "IfcObject",
                ["6063816c-89ff-4c8f-a814-3be6cb94128e"] = "IsolatedFoundation",
                ["c3ce17ff-6f28-411f-b18d-74fe957b2ba8"] = "Level",
                ["793d3f7c-905d-4d85-a351-b152241dd2e7"] = "LightingFixture",
                ["02bbebe8-e28b-4ee5-8916-11b514a35dca"] = "Line3D",
                ["dc82ca1a-a0c3-4a1a-aefb-a7d720dd3a09"] = "LinearDimension",
                ["83de45e6-4793-49ec-8b9e-65a2438f36de"] = "LineElectricalCircuit",
                ["857a042d-7d3c-4715-9ebf-95e2e9648adf"] = "LinkedImage",
                ["67a0b42c-8c1e-47e8-b46e-78d8bb260de0"] = "LinkedModel",
                ["de4420ce-02b6-4b12-9cd7-9322118be8fe"] = "MechanicalEquipment",
                ["fc443d5a-b76c-45e5-b91c-520ef0896109"] = "Opening",
                ["838cc9f6-e3d8-4132-af6f-c58df0f8d037"] = "Pipe",
                ["41e2788a-49ed-487f-9ae1-55b6e09ae6e5"] = "PipeAccessory",
                ["d31dc2e3-808e-4987-8481-7f86665a07fc"] = "PipeFitting",
                ["62cf086e-5a39-4484-840c-ffa6a1c6e2b7"] = "Plate",
                ["b8c7155a-b462-4ff5-bc41-c9c17a9f48fa"] = "PlumbingFixture",
                ["377c2fda-9411-43ac-a6c6-0e3b520be721"] = "RadialDimension",
                ["a1aca786-78a4-4015-b412-9150baad71a9"] = "Railing",
                ["debde004-afcc-4da8-8dd0-4223ff836acd"] = "Ramp",
                ["9fabc932-590f-4068-89a8-ee6ee3d7cbbf"] = "Rebar",
                ["bac4470f-d560-4f57-a49e-faa5f6e5a279"] = "Roof",
                ["f1a805ff-573d-f46b-ffba-57f4bccaa6ed"] = "Room",
                ["8b323bee-3882-4744-8838-24f45df714a9"] = "Route",
                ["ce93e320-7167-4cd1-92a8-5e42d546066b"] = "RoutePoint",
                ["4166fd59-64c0-45ee-ae3b-49fae1257ef1"] = "Section",
                ["3f522f49-aee2-4d73-9866-9b07cf336a69"] = "Stair",
                ["da557027-f243-4331-bb5b-853abc437cd7"] = "TextObject",
                ["97675473-ca62-4ea4-bc6e-bb2ca57b7e67"] = "Undefined (object)",
                ["4329112a-6b65-48d9-9da8-abf1f8f36327"] = "Wall",
                ["d7dd0293-dd65-4229-a64c-8b528d4e226f"] = "WallFoundation",
                ["2b02b353-2ca5-4566-88bb-917ea8460174"] = "Window",
                ["b00d5c25-92a8-4409-a3b7-7c37ed792c06"] = "WiringAccessory",

                // Style types
                ["cb825bf3-15ae-4190-821c-8ad314951ada"] = "Assembly",
                ["cf2b8b04-f595-4432-98f4-8234c95adbdd"] = "BeamStyle",
                ["923bf334-2e0a-41a0-9bf9-dc598c38586f"] = "BuildingElementModel",
                ["be49a354-19b7-435a-8957-9ef8782630c2"] = "ColumnStyle",
                ["edae3ec8-2f1d-4d76-9aab-1c5c12dfda7d"] = "DisplayStyle",
                ["19d0649f-582a-488e-a52b-585c1151a5e4"] = "DoorStyle",
                ["e04d0118-5c58-4a7f-bf9c-3f729de1e559"] = "DrawingLink",
                ["6c671391-bfea-4e92-9753-8855c05640a0"] = "DuctAccessoryStyle",
                ["6c6821a0-ebb9-445b-84a2-ed9eb0938e4f"] = "DuctFittingStyle",
                ["a999f05a-d730-42e7-bfc8-e4433ebace78"] = "DuctStyle",
                ["fa7f1ae9-f4f4-4f95-b108-feea4d7efeb7"] = "ElectricalConductorStyle",
                ["33fb4b37-83f9-422a-81d4-640a152c619e"] = "ElectricCircuitLineStyle",
                ["861c0037-7797-43a9-96e7-833a7a2c6ea4"] = "ElectricDistributionBoardStyle",
                ["514a3ae7-f551-4d0f-b5ba-5d4f0ecf4e7a"] = "ElementStyle",
                ["a369ad70-c1fe-41dd-af3d-bd659ea5b360"] = "EquipmentStyle",
                ["c08a2259-6612-4cd4-919a-a09865cd6e3e"] = "HatchPatternStyle",
                ["83085c7b-16c4-473e-85bc-9aafa504ff7d"] = "HoleStyle",
                ["6a18e669-bdcf-442a-bc81-63c12da72aa2"] = "ImageLink",
                ["0f0adba0-5c06-46c0-9c8a-b9d69ef1251f"] = "LayeredMaterial",
                ["501768ff-fe9e-4fce-8337-22a841ac4868"] = "LayoutStyle",
                ["1f85f676-bb99-4a6f-9f72-1789f2f7b362"] = "LightingFixtureStyle",
                ["0abcb18f-0aaf-4509-bf89-5c5fad9d5d8b"] = "Material",
                ["d43c7509-a92c-4e32-bd2d-ba6dd8f5b7a1"] = "MechanicalEquipmentStyle",
                ["d769d1c4-8c32-40a8-a716-68bc9b6b5d3c"] = "ModelLink",
                ["3603ef07-e3a4-477e-9e72-2c8225c0a351"] = "PageFormat",
                ["a31cf7ca-f17b-422a-886a-7a8c362cd49a"] = "PipeAccessoryStyle",
                ["b1359bdc-f7ff-43a4-bca0-8d09bc974537"] = "PipeFittingStyle",
                ["9d6dffb9-4828-40d8-8529-bf5cd2b58c4e"] = "PipeStyle",
                ["9b60d6ad-3468-478e-94df-a535c5aeaa3e"] = "PlateStyle",
                ["344299f5-7d7f-43e2-b0a2-1db8e06e8ac8"] = "PlumbingFixtureStyle",
                ["8734b5cd-57fc-409e-aefe-1fdc449bcb5c"] = "Profile",
                ["608edb78-96f3-40a6-a0ec-71000105581b"] = "RebarStyle",
                ["b50f63fa-7f3a-4762-8ad9-324afc7fe2e8"] = "ReinforcementGrade",
                ["03a52558-573f-46c9-bea5-4760eb7fa485"] = "ReinforcementStyle",
                ["7ee13bd6-7c0a-47d3-adce-35b8e0dae28a"] = "ReinforcementUnit",
                ["e65c5fad-d4d3-4f43-bd01-b28d0eb95571"] = "SystemStyle",
                ["43f26eac-02b0-4639-8447-deee54fa1ff6"] = "TagStyle",
                ["f22ba8c7-a75d-43f4-bdce-6967aeac6118"] = "TextStyle",
                ["eafcc366-1483-44d5-881f-b4688d306da5"] = "Topic",
                ["df67fd2f-bbd3-4810-a132-1451769d5e51"] = "Undefined (style)",
                ["fac43446-031c-413e-9993-6e9cf9f2306a"] = "WindowStyle",
                ["a6e0ba72-acbd-4423-9afc-04d84a09211a"] = "WiringAccessoryStyle",

                //Project entity types
                ["165d15bc-fd8d-4bbb-b73c-56956d7cebf1"] = "Building",
                ["517a337a-58d5-46ff-81b8-65cf0389a191"] = "Insulation",
                ["ca526024-04a1-40c7-87fd-2e95c722cc50"] = "Layer",
                ["73c6bfe1-2c3d-4b16-b8d8-19d0eb4f64ef"] = "MaterialLayer",
                ["9bd80f5a-9448-48de-a9ab-935a946dab65"] = "Project",
                ["3ccb02f0-9f3c-4775-a01b-79dc2ded8d5e"] = "ReinforcementUnitTemplate",
                ["46c9576e-c65e-46b4-9fec-b952284d83c1"] = "Root",
                ["5cbf0016-32bc-4630-99ea-c7cc94dda8e3"] = "Schedule",
                ["a7dfe1e1-bf2c-4c4a-ba74-3f156b1bbf8f"] = "Sheet",
                ["56652d5b-536e-4ef6-a1cd-5ad69bb025ab"] = "Site",
                ["ed1f87a1-5c9c-4994-969d-6d3854571193"] = "Table"
            };

            // 2. Основные методы плагина
            public bool Initialize(string pluginFolder)
            {
                _pluginFolder = pluginFolder;
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

            private void OnProjectCreated() => InitializeProjectData();

            private void OnProjectOpened(string filePath) => InitializeProjectData();

            // 5. Методы для работы со свойствами
            private void RemoveProperties()
            {
                ShowMessage("Удаление", "Удаление свойств");
            }

            private void ExportProperties()
            {
                ShowMessage("Экспорт", "Экспорт свойств");
            }

            private void ImportProperties()
            {
                ShowMessage("Импорт", "Импорт свойств");
            }


            // 8. Вспомогательные методы
            private bool ConfirmAction(string title, string message)
            {
                return System.Windows.MessageBox.Show(message,
                    $"{MessageHeader}: {title}",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes;
            }

            private void ShowMessage(string title, string message)
            {
                System.Windows.MessageBox.Show(message,
                    $"{MessageHeader}: {title}",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
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
                    if (description.Type.ToString() == "PropertyType_Enumeration")
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

                    propertiesList.Add(new Property
                    {
                        Guid = propGuid,
                        Name = propertyManager.GetPropertyNameS(propGuid),
                        Type = description.Type.ToString(),
                        Enumerations = enumerations,
                        ObjectTypes = objectTypes.ToArray()
                    });
                }
                return propertiesList;
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
}
