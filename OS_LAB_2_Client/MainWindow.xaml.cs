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
        int count = 0;  // Количество записей в таблице

        public MainWindow()
        {
            InitializeComponent();

            client = new Client();
            toData = new BindingList<Tech>();

            Thread brainBot = new Thread(new ThreadStart(listenUpdate));    
            brainBot.Start();
        }

        private void listenUpdate()
        {
            while(true)
            {
                Thread.Sleep(3000);
    
                    Dispatcher.BeginInvoke(new Action(delegate
                    {
                        tableUpdate();
                    }));
            }
        }

        private void tableUpdate()
        {
            if (client.lastMessage.Count != 0)
            {
                string inTable = client.lastMessage[0];
                if (inTable == "Empty") { }
                else
                {
                    if (inTable.Length > 0)
                    {
                        if (toData.Count > 0)
                        {
                            tableView.ItemsSource = null;
                        }

                        for (int index = 0; index < client.lastMessage.Count; index++)
                        {
                            var deserMessage = JsonConvert.DeserializeObject<Tech>(client.lastMessage[index]);
                            var technik = new Tech();
                            {
                                technik.id = deserMessage.id;
                                technik.name = deserMessage.name;
                                technik.marka = deserMessage.marka;
                                technik.tech = deserMessage.tech;
                                technik.serNum = deserMessage.serNum;
                                technik.faultClient = deserMessage.faultClient;
                                technik.date = deserMessage.date;
                            }

                            int ind = Convert.ToInt32(deserMessage.id) - 1;
                            if (count <= ind)
                            {
                                toData.Add(technik);
                                count++;
                            }
                            else
                            {
                                toData[ind] = deserMessage;
                            }
                        }
                        tableView.ItemsSource = toData;
                        client.isUpdate = false;
                        client.lastMessage.Clear();
                    }
                }
            }
        }

        private void Send()
        {
            if (toData.Count != 0)
            {
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
                technik.id = toData[index].id;
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
                    toData[currentRowIndex].id = (currentRowIndex + 1).ToString();
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
                    count++;
                    newRecordTable();
                    clearData();
                    tableView.ItemsSource = toData;
                }
                Send();
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
                count--;
                clearData();
                Send();
            }
            else { MessageBox.Show("Ячейка не выбрана", "Предупреждение!", MessageBoxButton.OK, MessageBoxImage.Warning); }
        }

        private void newRecordTable()
        {
            toData.Add(new Tech()
            {
                id = count.ToString(),
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
            //pickerDate.Text = "";
        }

        private bool isEmptyData()
        {
            if (textBoxFIO.Text == "" && textBoxMarks.Text == "" && comboBoxTechnic.Text == "" &&
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (client.IsConnected == false)
            {
                client.serverConnect();
                Thread.Sleep(150);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            client.disconnect();
            Environment.Exit(0);
        }
    }
}
