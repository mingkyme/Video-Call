using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Video_Call_Server.CustomClass
{
    abstract class CustomShape
    {
        public Shape shape;
        public static CustomCanvas mainCanvas;

        public abstract void SetSize(Point startPoint, Point nowPoint);
        public void Shape_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (mainCanvas.DrawMode == DrawModes.Eraser)
            {
                mainCanvas.Children.Remove(shape);
                shape = null;
            }
        }
        public void Shape_MouseMove(object sender, MouseEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed && mainCanvas.DrawMode == DrawModes.Eraser)
            {
                mainCanvas.Children.Remove(shape);
                shape = null;
            }
        }
    }
    class CsutomEllipse : CustomShape
    {
        public CsutomEllipse()
        {
            shape = new Ellipse();
            shape.Stroke = mainCanvas.Color;
            shape.StrokeThickness = mainCanvas.Size;
            shape.MouseLeftButtonDown += Shape_MouseLeftButtonDown;
            shape.MouseMove += Shape_MouseMove;
            mainCanvas.Children.Add(shape);
        }

        

        public override void SetSize(Point startPoint, Point nowPoint)
        {
            double x1, x2, y1, y2;
            if (startPoint.X < nowPoint.X)
            {
                x1 = startPoint.X;
                x2 = nowPoint.X;
            }
            else
            {
                x1 = nowPoint.X;
                x2 = startPoint.X;
            }
            if (startPoint.Y < nowPoint.Y)
            {
                y1 = startPoint.Y;
                y2 = nowPoint.Y;
            }
            else
            {
                y1 = nowPoint.Y;
                y2 = startPoint.Y;
            }

            Canvas.SetLeft(shape, x1);
            Canvas.SetTop(shape, y1);
            shape.Width = x2 - x1;
            shape.Height = y2 - y1;
        }
    }
    class CustomRectangle : CustomShape
    {
        public CustomRectangle()
        {
            shape = new Rectangle();
            shape.Stroke = mainCanvas.Color;
            shape.StrokeThickness = mainCanvas.Size;
            shape.MouseMove += Shape_MouseMove;
            mainCanvas.Children.Add(shape);
        }
        public override void SetSize(Point startPoint, Point nowPoint)
        {
            double x1, x2, y1, y2;
            if (startPoint.X < nowPoint.X)
            {
                x1 = startPoint.X;
                x2 = nowPoint.X;
            }
            else
            {
                x1 = nowPoint.X;
                x2 = startPoint.X;
            }
            if (startPoint.Y < nowPoint.Y)
            {
                y1 = startPoint.Y;
                y2 = nowPoint.Y;
            }
            else
            {
                y1 = nowPoint.Y;
                y2 = startPoint.Y;
            }

            Canvas.SetLeft(shape, x1);
            Canvas.SetTop(shape, y1);
            shape.Width = x2 - x1;
            shape.Height = y2 - y1;
        }
    }
}
