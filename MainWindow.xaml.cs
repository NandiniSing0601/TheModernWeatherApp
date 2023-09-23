using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
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
using TheAccuWeatherApp;

namespace TheModernWeatherApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly string apiKey = "487793a039fe48d398a165813232209";
        private string cityName;
        private DateTime _now;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public DateTime Now
        {
            get
            {
                return _now;
            }
            set
            {
                _now = value;
                OnPropertyChanged(nameof(Now));
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            lblDigitalClock.Visibility = Visibility.Collapsed;
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += (sender, args) =>
            {
                Now = DateTime.Now;
            };
            timer.Start();
        }

        private async void btnGetWeather_Click(object sender, RoutedEventArgs e)
        {
            cityName = txtCityName.Text.Trim();
            if (String.IsNullOrEmpty(cityName))
            {
                MessageBox.Show("Please enter a city name", "City Name is missing", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string apiUrl = $"http://api.weatherapi.com/v1/current.json?key={apiKey}&q={cityName}";
            try
            {
                HttpWebRequest webRequest = WebRequest.CreateHttp(apiUrl);
                webRequest.Method = "GET";

                using (WebResponse response = await webRequest.GetResponseAsync())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        if (responseStream != null)
                        {
                            using (MemoryStream memoryStream = new MemoryStream())
                            {
                                await responseStream.CopyToAsync(memoryStream);
                                byte[] responseData = memoryStream.ToArray();
                                string jsonResponse = Encoding.UTF8.GetString(responseData);
                                WeatherData weatherData = JsonConvert.DeserializeObject<WeatherData>(jsonResponse);
                                DisplayWeatherData(weatherData);
                            }
                        }
                    }
                }
            }
            catch
            (WebException ex)
            {
                MessageBox.Show("An Error occured while fetcing the data : " + ex.Message);
            }
            lblDigitalClock.Visibility = Visibility.Visible;
            txtCityName.Text = "";
        }

        private void DisplayWeatherData(WeatherData? weatherData)
        {
            lblCityName.Content = weatherData.Location.Name;
            lblCondition.Content = weatherData.Current.Condition.Text;
            lblTemperature.Content = weatherData.Current.TempC + "°C";
            lblHumidity.Content = weatherData.Current.Humidity + "%";

            BitmapImage weatherIcon = new BitmapImage();
            weatherIcon.BeginInit();
            weatherIcon.UriSource = new Uri("http:" + weatherData.Current.Condition.Icon);
            weatherIcon.EndInit();
            imgWeatherIcon.Source = weatherIcon;

            lblWindSpeed.Content = weatherData.Current.WindKph + " km/h";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

    }
}


