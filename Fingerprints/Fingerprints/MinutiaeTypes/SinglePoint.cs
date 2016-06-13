﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
namespace Fingerprints
{
    class SinglePoint : Minutiae
    {
        Brush color;
        double size;
        Point singlePoint = new Point();
        MouseButtonEventHandler handler = null;

        public SinglePoint(Brush color, double size)
        {
            this.size = size;
            this.color = color;
        }
        public override void Draw(Canvas canvas, Image image, Border border)
        {            
            handler += (ss, ee) =>
            {
                if (ee.RightButton == MouseButtonState.Pressed)
                {
                    singlePoint = ee.GetPosition(canvas);;
                    EllipseGeometry myEllipseGeometry = new EllipseGeometry();
                    myEllipseGeometry.Center = singlePoint;
                    myEllipseGeometry.RadiusX = 2 * size;
                    myEllipseGeometry.RadiusY = 2 * size;
                    Path myPath = new Path();
                    myPath.Stroke = color;
                    myPath.StrokeThickness = 0.3;
                    myPath.Data = myEllipseGeometry;
                    myPath.Opacity = 0.5;
                    canvas.Children.Add(myPath);
                    DeleteEvent(image);    
                }

            };
            image.MouseRightButtonDown += handler;
        }

        public void DeleteEvent(Image image)
        {
            image.MouseRightButtonDown -= handler;
        }
    }
}
