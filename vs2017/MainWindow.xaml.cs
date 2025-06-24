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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Reflection;

namespace DeviceHostLinkMemo
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<DataItem> DataItems { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            DataItems = new ObservableCollection<DataItem>();

            DataItems.CollectionChanged += OnCollectionChanged;

            string iniPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Param.ini");

            if (File.Exists(iniPath))
            {
                LoadItemList(iniPath);
            }
            else
            {

                for (int i = 0; i < 16; i++)
                {
                    DataItems.Add(new DataItem
                    {
                        SettingNo = i,
                        SignalUnitNo = 1,
                        SignalChannelNo = i%4,
                        SignalMemoryNo = i % 2,
                        SignalFrequency = 1000,
                        SignalLength = 20,
                        TrigerFrequency = 12,
                        TrigerBaseUnit = 1,
                        TrigerShiftTime = (int)(0.5*i)*30,
                        SeriesName = $"Series_{i:D2}"
                    });
                }

            }
            DataContext = this;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (DataItems.Count > 16)
            {
                MessageBox.Show("Cannot add more than 16 rows.", "Limit", MessageBoxButton.OK, MessageBoxImage.Warning);
                DataItems.RemoveAt(DataItems.Count - 1);
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var iniPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Param.ini");
            LoadItemList(iniPath);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var iniPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Param.ini");
            SaveItemList(iniPath);
        }


        private void LoadItemList(string iniFilePath)
        {
            if (!File.Exists(iniFilePath))
            {
                MessageBox.Show($"INI file not found: {iniFilePath}", "File Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var lines = File.ReadAllLines(iniFilePath, Encoding.UTF8);
                DataItems.Clear();

                foreach (var line in lines.Take(16))
                {
                    var tokens = line.Split(';');
                    if (tokens.Length < 10) continue;

                    DataItems.Add(new DataItem
                    {
                        SettingNo = int.Parse(tokens[0]),
                        SignalUnitNo = int.Parse(tokens[1]),
                        SignalChannelNo = int.Parse(tokens[2]),
                        SignalMemoryNo = int.Parse(tokens[3]),
                        SignalFrequency = int.Parse(tokens[4]),
                        SignalLength = int.Parse(tokens[5]),
                        TrigerFrequency = int.Parse(tokens[6]),
                        TrigerBaseUnit = int.Parse(tokens[7]),
                        TrigerShiftTime = int.Parse(tokens[8]),
                        SeriesName = tokens[9]
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load INI file.\n{ex.Message}", "Read Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveItemList(string iniFilePath)
        {
            try
            {
                var lines = DataItems.Select(item =>
                    $"{item.SettingNo};{item.SignalUnitNo};{item.SignalChannelNo};{item.SignalMemoryNo};" +
                    $"{item.SignalFrequency};{item.SignalLength};{item.TrigerFrequency};{item.TrigerBaseUnit};" +
                    $"{item.TrigerShiftTime};{item.SeriesName}");

                File.WriteAllLines(iniFilePath, lines, Encoding.UTF8);
                MessageBox.Show("Successfully saved.", "Save", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save INI file.\n{ex.Message}", "Write Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


    }
    public class DataItem : IDataErrorInfo
    {
        public int SettingNo { get; set; }
        public int SignalUnitNo { get; set; }
        public int SignalChannelNo { get; set; }
        public int SignalMemoryNo { get; set; }
        public int SignalFrequency { get; set; }
        public int SignalLength { get; set; }
        public int TrigerFrequency { get; set; }
        public int TrigerBaseUnit { get; set; }
        public int TrigerShiftTime { get; set; }
        public string SeriesName { get; set; } = string.Empty;

        public string Error => null;

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(SettingNo):
                        if (SettingNo < 0 || SettingNo > 16) return "Please specify a value between 0 and 16."; break;
                    case nameof(SignalUnitNo):
                        if (SignalUnitNo < 0 || SignalUnitNo > 48) return "Please specify a value between 0 and 48."; break;
                    case nameof(SignalChannelNo):
                        if (SignalChannelNo < 0 || SignalChannelNo > 3) return "Please specify a value between 0 and 3."; break;
                    case nameof(SignalMemoryNo):
                        if (SignalMemoryNo < 0 || SignalMemoryNo > 4) return "Please specify a value between 0 and 4."; break;
                    case nameof(SignalFrequency):
                        if (SignalFrequency < 0 || SignalFrequency > 9999) return "Please specify a value between 0 and 9999."; break;
                    case nameof(SignalLength):
                        if (SignalLength < 0 || SignalLength > 999) return "Please specify a value between 0 and 999."; break;
                    case nameof(TrigerShiftTime):
                        if (TrigerShiftTime < 0 || TrigerShiftTime > 9999) return "Please specify a value between 0 and 9999."; break;
                    case nameof(TrigerBaseUnit):
                        if (TrigerBaseUnit < 0 || TrigerBaseUnit > 2) return "Please specify a value between 0 and 2."; break;
                    case nameof(TrigerFrequency):
                        int[] validValues = TrigerBaseUnit == 0
                            ? new[] { 1, 2, 3, 4, 6, 8, 12, 24 }
                            : new[] { 1, 2, 3, 4, 5, 6, 10, 12, 15, 20, 30, 60 };
                        if (!validValues.Contains(TrigerFrequency)) return $"Invalid value for TrigerBaseUnit = {TrigerBaseUnit}."; break;
                    case nameof(SeriesName):
                        if (System.Text.Encoding.UTF8.GetByteCount(SeriesName) >= 64)
                            return "SeriesName must be less than 64 bytes including the null terminator."; break;
                }
                return null;
            }
        }

    }
}
