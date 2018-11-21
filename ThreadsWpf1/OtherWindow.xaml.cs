using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ThreadsWpf1
{
    /// <summary>
    /// Interaction logic for OtherWindow.xaml
    /// </summary>
    public partial class OtherWindow : Window
    {
        public OtherWindow(int initCounter)
        {
            InitializeComponent();
            thread = Thread.CurrentThread;
            Title = Title + ": " + thread.Name;
            label.Content = "" + initCounter;
        }

        private Thread thread;
        public static bool? Result;

        public static void Create(string name, int initCounter)
        {
            Thread thread = new Thread(() =>
            {
                SynchronizationContext.SetSynchronizationContext(
                    new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));
                OtherWindow me = new OtherWindow(initCounter);
                Dispatcher.Run();
                Result = me.ShowDialog();
            });
            thread.Name = name;
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            label.Content = "" + (int.Parse((string)label.Content) + 1);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Dispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
        }
    }
}
