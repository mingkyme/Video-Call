using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace Video_Call_Server.CustomClass
{
    public enum DrawModes
    {
        Pen,
        Line,
        ConnectedLine,
        Rectangle,
        Ellipse,
        Text,
        Eraser
    }
    class CustomCanvas : Canvas, INotifyPropertyChanged
    {
        private DrawModes drawMode;
        public DrawModes DrawMode
        {
            get
            {
                return drawMode;
            }
            set
            {
                drawMode = value; OnPropertyChanged(nameof(DrawMode));
            }
        }
        private Brush color;
        public Brush Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
            }
        }
        private double size;
        public double Size
        {
            get
            {
                return size;
            }
            set
            {
                size = value;
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public void AllClear()
        {
            Children.Clear();
        }
    }
}
