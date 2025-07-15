using System;
using System.Collections.Generic;
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

namespace PMManager
{
    /// <summary>
    /// Логика взаимодействия для CustomMessageBoxDouble.xaml
    /// </summary>
    public partial class CustomMessageBoxDouble : Window
    {
        public CustomMessageBoxDouble(string title, string message)
        {
            InitializeComponent();
            Title = title;
            MessageTextBlock.Text = message;
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
