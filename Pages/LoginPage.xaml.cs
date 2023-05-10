﻿using LogisticsClientsApp.Localizations;
using LogisticsClientsApp.Localizations.Data;
using LogisticsClientsApp.Pages.Tables;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Printing.IndexedProperties;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LogisticsClientsApp.Pages
{
    /// <summary>
    /// Логика взаимодействия для LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        StartWindow startWindow;

        public LoginPage(StartWindow startWindow)
        {
            InitializeComponent();
            startWindow.MainFrameK.NavigationService.RemoveBackEntry();
            if (startWindow != null) this.startWindow = startWindow;
            startWindow.LeftMenu.Visibility = Visibility.Hidden;
            Locale locale = new Locale("ru");
            locale.SetLocale(this);

            ErrorStackPanel.Visibility = Visibility.Hidden;
        }

        private async void LoginHandler()
        {
            var data = await startWindow.Login(LoginTextBox.Text.ToString(), PasswordTextBox.Password.ToString());
            startWindow.ChangePage(new TablePage());
            startWindow.ShowSideMenu();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorStackPanel.Visibility= Visibility.Visible;
            //Thread.Sleep(5000);
            LoginHandler();
        }

        private void Canvas_MouseEnter(object sender, MouseEventArgs e)
        {
            RotateTransform transform = new RotateTransform();

            var a = CanvasItem.RenderTransform;
            transform.Angle += 20;
        }

        private void UnSetErrorWhileLogin()
        {
            ErrorStackPanel.Visibility = Visibility.Hidden;
        }

        private void LoginTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UnSetErrorWhileLogin();
        }

        private void PasswordTextBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UnSetErrorWhileLogin();
        }
    }
}