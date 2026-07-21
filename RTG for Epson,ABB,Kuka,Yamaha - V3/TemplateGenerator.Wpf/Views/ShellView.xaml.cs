using MvvmCross.Platforms.Wpf.Views;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TemplateGenerator.Core.ViewModels;


namespace TemplateGenerator.Wpf.Views
{
    /// <summary>
    /// Interaction logic for FirstUserControlView.xaml
    /// </summary>
    public partial class ShellView : MvxWpfView
    {
        public ShellView()
        {
            InitializeComponent();
        }

        protected ShellViewModel ShellViewModel
        {
            get { return ViewModel as ShellViewModel; }
        }

        private void GenerateClick(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();    //Browse Button
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            string path = dialog.SelectedPath;
            ShellViewModel.GenerateProject(path);
        }

        private void ImportProjectClick(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK) return;
            ShellViewModel.ImportProject(dialog.SelectedPath);
        }

        private void UpdateProjectClick(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK) return;
            ShellViewModel.UpdateProject(dialog.SelectedPath);
        }

    }
}
