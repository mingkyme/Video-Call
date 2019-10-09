using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Path = System.Windows.Shapes.Path;

namespace Video_Call_Server
{
    public partial class MainWindow : Window
    {
        // Camera Port
        const int CAMERA_PORT = 7000;
        // Canvas Port
        const int CANVAS_PORT = 7001;
        const int BUFF_SIZE = 1024;
        const int FPS = 30;
        const int WIDTH = 640;
        const int HEIGHT = 480;

        BackgroundWorker receiveBackgroundWorker;
        BackgroundWorker sendBackgroundWorker;
        TcpListener cameraListener;
        TcpListener canvasListener;
        TcpClient cameraClient;
        TcpClient canvasClient;
        NetworkStream cameraStream;
        NetworkStream canvasStream;
        byte[] receiveBytes;

        public MainWindow()
        {
            InitializeComponent();
            CameraNetworkInit();
            CanvasNetworkInit();
            CustomClass.CustomShape.mainCanvas = XAML_MainCanvas;
        }
        async void CameraNetworkInit()
        {
            cameraListener = new TcpListener(IPAddress.Any, CAMERA_PORT);
            cameraListener.Start();
            cameraClient = await cameraListener.AcceptTcpClientAsync();
            cameraStream = cameraClient.GetStream();
            // 웹캠 화면을 받는 부분
            receiveBackgroundWorker = new BackgroundWorker();
            receiveBackgroundWorker.DoWork += ReceiveBackgroundWorker_DoWork;
            receiveBackgroundWorker.RunWorkerAsync();

        }
        async void CanvasNetworkInit()
        {
            canvasListener = new TcpListener(IPAddress.Any, CANVAS_PORT);
            canvasListener.Start();
            canvasClient = await canvasListener.AcceptTcpClientAsync();
            canvasStream = canvasClient.GetStream();
            // 그린 데이터를 보내는 부분
            sendBackgroundWorker = new BackgroundWorker();
            sendBackgroundWorker.DoWork += SendBackgroundWorker_DoWork;
            sendBackgroundWorker.RunWorkerAsync();
        }
        RenderTargetBitmap inkCanvas_RTB;
        RenderTargetBitmap mainCanvas_RTB;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            inkCanvas_RTB = new RenderTargetBitmap((int)XAML_Viewbox.ActualWidth, (int)XAML_Viewbox.ActualHeight, 120, 120, PixelFormats.Default);
            mainCanvas_RTB = new RenderTargetBitmap((int)XAML_Viewbox.ActualWidth, (int)XAML_Viewbox.ActualHeight, 120, 120, PixelFormats.Default);

            

            

        }

        private void SendBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    // XAML_MainCanvas 를 이미지화 png 해야 함.
                    mainCanvas_RTB.Render(XAML_MainCanvas);
                    try
                    {

                        // 이미지 합성
                        var group = new DrawingGroup();
                        group.Children.Add(new ImageDrawing(mainCanvas_RTB, new Rect(0, 0, XAML_Viewbox.ActualWidth, XAML_Viewbox.ActualHeight)));
                        group.Children.Add(new ImageDrawing(inkCanvas_RTB, new Rect(0, 0, XAML_Viewbox.ActualWidth, XAML_Viewbox.ActualHeight)));
                        DrawingVisual drawingVisual = new DrawingVisual();
                        using (var drawingContext = drawingVisual.RenderOpen())
                        {
                            drawingContext.DrawDrawing(new DrawingImage(group).Drawing);
                        }

                        //Bitmapt으로 변환
                        RenderTargetBitmap bitmap = new RenderTargetBitmap((int)XAML_Viewbox.ActualWidth, (int)XAML_Viewbox.ActualHeight, 96d, 96d, System.Windows.Media.PixelFormats.Default);
                        bitmap.Render(drawingVisual);

                        PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
                        pngEncoder.Frames.Add(BitmapFrame.Create(bitmap));
                        System.IO.MemoryStream ms = new System.IO.MemoryStream();

                        pngEncoder.Save(ms);
                        ms.Close();


                        var sendBytes = ms.ToArray();
                        int len = sendBytes.Length;
                        try
                        {

                            canvasStream.WriteByte((byte)(len / 256 / 256));
                            canvasStream.WriteByte((byte)(len / 256));
                            canvasStream.WriteByte((byte)(len % 256));

                            int start = 0;
                            while (start < len)
                            {
                                int n = len - start >= BUFF_SIZE ? BUFF_SIZE : len - start;
                                canvasStream.Write(sendBytes, start, n);
                                start += n;
                            }

                            mainCanvas_RTB.Clear();

                        }
                        catch
                        {

                        }
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show(err.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }));
                // 준비됐는지 확인
                canvasStream.ReadByte();
                Thread.Sleep(1000 / FPS);
            }

        }

        private void ReceiveBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {

            while (true)
            {
                int len = 0;
                var a = cameraStream.ReadByte() * 256 * 256;
                var b = cameraStream.ReadByte() * 256;
                var c = cameraStream.ReadByte();
                len = a + b + c;
                Console.WriteLine(a / 256 /256);
                Console.WriteLine(b / 256);
                Console.WriteLine(c);
                Console.WriteLine(len);
                receiveBytes = new byte[len];

                int received = 0;
                while (received < len)
                {
                    int n = len - received >= BUFF_SIZE ? BUFF_SIZE : len - received;
                    cameraStream.Read(receiveBytes, received, n);
                    received += n;
                }
                Dispatcher.Invoke(() =>
                {
                    try
                    {
                        BitmapImage image;
                        image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = new MemoryStream(receiveBytes);
                        image.EndInit();

                        XAML_Image.Source = null;
                        XAML_Image.Source = image;
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                });

                // 준비됨을 보냄
                cameraStream.WriteByte(0);
                Thread.Sleep(1000 / FPS);

            }
        }

        private void XAML_InkCanvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            try
            {
                var strokePath = new Path
                {
                    Data = e.Stroke.GetGeometry(),
                    StrokeThickness = 2,
                    Fill = new SolidColorBrush(e.Stroke.DrawingAttributes.Color),
                    Stroke = Brushes.Transparent
                };
                strokePath.MouseMove += StrokePath_MouseMove;
                strokePath.MouseLeftButtonDown += StrokePath_MouseLeftButtonDown;
                XAML_MainCanvas.Children.Add(strokePath);
                (sender as InkCanvas).Strokes.Remove(e.Stroke);
                (sender as InkCanvas).Strokes.Clear();
                inkCanvas_RTB.Clear();
            }
            finally
            {

            }
        }

        private void StrokePath_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (XAML_MainCanvas.DrawMode == CustomClass.DrawModes.Eraser)
            {
                XAML_MainCanvas.Children.Remove(sender as Path);
            }
        }

        private void StrokePath_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && XAML_MainCanvas.DrawMode == CustomClass.DrawModes.Eraser)
            {
                XAML_MainCanvas.Children.Remove(sender as Path);
            }
        }

        private bool isDrawing;
        private Point startPoint;
        private CustomClass.CustomShape focusShape;
        private void XAML_MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                if (isDrawing)
                {
                    isDrawing = false;
                }
            }
            else // 눌렀을 때
            {
                if (isDrawing)
                {
                    // 이어짐
                    switch (XAML_MainCanvas.DrawMode)
                    {
                        case CustomClass.DrawModes.Pen:
                            break;
                        case CustomClass.DrawModes.Eraser:
                            break;
                        default:
                            focusShape.SetSize(startPoint, e.GetPosition(XAML_MainCanvas));
                            break;
                    }
                    return;
                }
                else
                {
                    // 첫 클릭
                    switch (XAML_MainCanvas.DrawMode)
                    {
                        case CustomClass.DrawModes.Pen:
                            break;
                        case CustomClass.DrawModes.Rectangle:
                            focusShape = new CustomClass.CustomRectangle();
                            break;
                        case CustomClass.DrawModes.Ellipse:
                            focusShape = new CustomClass.CsutomEllipse();
                            break;
                        default:
                            break;
                    }
                    isDrawing = true;
                    startPoint = e.GetPosition(XAML_MainCanvas);
                }
            }
        }

        private void XAML_MainCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDrawing = false;

        }
        private void XAML_InkCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                inkCanvas_RTB.Render(XAML_InkCanvas);

            }
        }

        private void CustomColorRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var value = (sender as CustomClass.CustomColorRadioButton).Color;
            XAML_MainCanvas.Color = value;
            XAML_InkCanvas.DefaultDrawingAttributes.Color = ((SolidColorBrush)value).Color;

        }

        private void CustomPenModeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            XAML_MainCanvas.DrawMode = (sender as CustomClass.CustomPenModeRadioButton).DrawMode;

        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double value = (sender as Slider).Value;
            XAML_MainCanvas.Size = value;
            XAML_InkCanvas.DefaultDrawingAttributes.Width = value;
            XAML_InkCanvas.DefaultDrawingAttributes.Height = value;
        }
    }

}
