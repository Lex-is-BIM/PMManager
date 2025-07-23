using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static PMManager.PMManager;

namespace PMManager
{
    /// <summary>
    /// Диалог выбора материалов
    /// </summary>
    public partial class MaterialSelectorDialog : Window
    {
        private ObservableCollection<Material> _materials;
        private ObservableCollection<LayeredMaterial> _layeredMaterials;
        private string _title;

        // Изменяем реализацию свойства
        public ObservableCollection<LayeredMaterial> LayeredMaterials
        {
            get { return _layeredMaterials; }
            set
            {
                _layeredMaterials = new ObservableCollection<LayeredMaterial>(value);
                LayeredMaterialsDataGrid.ItemsSource = _layeredMaterials;
                InitializeLayeredMaterialsSorting(); // Добавляем сортировку при установке
            }
        }

        public MaterialSelectorDialog(ObservableCollection<Material> materials, string title)
        {
            InitializeComponent();
            _materials = materials ?? throw new ArgumentNullException(nameof(materials));
            _title = title;
            Title = _title;
            MaterialsDataGrid.ItemsSource = _materials;

            // Инициализируем коллекцию многослойных материалов
            _layeredMaterials = new ObservableCollection<LayeredMaterial>();
            LayeredMaterialsDataGrid.ItemsSource = _layeredMaterials;

            // Добавляем инициализацию сортировки
            InitializeLayeredMaterialsSorting();
        }

        public IEnumerable<Material> SelectedMaterials =>
            _materials.Where(m => m.IsSelected);

        public IEnumerable<LayeredMaterial> SelectedLayeredMaterials =>
            _layeredMaterials.Where(m => m.IsSelected); 

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            SaveChanges();
            DialogResult = true;
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void SaveChanges()
        {
            MaterialsDataGrid.CommitEdit();
        }

        // Установить выделение для выбранного материала
        private void SetChecked_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialsDataGrid.SelectedItem is Material selectedMaterial)
            {
                selectedMaterial.IsSelected = true;
            }
        }

        // Снять выделение с выбранного материала
        private void UncheckChecked_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialsDataGrid.SelectedItem is Material selectedMaterial)
            {
                selectedMaterial.IsSelected = false;
            }
        }

        // Выделить все материалы
        private void CheckAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var material in _materials)
            {
                material.IsSelected = true;
            }
        }

        // Снять выделение со всех материалов
        private void UncheckAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var material in _materials)
            {
                material.IsSelected = false;
            }
        }

        // Установить выделение для выбранного многослойного материала
        private void SetLayeredChecked_Click(object sender, RoutedEventArgs e)
        {
            if (LayeredMaterialsDataGrid.SelectedItem is LayeredMaterial selectedMaterial)
            {
                selectedMaterial.IsSelected = true;
            }
        }

        // Снять выделение с выбранного многослойного материала
        private void UncheckLayeredChecked_Click(object sender, RoutedEventArgs e)
        {
            if (LayeredMaterialsDataGrid.SelectedItem is LayeredMaterial selectedMaterial)
            {
                selectedMaterial.IsSelected = false;
            }
        }

        // Выделить все многослойные материалы
        private void CheckAllLayered_Click(object sender, RoutedEventArgs e)
        {
            foreach (var material in _layeredMaterials)
            {
                material.IsSelected = true;
            }
        }

        // Снять выделение со всех многослойных материалов
        private void UncheckAllLayered_Click(object sender, RoutedEventArgs e)
        {
            foreach (var material in _layeredMaterials)
            {
                material.IsSelected = false;
            }
        }

        private void MaterialsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MaterialsDataGrid.CommitEdit();
        }

        private void MaterialsDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var column = MaterialsDataGrid.Columns.FirstOrDefault(c => c.SortMemberPath == "Name");
            if (column != null)
            {
                MaterialsDataGrid.Items.SortDescriptions.Clear();
                MaterialsDataGrid.Items.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                column.SortDirection = ListSortDirection.Ascending;
                MaterialsDataGrid.Items.Refresh();
            }
        }

        private void MaterialsDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            DataGridColumn column = e.Column;
            ListSortDirection newDir = ListSortDirection.Ascending;

            // Определяем направление сортировки
            if (column.SortDirection == ListSortDirection.Ascending)
                newDir = ListSortDirection.Descending;

            var view = CollectionViewSource.GetDefaultView(MaterialsDataGrid.ItemsSource);
            view.SortDescriptions.Clear();

            // Добавляем описание сортировки
            view.SortDescriptions.Add(
                new SortDescription(
                    column.SortMemberPath,
                    newDir));

            e.Handled = true; // Важно установить true, чтобы сортировка сработала
        }

        private void InitializeLayeredMaterialsSorting()
        {
            var column = LayeredMaterialsDataGrid.Columns.FirstOrDefault(c => c.SortMemberPath == "Name");
            if (column != null)
            {
                LayeredMaterialsDataGrid.Items.SortDescriptions.Clear();
                LayeredMaterialsDataGrid.Items.SortDescriptions.Add(
                    new SortDescription("Name", ListSortDirection.Ascending));
                column.SortDirection = ListSortDirection.Ascending;
                LayeredMaterialsDataGrid.Items.Refresh();
            }
        }

        private void LayeredMaterialsDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeLayeredMaterialsSorting();
        }
    }
}
