﻿using Microsoft.Win32;
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

namespace Fingerprints
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MinutiaeTypeController controller;
        public DrawingEventHandler drawing;
        public MainWindow()
        {
            InitializeComponent();
            Application.Current.MainWindow = this;
            Picture p = new Picture(this);
            drawing = new DrawingEventHandler();
            p.InitializeR();
            p.InitializeL();
            controller = new MinutiaeTypeController();
            comboBox.ItemsSource = controller.Show();
            comboBoxChanged();
            InitTable();

            addEmpty.Click += addEmpty_Click;

            //Database.InitialData();
            this.Closing += (ss, ee) =>
            {
                if(MessageBox.Show("Czy zapisać zmiany?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    FileTransfer.Save();
                }
            };

            saveButton.Click += (ss, ee) =>
            {
                FileTransfer.Save();
            };
        }

        public void InitTable()
        {
            Table table = new Table();
        }

        public void comboBoxChanged()
        {
            comboBox.SelectionChanged += (ss, ee) =>
            {
                if (comboBox.SelectedValue != null)
                {
                    drawing.startNewDrawing(comboBox.SelectedValue.ToString());
                }
            };
        }

        private void wizardAdd_Click(object sender, RoutedEventArgs e)
        {
            Window1 win = new Window1();
            win.ShowDialog();
            drawing.stopDrawing();
            comboBox.ItemsSource = controller.Show();
        }

        private void leftMenuClick_Delete(object sender, RoutedEventArgs e)
        {
            int index = listBoxImageL.SelectedIndex;
            if (index == -1)
            {
                return;
            }
            listBoxImageL.Items.RemoveAt(index);
            canvasImageL.Children.RemoveAt(index);
            FileTransfer.ListL.RemoveAt(index);
        }
        private void rightMenuClick_Delete(object sender, RoutedEventArgs e)
        {
            int index = listBoxImageR.SelectedIndex;
            if (index == -1)
            {
                return;
            }

            listBoxImageR.Items.RemoveAt(index);
            canvasImageR.Children.RemoveAt(index);
            FileTransfer.ListR.RemoveAt(index);
        }

        void activeCanvasL_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb != null)
            {
                if (rb.IsChecked == true)
                {
                    borderRight.BorderBrush = Brushes.Black;
                    borderLeft.BorderBrush = Brushes.DeepSkyBlue;
                }
            }
        }
        void activeCanvasR_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb != null)
            {
                if (rb.IsChecked == true)
                {
                    borderLeft.BorderBrush = Brushes.Black;
                    borderRight.BorderBrush = Brushes.DeepSkyBlue;
                }
            }
        }
        private void addEmpty_Click(object sender, EventArgs e)
        {
            Empty empty = new Empty();
            //if (activeCanvasL.IsChecked == true)
            //{
            //    empty.Draw(canvasImageL, imageL);
            //    FileTransfer.ListL.Add("Puste");
            //}
            //else
            //{
            //    empty.Draw(canvasImageR, imageR);
            //    FileTransfer.ListR.Add("Puste");
            //}
        }
    }
}
