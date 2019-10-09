using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
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
using Image = System.Windows.Controls.Image;
using Rectangle = System.Drawing.Rectangle;

namespace Video_Call_Client
{
    public partial class MainWindow : Window
    {
        const string SERVER = "127.0.0.1";

        // Camera Port
        const int CAMERA_PORT = 7000;
        // Canvas Port
        const int CANVAS_PORT = 7001;

        const int BUFF_SIZE = 1024;
        const int FPS = 30;
        BackgroundWorker sendBackgroundWorker;
        BackgroundWorker receiveBackgroundWorker;
        TcpClient cameraClient;
        TcpClient canvasClient;
        NetworkStream cameraStream;
        NetworkStream canvasStream;
        async void CameraNetworkInit()
        {
            cameraClient = new TcpClient();
            await cameraClient.ConnectAsync(SERVER, CAMERA_PORT);
            cameraStream = cameraClient.GetStream();

            sendBackgroundWorker = new BackgroundWorker();
            sendBackgroundWorker.DoWork += sendBackgroundWorker_DoWork;
            sendBackgroundWorker.RunWorkerAsync();
        }
        async void CanvasNetworkInit()
        {
            canvasClient = new TcpClient();
            await canvasClient.ConnectAsync(SERVER, CANVAS_PORT);
            canvasStream = canvasClient.GetStream();

            receiveBackgroundWorker = new BackgroundWorker();
            receiveBackgroundWorker.DoWork += ReceiveBackgroundWorker_DoWork;
            receiveBackgroundWorker.RunWorkerAsync();
        }
        public MainWindow()
        {
            InitializeComponent();
            CameraNetworkInit();
            CanvasNetworkInit();




            // WEBCAM
            cap = OpenCvSharp.VideoCapture.FromCamera(OpenCvSharp.CaptureDevice.Any, 0);
            cap.FrameWidth = frameWidth;
            cap.FrameHeight = frameHeight;
            cap.Open(0);
            wb = new WriteableBitmap(cap.FrameWidth, cap.FrameHeight, 96, 96, PixelFormats.Bgr24, null);

        }

        private void ReceiveBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                try
                {
                    int len = 0;
                    len += canvasStream.ReadByte() * 256 * 256;
                    len += canvasStream.ReadByte() * 256;
                    len += canvasStream.ReadByte() % 256;
                    byte[] receiveBytes = new byte[len];


                    int received = 0;
                    while (received < len)
                    {
                        int n = len - received >= BUFF_SIZE ? BUFF_SIZE : len - received;
                        canvasStream.Read(receiveBytes, received, n);
                        received += n;
                    }


                    Dispatcher.Invoke(new Action(() =>
                    {
                        using (Stream stream = new MemoryStream(receiveBytes))
                        {
                            BitmapImage image = new BitmapImage();
                            stream.Position = 0;
                            image.BeginInit();
                            image.CacheOption = BitmapCacheOption.OnLoad;
                            image.StreamSource = stream;
                            image.EndInit();
                            XAML_Image.Source = image;
                        }
                    }));
                    // Read Check
                    canvasStream.WriteByte(0);
                    Thread.Sleep(1000 / FPS);

                }
                catch
                {

                }
            }
        }

        private void sendBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                try
                {

                    byte[] sendBytes = CamCapture();
                    int len = sendBytes.Length;
                    cameraStream.WriteByte((byte)(len / 256 / 256));
                    cameraStream.WriteByte((byte)(len / 256));
                    cameraStream.WriteByte((byte)(len % 256));

                    int start = 0;
                    while (start < len)
                    {
                        int n = len - start >= BUFF_SIZE ? BUFF_SIZE : len - start;
                        cameraStream.Write(sendBytes, start, n);
                        start += n;
                    }
                    // Ready Check
                    cameraStream.ReadByte();
                    Thread.Sleep(1000 / FPS);
                }
                catch (Exception)
                {

                }
            }
        }
        /// <summary>
        /// 화면을 캡쳐합니다.
        /// </summary>
        /// <returns></returns>
        /// 
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
        // WEBCAM
        OpenCvSharp.VideoCapture cap;
        WriteableBitmap wb;
        const int frameWidth = 640;
        const int frameHeight = 480;
        OpenCvSharp.Mat mat;
        byte[] CamCapture()
        {
            mat = new OpenCvSharp.Mat();
            cap.Read(mat);
            return mat.ToBytes();
        }

    }
}
