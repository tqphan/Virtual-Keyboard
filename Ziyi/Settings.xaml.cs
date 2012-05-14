using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Ziyi
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
            this.Visibility = System.Windows.Visibility.Hidden;
            this.comboBox1.ItemsSource = System.Enum.GetValues(typeof(System.Windows.Input.MouseButton));
            this.comboBox1.SelectedItem = Properties.Settings.Default.PrimaryInputTrigger;
            this.comboBox1.SelectionChanged += new SelectionChangedEventHandler(comboBox1_SelectionChanged);

        }

        void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.PrimaryInputTrigger = (MouseButton)this.comboBox1.SelectedItem;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true; 
            this.Visibility = System.Windows.Visibility.Hidden; 
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Hidden; 
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Hidden; 
            Properties.Settings.Default.Save(); 
            
        }

        private void openThemeButton_Click(object sender, RoutedEventArgs e)
        {
            var main = Application.Current.MainWindow as MainWindow;
            main.LoadAnotherTheme();
        }
    }
}
