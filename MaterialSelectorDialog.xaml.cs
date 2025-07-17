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
        private readonly ObservableCollection<Material> _materials;
        private readonly string _title;

        public MaterialSelectorDialog(ObservableCollection<Material> materials, string title)
        {
            InitializeComponent();
            _materials = materials ?? throw new ArgumentNullException(nameof(materials));
            _title = title;
            Title = _title;
            MaterialsDataGrid.ItemsSource = _materials;
        }

        /// <summary>
        /// Возвращает выбранные материалы
        /// </summary>
        public IEnumerable<Material> SelectedMaterials =>
            _materials.Where(m => m.IsSelected);

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
            e.Handled = false;
        }
    }
}
