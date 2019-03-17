using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
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

namespace Hop.App.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly TraversalRequest MoveFocusRequest = new TraversalRequest(FocusNavigationDirection.Next);

        public MainWindow()
        {
            InitializeComponent();
            this.QueryTextBox.PreviewKeyDown += HandleKeyDown;
            this.ItemsListView.PreviewKeyDown += FocusTextBox;
            this.ItemsListView.TargetUpdated += ItemsUpdated;
        }

        private void ItemsUpdated(object sender, DataTransferEventArgs e)
        {
            this.ItemsListView.SelectedIndex = 0;
            this.ItemsListView.UpdateLayout();
            var item = this.ItemsListView.ItemContainerGenerator.ContainerFromIndex(0) as ListViewItem;
            item?.Focus();
        }

        private void FocusTextBox(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Down && e.Key != Key.Up && e.Key != Key.Tab && e.Key != Key.LeftShift)
            {
                MoveFocus();
                this.QueryTextBox.RaiseEvent(e);
            }
            if (e.Key == Key.Up && this.ItemsListView.SelectedIndex == 0)
            {
                e.Handled = true;
                MoveFocus();
            }
        }

        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                e.Handled = true;
                MoveFocus();
            }
        }

        private void MoveFocus() => (Keyboard.FocusedElement as UIElement)?.MoveFocus(MoveFocusRequest);
    }
}
