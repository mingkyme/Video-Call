using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Rectangle = System.Drawing.Rectangle;

namespace Video_Call_Client
{
    public partial class MainWindow : Window
    {
        const string SERVER = "127.0.0.1";
        const int PORT = 7000;
        const int BUFF_SIZE = 1024;

        BackgroundWorker clientBackgroundWorker;
        TcpClient tcp;
        public MainWindow()
        {
            InitializeComponent();
            clientBackgroundWorker = new BackgroundWorker();
            clientBackgroundWorker.DoWork += ClientBackgroundWorker_DoWork;
            clientBackgroundWorker.RunWorkerAsync();

            
        }

        private void ClientBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            tcp = new TcpClient();
            tcp.Connect(SERVER, PORT);
            NetworkStream stream = tcp.GetStream();
            while (true)
            {
                try
                {

                    byte[] sendBytes = ScreenCapture();
                    Dispatcher.Invoke(() =>
                    {
                        XAML_Text.Text = sendBytes.Length.ToString();
                    });
                    int len = sendBytes.Length;
                    stream.WriteByte((byte)(len / 256 / 256));
                    stream.WriteByte((byte)(len / 256));
                    stream.WriteByte((byte)(len % 256));

                    int start = 0;
                    while (start < len)
                    {
                        int n = len - start >= BUFF_SIZE ? BUFF_SIZE : len - start;
                        stream.Write(sendBytes, start, n);
                        start += n;
                    }
                    Thread.Sleep(1000 / 24);
                }
                catch(Exception)
                {

                }
            }
        }

        static byte[] ScreenCapture()
        {
            Rectangle rect = Screen.PrimaryScreen.Bounds;
            Bitmap scrbmp = new Bitmap(rect.Width, rect.Height);
            
            using (Graphics g = Graphics.FromImage(scrbmp))
            {
                g.CopyFromScreen(rect.X, rect.Y, 0, 0, scrbmp.Size, CopyPixelOperation.SourceCopy);
            }
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(scrbmp, typeof(byte[]));
        }

    }
}
