﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Fingerprints
{
    class Table
    {
        int countL = 0;
        int countR = 0;
        MouseButtonEventHandler handlerL = null;
        MouseButtonEventHandler handlerR = null;
        public void FillTableL(Canvas canvasL, Image imageL ,ListBox listBoxL, ComboBox comboBox)
        {
            handlerL += (ss, ee) =>
            {
                if (countL != canvasL.Children.Count && canvasL.Children.Count != 0)
                {
                    listBoxL.Items.Add(comboBox.SelectedItem.ToString());
                    countL = canvasL.Children.Count;
                }
            };
            imageL.MouseRightButtonUp += handlerL;
        }

        public void FillTableR(Canvas canvasR, Image imageR, ListBox listBoxR, ComboBox comboBox)
        {
            handlerR += (ss, ee) =>
            {
                if (countR != canvasR.Children.Count && canvasR.Children.Count != 0)
                {
                    listBoxR.Items.Add(comboBox.SelectedItem.ToString());
                    countR = canvasR.Children.Count;
                }
            };
            imageR.MouseRightButtonUp += handlerR;
        }

        public void UpdateCount(Canvas canvasL, Canvas canvasR)
        {
            countL -= 1;
            countR -= 1;
        }

        public void SelectedObject(Canvas canvas, ListBox listBox, Canvas canvas2)
        {
            listBox.SelectionChanged += (ss, ee) =>
            {
                try
                {
                    for (int i = 0; i < canvas.Children.Count; i++)
                    {
                        if (canvas.Children[i] != null)
                        {
                            canvas.Children[i].Opacity = 0.5;
                        }
                        if (canvas2.Children[i] != null)
                        {
                            canvas2.Children[i].Opacity = 0.5;
                        }                            
                    }
                    if (listBox.SelectedIndex != -1)
                    {
                        var element = canvas.Children[listBox.SelectedIndex];
                        var element2 = canvas2.Children[listBox.SelectedIndex];
                        element.Opacity = 1;
                        element2.Opacity = 1;
                    }
                }
                catch (Exception)
                {

                    
                }
                
            };
        }
    }
}
