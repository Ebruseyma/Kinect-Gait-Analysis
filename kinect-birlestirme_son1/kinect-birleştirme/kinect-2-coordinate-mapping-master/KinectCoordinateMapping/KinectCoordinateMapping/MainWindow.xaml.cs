using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using LightBuzz.Vitruvius;
using System.IO;


namespace KinectCoordinateMapping
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        KinectSensor _sensor;
        MultiSourceFrameReader _reader;
        IList<Body> _bodies;

        CameraMode _mode = CameraMode.Color;

        private const float InferredZPositionClamp = 0.1f;

        public event PropertyChangedEventHandler PropertyChanged;
        private Body _body = null;
        private Floor _floor = null;

        private Point currentPoint;
        private Point previousPoint;
        private int stepCount = 0;
        private bool legUp = false;
        private int right;
        private int left;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _sensor = KinectSensor.GetDefault();

            if (_sensor != null)
            {
                _sensor.Open();

                _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared | FrameSourceTypes.Body);
                _reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (_reader != null)
            {
                _reader.Dispose();
            }

            if (_sensor != null)
            {
                _sensor.Close();
            }
        }

        void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var reference = e.FrameReference.AcquireFrame();

            // Color
            using (var frame = reference.ColorFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (_mode == CameraMode.Color)
                    {
                        camera.Source = frame.ToBitmap();
                    }
                }
            }

            // Depth
            using (var frame = reference.DepthFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (_mode == CameraMode.Depth)
                    {
                        camera.Source = frame.ToBitmap();
                    }
                }
            }

            // Infrared
            using (var frame = reference.InfraredFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (_mode == CameraMode.Infrared)
                    {
                        camera.Source = frame.ToBitmap();
                    }
                }
            }

            // Body
            using (var frame = reference.BodyFrameReference.AcquireFrame())
            {
                //BURADA - Text e veri kaydetme class'ından instance alıyoruz
                DataSave ds = new DataSave();
                Distance bs= new Distance();



                if (frame != null)
                {
                    canvas.Children.Clear();
                    _floor = frame.Floor();
                    _body = frame.Body();



                    _bodies = new Body[frame.BodyFrameSource.BodyCount];

                    frame.GetAndRefreshBodyData(_bodies);

                    foreach (var body in _bodies)
                    {
                        if (body.IsTracked)
                        {
                            // COORDINATE MAPPING
                            foreach (Joint joint in body.Joints.Values)
                            {
                                if (joint.TrackingState == TrackingState.Tracked)
                                {
                                    // 3D space point
                                    CameraSpacePoint jointPosition = joint.Position;

                                    if (jointPosition.Z<0)
                                    {
                                        jointPosition.Z = InferredZPositionClamp;
                                    }

                                    // 2D space point
                                    Point point = new Point();

                                    if (_mode == CameraMode.Color)
                                    {
                                        ColorSpacePoint colorPoint = _sensor.CoordinateMapper.MapCameraPointToColorSpace(jointPosition);
                             
                                        point.X = float.IsInfinity(colorPoint.X) ? 0 : colorPoint.X;
                                        point.Y = float.IsInfinity(colorPoint.Y) ? 0 : colorPoint.Y;

                                    }
                                    else if (_mode == CameraMode.Depth || _mode == CameraMode.Infrared) // Change the Image and Canvas dimensions to 512x424
                                    {
                                        DepthSpacePoint depthPoint = _sensor.CoordinateMapper.MapCameraPointToDepthSpace(jointPosition);
                                     
                                        point.X = float.IsInfinity(depthPoint.X) ? 0 : depthPoint.X;
                                        point.Y = float.IsInfinity(depthPoint.Y) ? 0 : depthPoint.Y;
                                    }

                                    // Draw
                                    Ellipse ellipse = new Ellipse
                                    {
                                        Fill = Brushes.Red,
                                        Width = 30,
                                        Height = 30
                                    };



                                    //bool legUp = false;
                                    //Point currentPoint = point;
                                    //if (legUp && Math.Abs(currentPoint.Y - previousPoint.Y) < 15)
                                    //{
                                    //    legUp = false;
                                    //    stepCount++;

                                    //}
                                    //else if (!legUp && Math.Abs(currentPoint.Y - previousPoint.Y) > 15)
                                    //{
                                    //    legUp = true;
                                    //    stepCount++;
                                    //}
                                    //previousPoint = currentPoint;



                                    Point currentPoint = point;
                                    if (Math.Abs(currentPoint.Y - previousPoint.Y) > 5)
                                    {
                                        stepCount++;

                                    }
                                    else if ( Math.Abs(currentPoint.Y - previousPoint.Y) < 5)
                                    {
                                    }
                                    previousPoint = currentPoint;





                                    //if (!legUp && right > left + 15)
                                    //{
                                    //    legUp = true;
                                    //    stepCount++;

                                    //}
                                    //else if (legUp && left > right + 15)
                                    //{
                                    //    legUp = false;
                                    //    stepCount++;

                                    //}
                                    //previousPoint = currentPoint;


                                    //BURADA - Gelen X Y Z verileri text e kaydedilir.
                                    ds.dataSave(stepCount,joint.JointType.ToString(), point.X, point.Y, jointPosition.Z, joint.JointType.ToString());

                                    Canvas.SetLeft(ellipse, point.X - ellipse.Width / 2);
                                    Canvas.SetTop(ellipse, point.Y - ellipse.Height / 2);

                                   
                                }
                            }
                        }
                    }

                      if (_floor != null && _body != null)
                    {
                        CameraSpacePoint ankle3D = _body.Joints[JointType.FootLeft].Position;

                        Point ankle2D = ankle3D.ToPoint();

                        double distance = _floor.DistanceFrom(ankle3D);
                        int floorY = _floor.FloorY((int)ankle2D.X, (ushort)(ankle3D.Z * 1000));

                        // double speed;
                        string time = DateTime.Now.ToString("ss.fff");


                        //spped= distance/time bunu yazıdırcaz
                        //speed = (Double.Parse(distance)) / Double.Parse   (time)).ToString();

                        // speed = (double(distance )/ time).ToString;



                        //int adimSayisi = 0;
                        //legUp = false;
                        //if (distance==0)
                        //{
                        //    legUp = false;
                        //    t2 = currentTime;
                        //}
                        //else
                        //{
                        //    legUp = true;
                        //    adimSayisi += 1;
                        //    t1= currentTime;
                        //    time = Convert.ToInt32(t2) - Convert.ToInt32(t1);
                        //}

                        TblDistance.Text = distance.ToString("N2");
                        bs.dataSave(distance, time);



                        Canvas.SetLeft(ImgFoot, ankle2D.X - ImgFoot.Width / 2.0);
                        Canvas.SetTop(ImgFoot, ankle2D.Y - ImgFoot.Height / 2.0);
                        Canvas.SetLeft(ImgFloor, ankle2D.X - ImgFloor.Width / 2.0);
                        Canvas.SetTop(ImgFloor, floorY - ImgFloor.Height / 2.0);
                    }
                      
                }
               
            }
        }
    }

    enum CameraMode
    {
        Color,
        Depth,
        Infrared
    }
}
