using System;
using System.Collections.Generic;
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

namespace PdfConverter
{
    /// <summary>
    /// Interaction logic for LoadingControl.xaml
    /// </summary>
    public partial class LoadingControl : Window, IDisposable
    {
        public Action<object> Worker { get; set; }
        public LoadingControl()
        {
            InitializeComponent();
        }
        public LoadingControl(Action<object> worker)
        {
            InitializeComponent();
            Worker = worker ?? throw new ArgumentNullException();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Worker != null)
            {
                Task.Factory.StartNew(Worker, null).ContinueWith(x => { Close(); }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }
        public void Dispose()
        {

        }
    }
}
