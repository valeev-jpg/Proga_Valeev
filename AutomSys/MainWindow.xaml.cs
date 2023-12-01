using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
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
using System.Windows.Threading;
using AutomSys.Items;

namespace AutomSys
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly EventsController _eventsController;
        private DispatcherTimer _timer;

        public bool SimulationStarted = false;

        public List<Automate> Automates;

        public MainWindow()
        {
            InitializeComponent();
            EventGrid.ItemsSource = EventsController.Events;
            _eventsController = new EventsController(EventGrid);
            SetTime();
            _timer = new DispatcherTimer();

            Automates = new List<Automate>
            {
                new Automate((Image)this.FindName("Автомат1"), Label1, AutomateStatuses.Green, _eventsController),
                new Automate((Image)this.FindName("Автомат2"), Label2, AutomateStatuses.Green, _eventsController),
                new Automate((Image)this.FindName("Автомат3"), Label3, AutomateStatuses.Green, _eventsController)
            };
 
            Grid.ItemsSource = Automates;
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _timer.Tick += new EventHandler(DispatcherTimer_Tick);
            _timer.Interval = new TimeSpan(0, 0, 1);
            _timer.Start();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            SimulationStarted = !SimulationStarted;

            if (SimulationStarted)
            {
                Task.Run(() =>
                {
                    SimulateController.Simulate(Automates, _eventsController);
                });

                _eventsController.AddEvent(sender, "Симуляция запущена");
            }

            StartSimulationButton.IsEnabled = !SimulationStarted;
            await Task.Delay(5000);
            StopSimulationButton.IsEnabled = SimulationStarted;
            Brigada.IsEnabled = true;
        }

        private async void Button_Click5(object sender, RoutedEventArgs e)
        {
            SimulationStarted = !SimulationStarted;

            if (!SimulationStarted)
            {

                SimulateController.StopSimulation();


                _eventsController.AddEvent(sender, "Симуляция остановлена");
            }

            StopSimulationButton.IsEnabled = SimulationStarted;
            await Task.Delay(3000);
            StartSimulationButton.IsEnabled = !SimulationStarted;
            Brigada.IsEnabled = false;
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var image = (Image)sender;

            var automate = Automate.FindImageBySource(Grid, image);

            automate.FixAutomate();
        }

        private async void DispatcherTimer_Tick(object? sender, EventArgs e)
        {
            SetTime();

            Grid.Items.Refresh();
        }

        private void DataGridCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var dataGridCellTarget = (DataGridCell)sender;
            if(dataGridCellTarget?.Content is TextBlock textbox && textbox.Text == AutomateStatuses.Red)
            {
                (dataGridCellTarget.DataContext as Automate)?.FixAutomate();
            }
        }

        public void SetTime()
        {
            TimeText.Content = "Текущее время: " + DateTime.Now.ToString();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            FillAutomate(Automates[0]);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            FillAutomate(Automates[1]);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            FillAutomate(Automates[2]);
        }

        public void FillAutomate(Automate automate)
        {
            //if (automate.ResourcesCount > 0) return;
            automate.FillItems(); 
            Grid.Items.Refresh();
        }

        private void OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var displayName = GetPropertyDisplayName(e.PropertyDescriptor);

            if (!string.IsNullOrEmpty(displayName))
            {
                e.Column.Header = displayName;
            }

        }

        public static string GetPropertyDisplayName(object descriptor)
        {
            var pd = descriptor as PropertyDescriptor;

            if (pd != null)
            {
                // Check for DisplayName attribute and set the column header accordingly
                var displayName = pd.Attributes[typeof(DisplayNameAttribute)] as DisplayNameAttribute;

                if (displayName != null && displayName != DisplayNameAttribute.Default)
                {
                    return displayName.DisplayName;
                }

            }
            else
            {
                var pi = descriptor as PropertyInfo;

                if (pi != null)
                {
                    // Check for DisplayName attribute and set the column header accordingly
                    Object[] attributes = pi.GetCustomAttributes(typeof(DisplayNameAttribute), true);
                    for (int i = 0; i < attributes.Length; ++i)
                    {
                        var displayName = attributes[i] as DisplayNameAttribute;
                        if (displayName != null && displayName != DisplayNameAttribute.Default)
                        {
                            return displayName.DisplayName;
                        }
                    }
                }
            }

            return null;
        }

        private async void Brigada_Click(object sender, RoutedEventArgs e)
        {
            await FixAllAutomates();
        }

        private async Task FixAllAutomates()
        {
            foreach (var automate in Automates)
            {
                automate.FixAutomate(); // Вызываем метод чинки для каждого автомата
                await Task.Delay(1000); // Добавляем задержку между чинкой каждого автомата (для демонстрационных целей)
            }
            _eventsController.AddEvent("все автоматы починены");
        }
        
    }
}
