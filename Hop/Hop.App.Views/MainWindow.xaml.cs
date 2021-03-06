﻿using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using TrayIcon = System.Windows.Forms.NotifyIcon;
using ContextMenuStrip = System.Windows.Forms.ContextMenuStrip;
using ToolStripMenuItem = System.Windows.Forms.ToolStripMenuItem;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace Hop.App.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly TraversalRequest MoveFocusRequest = new TraversalRequest(FocusNavigationDirection.Next);

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HOTKEY_ID = 9000;
        private const uint MOD_ALT = 0x0001;
        private const uint VK_C = 0x48;
        private const int WM_HOTKEY = 0x0312;
        private IntPtr _windowHandle;
        private HwndSource _source;
        private TrayIcon TrayIcon = new TrayIcon(); 

        public MainWindow()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            this.QueryTextBox.PreviewKeyDown += HandleKeyDown;
            this.ItemsListView.PreviewKeyDown += FocusTextBox;
            this.ItemsListView.TargetUpdated += ItemsUpdated;

            var uri = new Uri("pack://application:,,,/Hop.App.Views;component/Hop.ico");
            var stream = Application.GetResourceStream(uri);
            this.TrayIcon.Icon = new Icon(stream.Stream);
            var contextMenuStrip = new ContextMenuStrip();
            var openItem = new ToolStripMenuItem
            {
                Name = "OpenMenuItem",
                Text = "Open Hop"
            };
            openItem.Click += OpenHop;
            contextMenuStrip.Items.Add(openItem);
            var exitItem = new ToolStripMenuItem
            {
                Name = "Exit",
                Text = "Exit"
            };
            exitItem.Click += Exit;
            contextMenuStrip.Items.Add(exitItem);
            this.TrayIcon.ContextMenuStrip = contextMenuStrip;
            this.TrayIcon.Visible = true;
        }

        private void Exit(object sender, EventArgs e) => this.Close();

        private void OpenHop(object sender, EventArgs e)
        {
            this.Visibility = Visibility.Visible;
        }

        private void ItemsUpdated(object sender, DataTransferEventArgs e)
        {
            this.ItemsListView.SelectedIndex = 0;
            this.ItemsListView.UpdateLayout();
            var item = this.ItemsListView.ItemContainerGenerator.ContainerFromIndex(0) as ListViewItem;
            if (item != null) item.Focus();
            else this.QueryTextBox.Focus();
        }

        private void FocusTextBox(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                this.Visibility = Visibility.Collapsed;
            }
            if (e.Key != Key.Down && e.Key != Key.Up && e.Key != Key.Tab && e.Key != Key.LeftShift && e.Key != Key.Return)
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


        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            _windowHandle = new WindowInteropHelper(this).Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);

            RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_ALT, VK_C);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HOTKEY_ID:
                            int vkey = (((int)lParam >> 16) & 0xFFFF);
                            if (vkey == VK_C)
                            {
                                if (this.Visibility == Visibility.Collapsed)
                                {
                                    this.Visibility = Visibility.Visible;
                                    this.QueryTextBox.Focus();
                                }
                                else this.Visibility = Visibility.Collapsed;
                            }
                            handled = true;
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        protected override void OnClosed(EventArgs e)
        {
            TrayIcon.Dispose();
            _source.RemoveHook(HwndHook);
            UnregisterHotKey(_windowHandle, HOTKEY_ID);
            base.OnClosed(e);
        }
    }

    [ValueConversion(typeof(System.Drawing.Image), typeof(BitmapSource))]
    public class ImageToBitmapSourceConverter : IValueConverter
    {
        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteObject(IntPtr value);

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            System.Drawing.Image myImage = (System.Drawing.Image)value;

            var bitmap = new Bitmap(myImage);
            IntPtr bmpPt = bitmap.GetHbitmap();
            BitmapSource bitmapSource =
             Imaging.CreateBitmapSourceFromHBitmap(
                   bmpPt,
                   IntPtr.Zero,
                   Int32Rect.Empty,
                   BitmapSizeOptions.FromEmptyOptions());

            //freeze bitmapSource and clear memory to avoid memory leaks
            bitmapSource.Freeze();
            DeleteObject(bmpPt);

            return bitmapSource;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
