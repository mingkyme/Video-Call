using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
using System.Windows.Threading;

namespace Video_Call_Server
{
    public partial class MainWindow : Window
    {
        const int PORT = 7000;
        const int BUFF_SIZE = 1024;

        BackgroundWorker serverBackgroundWorker;
        TcpListener listener;

        byte[] receiveBytes;
        public MainWindow()
        {
            InitializeComponent();
            serverBackgroundWorker = new BackgroundWorker();
            serverBackgroundWorker.DoWork += ServerBackgroundWorker_DoWork;
            serverBackgroundWorker.RunWorkerAsync();
        }

        private void ServerBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            listener = new TcpListener(IPAddress.Any, PORT);
            listener.Start();

            TcpClient tcp = listener.AcceptTcpClient();
            NetworkStream stream = tcp.GetStream();
            while (true)
            {
                int len = 0;
                len += stream.ReadByte() * 256 * 256;
                len += stream.ReadByte() * 256;
                len += stream.ReadByte();

                receiveBytes = new byte[len];
                int received = 0;

                while(received < len)
                {
                    int n = len - received >= BUFF_SIZE ? BUFF_SIZE : len - received;
                    stream.Read(receiveBytes, received, n);
                    received += n;

                }
                Dispatcher.Invoke(() =>
                {
                    try
                    {
                        BitmapImage image;
                        image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.None;
                        image.StreamSource = new MemoryStream(receiveBytes);
                        image.EndInit();
                        XAML_Image.Source = null;
                        XAML_Image.Source = image;
                    }
                    catch
                    {

                    }
                });
            }
        }
    }
}
