using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace OS_LAB_2_Client
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int currentRowIndex;
        bool IsClick = false;
        Client client;
        private BindingList<Tech> toData;

        public MainWindow()
        {
            InitializeComponent();

            client = new Client();
            toData = new BindingList<Tech>();
        }

        private void buttonSend_Click(object sender, RoutedEventArgs e)
        {
            if (toData.Count != 0)
            {
                if (client.IsConnected == false)
                {
                    client.serverConnect();
                }
                if (client.checkConnection())
                {
                    for (int i = 0; i < toData.Count; i++)
                    {
                        client.sendMessage(buildJSON(i));
                        Thread.Sleep(20);
                    }
                }
                else MessageBox.Show("Соедение с сервером не установленно!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else { MessageBox.Show("Таблица пуста!", "Предупреждение!", MessageBoxButton.OK, MessageBoxImage.Warning); }
        }
        private string buildJSON(int index)
        {
            var technik = new Tech();
            {
                technik.name = toData[index].name;
                technik.marka = toData[index].marka;
                technik.tech = toData[index].tech;
                technik.serNum = toData[index].serNum;
                technik.faultClient = toData[index].faultClient;
                technik.date = toData[index].date;
            }
            string jsonData = JsonConvert.SerializeObject(technik);
            Console.WriteLine(index + ". " + jsonData);
            return jsonData;
        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            currentRowIndex = tableView.Items.IndexOf(tableView.CurrentItem);

            textBoxFIO.Text = toData[currentRowIndex].name;
            textBoxMarks.Text = toData[currentRowIndex].marka;
            textBoxFault.Text = toData[currentRowIndex].faultClient;
            textBoxSerNum.Text = toData[currentRowIndex].serNum;
            comboBoxTechnic.Text = toData[currentRowIndex].tech;
            pickerDate.Text = toData[currentRowIndex].date;

            IsClick = true;
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            if (isEmptyData())
            {
                if (IsClick)
                {
                    toData[currentRowIndex].name = textBoxFIO.Text;
                    toData[currentRowIndex].marka = textBoxMarks.Text;
                    toData[currentRowIndex].faultClient = textBoxFault.Text;
                    toData[currentRowIndex].serNum = textBoxSerNum.Text;
                    toData[currentRowIndex].tech = comboBoxTechnic.Text;
                    toData[currentRowIndex].date = pickerDate.Text;

                    tableView.ItemsSource = null;
                    tableView.ItemsSource = toData;

                    IsClick = false;
                    clearData();
                }
                else
                {
                    newRecordTable();
                    clearData();
                    tableView.ItemsSource = toData;
                }
            }
            else { MessageBox.Show("Не все поля были заполненны", "Предупреждение!", MessageBoxButton.OK, MessageBoxImage.Warning); }

        }

        private void buttonRemove_Click(object sender, RoutedEventArgs e)
        {
            if (IsClick)
            {
                toData.RemoveAt(currentRowIndex);
                tableView.ItemsSource = null;
                tableView.ItemsSource = toData;

                IsClick = false;
                clearData();
            }
            else { MessageBox.Show("Ячейка не выбрана", "Предупреждение!", MessageBoxButton.OK, MessageBoxImage.Warning); }
        }

            private void newRecordTable()
        {
            toData.Add(new Tech()
            {
                name = textBoxFIO.Text,
                marka = textBoxMarks.Text,
                tech = comboBoxTechnic.Text,
                serNum = textBoxSerNum.Text,
                faultClient = textBoxFault.Text,
                date = pickerDate.Text
            });
        }

        private void clearData()
        {
            textBoxFIO.Text = "";
            textBoxMarks.Text = "";
            textBoxFault.Text = "";
            textBoxSerNum.Text = "";
            comboBoxTechnic.Text = "";
            pickerDate.Text = "";
        }

        private bool isEmptyData()
        {
            if(textBoxFIO.Text == "" && textBoxMarks.Text == "" && comboBoxTechnic.Text == "" &&
                textBoxSerNum.Text == "" && textBoxFault.Text == "")
            {
                return false;
            }
            else
            {
                if (pickerDate.Text == "")
                {
                    pickerDate.SelectedDate = DateTime.Now;
                }
                return true;
            }
        }
    }
}
