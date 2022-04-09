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
using PhotoRenamer.Core;

namespace PhotoRenamer.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly RenameProcessor _renameProcessor;
        public MainWindow()
        {
            InitializeComponent();
            _renameProcessor = new RenameProcessor();
            _renameProcessor.Notify += DisplayMessage;

        }

        private void DisplayMessage(string filename)
        {
            throw new NotImplementedException();
        }

        private void BtPath_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtRename_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
