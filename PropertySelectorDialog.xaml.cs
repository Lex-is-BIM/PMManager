using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
using static PMManager.PMStateKeeper;
using static PMManager.PMStateKeeper.Property;

namespace PMManager
{
    /// <summary>
    /// Логика взаимодействия для PropertySelectorDialog.xaml
    /// </summary>
    public partial class PropertySelectorDialog : Window
    {
        private readonly ObservableCollection<Property> _properties;

        public PropertySelectorDialog(ObservableCollection<Property> properties)
        {
            InitializeComponent();
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            PropertiesData.ItemsSource = _properties;
        }

        public IEnumerable<Property> SelectedProperties =>
            _properties.Where(p => p.IsSelected);

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
            PropertiesData.CommitEdit();
        }

        // Установить выделенное
        private void SetChecked_Click(object sender, RoutedEventArgs e)
        {
            if (PropertiesData.SelectedItem is Property selectedItem)
            {
                selectedItem.IsSelected = true;
            }
        }

        // Снять выделенное
        private void UncheckChecked_Click(object sender, RoutedEventArgs e)
        {
            if (PropertiesData.SelectedItem is Property selectedItem)
            {
                selectedItem.IsSelected = false;
            }
        }

        // Установить все
        private void CheckAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _properties)
            {
                item.IsSelected = true;
            }
        }

        // Снять все
        private void UncheckAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _properties)
            {
                item.IsSelected = false;
            }
        }

        private void PropertiesData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Сохраняем изменения перед сменой выбора
            PropertiesData.CommitEdit();
        }
    }

    public class ArrayToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ObjectType[] objectTypes)
            {
                // Собираем только имена объектов через запятую
                return string.Join(", ", objectTypes.Select(obj => obj.Name));
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
