﻿using ApiService;
using Grpc.Core;
using LogisticsClientsApp.Localizations;
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

namespace LogisticsClientsApp.Pages.Tables
{
    /// <summary>
    /// Логика взаимодействия для RolesTablePage.xaml
    /// </summary>
    public partial class RolesTablePage : Page
    {
        public List<RolesObject> Roles { get; set; }
        public List<RolesObject> RolesOriginal { get; set; }

        public int takePages = 10;
        public int skipPages = 0;
        private Locale locale;

        StartWindow startWindow;
        public RolesTablePage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            startWindow = (StartWindow)Window.GetWindow(this);
            locale = new Locale(startWindow.selectedLocale);
            string tableName = "категории";
            var tablePage = startWindow.MainFrameK.Content as TablePage;
            tablePage.TextBlockTableName.Text = tableName;
            SetData();
            ResizeDataGrid();
            startWindow.SizeChanged += (o, e) =>
            {
                ResizeDataGrid();
            };
        }

        public void FastSearch(string text, string? param)
        {
            if (text != "")
                switch (param)
                {
                    case "Название":
                        text = text.Trim();
                        Roles = RolesOriginal
                            .Where(x => x.Name.Contains(text))
                            .ToList();
                        if (Roles.Count == 0)
                            Roles = RolesOriginal;
                        break;
                }
            else
                Roles = RolesOriginal;
            dataGrid.ItemsSource = null;
            dataGrid.ItemsSource = Roles.Skip(skipPages).Take(takePages);
            PaginationTextBlock.Text = $"{skipPages + 10} из {Roles.Count}";
        }

        public void ResizeDataGrid()
        {
            dataGrid.MaxHeight = startWindow.Height / 2 - 40; ;
        }

        private void PrevTablePageButton_Click(object sender, RoutedEventArgs e)
        {
            if (skipPages - 10 >= 0)
            {
                skipPages -= 10;
                var skippedCargo = Roles.Skip(skipPages).Take(takePages).ToList();
                dataGrid.ItemsSource = skippedCargo;
                PaginationTextBlock.Text = $"{skipPages + 10} из {Roles.Count}";
            }
        }

        private void NextTablePageButton_Click(object sender, RoutedEventArgs e)
        {
            if (skipPages + 10 < Roles.Count)
            {
                skipPages += 10;
                var skippedCargo = Roles.Skip(skipPages).Take(takePages).ToList();
                dataGrid.ItemsSource = skippedCargo;
                PaginationTextBlock.Text = $"{skipPages + 10} из {Roles.Count}";
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show($"Вы действительно хотите удалить запись?", "Удаление", MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.No);
            if (result == MessageBoxResult.OK)
            {
                try
                {
                    var item = dataGrid.SelectedItem as RolesObject;
                    var resultLocal = await startWindow.client.DeleteRoleAsync(new GetOrDeleteRoleRequest { Id = item.Id }, startWindow.headers);
                    RolesOriginal.Remove(item);

                    dataGrid.ItemsSource = null;
                    dataGrid.ItemsSource = RolesOriginal.Skip(skipPages).Take(takePages);
                }
                catch (RpcException ex)
                {
                    if (ex.StatusCode == StatusCode.Unauthenticated)
                        MessageBox.Show("Ваше время сессии истекло. Перезайдите в аккаунт", "Сессия", MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                        MessageBox.Show($"Возникла ошибка: {ex.StatusCode}. Проверьте, что данная запись нигде более не используется", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void SetData()
        {
            try
            {
                var item = await startWindow.client.GetListRolesAsync(new Google.Protobuf.WellKnownTypes.Empty(), startWindow.headers);
                Roles = new List<RolesObject>();
                Roles.AddRange(item.RolesObject.ToList());

                RolesOriginal = Roles;

                dataGrid.ItemsSource = null;
                dataGrid.ItemsSource = Roles.Skip(skipPages).Take(takePages);
                locale.SetLocale(this);
                PaginationTextBlock.Text = $"{skipPages + 10} из {Roles.Count}";
            }
            catch (RpcException ex)
            {
#warning TODO
            }
        }

        private void OpenRowButton_Click(object sender, RoutedEventArgs e)
        {
            TablePage tablePage = (TablePage)startWindow.MainFrameK.Content;
            tablePage.ShowModalPage(0);
        }

        public void Dispose()
        {
            startWindow.SizeChanged -= (o, e) =>
            {
                ResizeDataGrid();
            };
            Roles.Clear();
            RolesOriginal.Clear();
            dataGrid.ItemsSource = null;
            BindingOperations.ClearAllBindings(dataGrid);
        }
    }
}