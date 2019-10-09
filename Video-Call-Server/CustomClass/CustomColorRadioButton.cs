using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace Video_Call_Server.CustomClass
{
    class CustomColorRadioButton : RadioButton
    {
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

    }
}
