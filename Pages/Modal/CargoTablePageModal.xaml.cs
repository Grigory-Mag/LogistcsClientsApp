﻿using ApiService;
using Grpc.Core;
using LogisticsClientsApp.Localizations;
using LogisticsClientsApp.Pages.Tables;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LogisticsClientsApp.Pages.Modal
{
    /// <summary>
    /// Логика взаимодействия для CargoTablePageModal.xaml
    /// </summary>
    public partial class CargoTablePageModal : UserControl
    {
        private static CargoTablePageModal instance;
        public StartWindow startWindow;
        public CargoObject data = new CargoObject();
        public ListCargoType cargoTypes;
        public byte mode = 0;
        string text = "Обновить";

        public CargoTablePageModal()
        {
            InitializeComponent();
        }

        public void SetMode(byte mode)
        {
            this.mode = mode;
            if (mode == 0)
            {
                UpdateButton.Content = "обновить";
                text = "Обновить";
            }                
            else
            {
                UpdateButton.Content = "добавить";
                text = "Добавить";
            }                
        }

        private void ModalPageControl_Loaded(object sender, RoutedEventArgs e)
        {
            startWindow = (StartWindow)Window.GetWindow(this);
            SetLinkedData();
            Locale locale = new Locale(startWindow.selectedLocale);
            locale.SetLocale(this);
        }

        public void CloseAnimation()
        {
            startWindow = (StartWindow)Window.GetWindow(this);
            var tablePage = (TablePage)startWindow.MainFrameK.Content;
            tablePage.MainPanel.Opacity = 1;
            tablePage.MainPanel.IsEnabled = true;

            Storyboard sb = Resources["CloseModal"] as Storyboard;
            sb.Begin(ModalPageControl);
        }

        public void UpdateDisplayedData(CargoObject cargoObject)
        {
            this.data = cargoObject;
            WeightTextBox.Text = data.Weight.ToString();
            VolumeTextBox.Text = data.Volume.ToString();
            NameTextBox.Text = data.Name.ToString();
            PriceTextBox.Text = data.Price.ToString();
            ConstraintsTextBox.Text = data.Constraints.ToString();
            if (startWindow != null)
                SetLinkedData();

        }

        public async void SetLinkedData()
        {
            cargoTypes = await startWindow.client.GetListCargoTypesAsync(new Google.Protobuf.WellKnownTypes.Empty(), startWindow.headers);
            TypeComboBox.ItemsSource = cargoTypes.CargoType;
            TypeComboBox.SelectedItem = data.CargoType == null ? null : cargoTypes.CargoType.First(x => x.Id == data.CargoType.Id);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            CloseAnimation();
        }

        private async void UpdateData()
        {
            try
            {
                CargoObject reqResult = new CargoObject();
                if (mode == 0)
                    reqResult = await startWindow.client.UpdateCargoAsync(new CreateOrUpdateCargoRequest { Cargo = data }, startWindow.headers);
                if (mode == 1)
                    reqResult = await startWindow.client.CreateCargoAsync(new CreateOrUpdateCargoRequest { Cargo = data }, startWindow.headers);

                var tablePage = (TablePage)startWindow.MainFrameK.Content;
                var page = tablePage.DataGridFrame.Content as CargoTablePage;
                if (mode == 0)
                {
                    var index = page.CargoObjects.FindIndex(t => t.Id == reqResult.Id);
                    page.CargoObjects[index] = reqResult;
                }
                if (mode == 1)
                    page.CargoObjectsOriginal.Add(reqResult);

                page.dataGrid.ItemsSource = null;
                page.dataGrid.ItemsSource = page.CargoObjectsOriginal.Skip(page.skipPages).Take(page.takePages).OrderBy(x=> x.Id);
                page.dataGrid.Items.Refresh();
                page.PaginationTextBlock.Text = $"{page.skipPages + 10} из {page.CargoObjectsOriginal.Count}";

                ShowToast(TablePage.Messages.Success);
            }
            catch (RpcException ex)
            {
                ShowToast(TablePage.Messages.Error);
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder changedDataNotify = new StringBuilder();
            if (mode == 0)
            {
                if (WeightTextBox.Text != data.Weight.ToString())
                    changedDataNotify.Append($"Масса: {data.Weight} -> {WeightTextBox.Text}\n");
                if (VolumeTextBox.Text != data.Volume.ToString())
                    changedDataNotify.Append($"Объём: {data.Volume} -> {VolumeTextBox.Text}\n");
                if (NameTextBox.Text != data.Name.ToString())
                    changedDataNotify.Append($"Название: {data.Name} -> {NameTextBox.Text}\n");
                if (PriceTextBox.Text != data.Price.ToString())
                    changedDataNotify.Append($"Цена: {data.Price} -> {PriceTextBox.Text}\n");
                if (ConstraintsTextBox.Text != data.Constraints.ToString())
                    changedDataNotify.Append($"Ограничения: {data.Constraints} -> {ConstraintsTextBox.Text}\n");
                if ((TypeComboBox.SelectedItem as CargoTypesObject)!.Name != data.CargoType.Name.ToString())
                    changedDataNotify.Append($"Тип груза: {data.CargoType.Name} -> {(TypeComboBox.SelectedItem as CargoTypesObject).Name}\n");
            }

            var result = MessageBox.Show($"Применить изменения?\n {changedDataNotify}", $"{text}", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var weight = WeightTextBox.Text.Replace('.', ',');
                    var volume = VolumeTextBox.Text.Replace(".", ",");
                    data.Weight = double.Parse(weight);
                    data.Volume = double.Parse(volume);
                    data.Name = NameTextBox.Text;
                    data.Price = double.Parse(PriceTextBox.Text);
                    data.Constraints = ConstraintsTextBox.Text;
                    data.CargoType = TypeComboBox.SelectedItem as CargoTypesObject;
                    UpdateData();
                }
                catch (Exception ex)
                {
                    switch (ex)
                    {
                        case RpcException:
                            MessageBox.Show($"Возникла ошибка. Обратитесь к администратору\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            break;
                        default:
                            MessageBox.Show("Проверьте заполненность всех полей. Удостоверьтесь, что численные значения введены верно", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                            break;
                    }
                }
            }
        }

        public void ShowToast(TablePage.Messages result)
        {
            ModalPageFrameNotification.Content = new ToastPage(result);
        }
    }
}
