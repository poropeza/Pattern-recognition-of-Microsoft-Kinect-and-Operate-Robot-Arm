//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    using Microsoft.Kinect;
    using System;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using NxtNet;
    using System.Threading;
    using System.IO.Ports;
    // using Lego.Ev3.Core;
    // using Lego.Ev3.Desktop;


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Width of output drawing
        /// </summary>
        private const float RenderWidth = 640.0f;


        /// <summary>
        /// Height of our output drawing
        /// </summary>
        private const float RenderHeight = 480.0f;


        //lego
        private Nxt _nxt;
        Thread Motor;

        //bool flagConexion = true;


        /// <summary>
        /// Thickness of drawn joint lines
        /// </summary>
        private const double JointThickness = 5;

        /// <summary>
        /// Thickness of body center ellipse
        /// </summary>
        private const double BodyCenterThickness = 10;

        /// <summary>
        /// Thickness of clip edge rectangles
        /// </summary>
        private const double ClipBoundsThickness = 10;

        /// <summary>
        /// Brush used to draw skeleton center point
        /// </summary>
        private readonly Brush centerPointBrush = Brushes.Blue;

        /// <summary>
        /// Brush used for drawing joints that are currently tracked
        /// </summary>
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));

        /// <summary>
        /// Brush used for drawing joints that are currently inferred
        /// </summary>        
        private readonly Brush inferredJointBrush = Brushes.Yellow;

        /// <summary>
        /// Pen used for drawing bones that are currently tracked
        /// </summary>
        private readonly Pen trackedBonePen = new Pen(Brushes.Green, 6);

        /// <summary>
        /// Pen used for drawing bones that are currently inferred
        /// </summary>        
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor sensor;



        /// <summary>
        /// Drawing group for skeleton rendering output
        /// </summary>
        private DrawingGroup drawingGroup;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSource;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {

            InitializeComponent();
            _nxt = new Nxt();
            _nxt.Connect("COM5");
            label1.Content = "Lego Conectado!";
            //Motor = new Thread(new ThreadStart(avanzar));
            //this._nxt.GetOutputState(MotorPort.All);
            // this._nxt.ResetMotorPosition(MotorPort.PortA,true);
            //this._nxt.StartProgram("home/root/Demo.rxe");



            //  Motor = new Thread(new ThreadStart(avanzar));

            //tor.Start();
            //Thread.Sleep(1000);

        }

        /// <summary>
        /// Draws indicators to show which edges are clipping skeleton data
        /// </summary>
        /// <param name="skeleton">skeleton to draw clipping information for</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private static void RenderClippedEdges(Skeleton skeleton, DrawingContext drawingContext)
        {
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, RenderHeight - ClipBoundsThickness, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, ClipBoundsThickness, RenderHeight));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(RenderWidth - ClipBoundsThickness, 0, ClipBoundsThickness, RenderHeight));
            }
        }

        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // Create the drawing group we'll use for drawing
            this.drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);

            // Display the drawing using our image control
            Image.Source = this.imageSource;


            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                // Turn on the skeleton stream to receive skeleton frames
                this.sensor.SkeletonStream.Enable();

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;


                // Start the sensor!
                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }


        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.Stop();
            }
        }

        /// <summary>
        /// Event handler for Kinect sensor's SkeletonFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
                this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
            }

            using (DrawingContext dc = this.drawingGroup.Open())
            {
                // Draw a transparent background to set the render size
                dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));

                if (skeletons.Length != 0)
                {
                    foreach (Skeleton skel in skeletons)
                    {
                        RenderClippedEdges(skel, dc);


                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            this.DrawBonesAndJoints(skel, dc);
                            // Derecha  

                            dX.Content = System.Math.Round(skel.Joints[JointType.HandRight].Position.X, 3);
                            dY.Content = System.Math.Round(skel.Joints[JointType.HandRight].Position.Y, 3);
                            dZ.Content = System.Math.Round(skel.Joints[JointType.HandRight].Position.Z, 3);

                            // --- Izquierda
                            iX.Content = System.Math.Round(skel.Joints[JointType.HandLeft].Position.X, 3);
                            iY.Content = System.Math.Round(skel.Joints[JointType.HandLeft].Position.Y, 3);
                            iZ.Content = System.Math.Round(skel.Joints[JointType.HandLeft].Position.Z, 3);


                            //condicion para salir del programa
                            if (System.Math.Round(skel.Joints[JointType.HandRight].Position.X, 3) > 0.6 && System.Math.Round(skel.Joints[JointType.HandRight].Position.X, 3) < 0.85 && (System.Math.Round(skel.Joints[JointType.HandRight].Position.Y, 3) > 0.52 && System.Math.Round(skel.Joints[JointType.HandRight].Position.Y, 3) < 0.64))
                            {
                                //se cierra el programa

                                this.Close();
                                //Thread.Sleep(1000);

                            }



                            if (skel.Joints[JointType.ShoulderCenter].Position.Z <= 1.2)
                            {


                                Advertencia.Opacity = 100;
                                Motor.Abort();

                            }
                            else
                            {
                                Advertencia.Opacity = 0;


                            }

                            //conexion con el lego mindstorm
                            //se ha esto, para establecer solo una conexion durante la ejecucion del programa
                            //y no haya múltiples conexiones con el mismo brick

                            /*if (flagConexion)
                            {
                                lego = new Nxt();
                                lego.Connect("COM7");
                                label1.Content = "Lego Conectado!";

                                flagConexion = false;
                                //Motor = new Thread(new ThreadStart(detenerse));
                                //Motor.Start();
                                //Thread.Sleep(1000);
                            }*/

                            //movimientos para el ROBOT (actualmente solo funciona con la mano derecha)

                            if (System.Math.Round(skel.Joints[JointType.HandRight].Position.X, 3) > 0.09 && System.Math.Round(skel.Joints[JointType.HandRight].Position.X, 3) < 0.74 && (System.Math.Round(skel.Joints[JointType.HandRight].Position.Y, 3) > 0.04 && System.Math.Round(skel.Joints[JointType.HandRight].Position.Y, 3) < 0.54))
                            {
                                //estas en el primer cuadrante
                                label1.Content = "EL LEGO BAJÓ";
                                // this._nxt.ResetMotorPosition(MotorPort.PortA,true);
                                //this._nxt.StartProgram("Demo.rbt");





                                Motor = new Thread(new ThreadStart(avanzar));



                                Motor.Start();
                                //Thread.Sleep(1000);
                                // Motor = new Thread(new ThreadStart(detenerse));
                                // Motor.Start();
                                //Motor.Abort();



                            }
                            else if (System.Math.Round(skel.Joints[JointType.HandRight].Position.X, 3) > -0.86 && System.Math.Round(skel.Joints[JointType.HandRight].Position.X, 3) < -0.23 && (System.Math.Round(skel.Joints[JointType.HandRight].Position.Y, 3) > 0.05 && System.Math.Round(skel.Joints[JointType.HandRight].Position.Y, 3) < 0.54))
                            {
                                //estas en el segundo cuadrante
                                label1.Content = "EL LEGO SUBIÓ";
                                Motor = new Thread(new ThreadStart(reversa));



                                Motor.Start();

                                //Thread.Sleep(1000);
                                // Motor = new Thread(new ThreadStart(detenerse));
                                // Motor.Start();

                            }
                            else if (System.Math.Round(skel.Joints[JointType.HandRight].Position.X, 3) > -0.86 && System.Math.Round(skel.Joints[JointType.HandRight].Position.X, 3) < -0.23 && (System.Math.Round(skel.Joints[JointType.HandRight].Position.Y, 3) > -0.64 && System.Math.Round(skel.Joints[JointType.HandRight].Position.Y, 3) < -0.07))
                            {
                                //estas en el tercer cuadrante
                                label1.Content = "EL LEGO GIRÓ A LA DERECHA";
                                Motor = new Thread(new ThreadStart(derecha));



                                Motor.Start();

                                // Thread.Sleep(1000);
                                // Motor = new Thread(new ThreadStart(detenerse));
                                // Motor.Start();

                            }
                            else if (System.Math.Round(skel.Joints[JointType.HandRight].Position.X, 3) > 0.09 && System.Math.Round(skel.Joints[JointType.HandRight].Position.X, 3) < 0.74 && (System.Math.Round(skel.Joints[JointType.HandRight].Position.Y, 3) > -0.64 && System.Math.Round(skel.Joints[JointType.HandRight].Position.Y, 3) < -0.07))
                            {
                                //estas en el cuarto cuadrante
                                label1.Content = "EL LEGO GIRÓ A LA IZUIERDA";

                                Motor = new Thread(new ThreadStart(izquierda));



                                Motor.Start();

                                //Thread.Sleep(1000);
                                // Motor = new Thread(new ThreadStart(detenerse));
                                // Motor.Start();
                            }
                            else
                            {
                                //estas en el medio y no se hace nada
                                label1.Content = "SIN ACCIÓN";

                                // Motor = new Thread(new ThreadStart(detenerse));
                                // Motor.Start();
                            }

                        }
                        else if (skel.TrackingState == SkeletonTrackingState.PositionOnly)
                        {
                            dc.DrawEllipse(
                            this.centerPointBrush,
                            null,
                            this.SkeletonPointToScreen(skel.Position),
                            BodyCenterThickness,
                            BodyCenterThickness);
                        }
                    }
                }

                // prevent drawing outside of our render area
                this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
            }
        }

        /// <summary>
        /// Draws a skeleton's bones and joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private void DrawBonesAndJoints(Skeleton skeleton, DrawingContext drawingContext)
        {
            // Render Torso
            this.DrawBone(skeleton, drawingContext, JointType.Head, JointType.ShoulderCenter);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderRight);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.Spine);
            this.DrawBone(skeleton, drawingContext, JointType.Spine, JointType.HipCenter);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipLeft);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipRight);

            // Left Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderLeft, JointType.ElbowLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowLeft, JointType.WristLeft);
            this.DrawBone(skeleton, drawingContext, JointType.WristLeft, JointType.HandLeft);

            // Right Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderRight, JointType.ElbowRight);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowRight, JointType.WristRight);
            this.DrawBone(skeleton, drawingContext, JointType.WristRight, JointType.HandRight);



            // Render Joints
            foreach (Joint joint in skeleton.Joints)
            {
                Brush drawBrush = null;

                if (joint.TrackingState == JointTrackingState.Tracked)
                {
                    drawBrush = this.trackedJointBrush;
                }
                else if (joint.TrackingState == JointTrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush;
                }

                if (drawBrush != null)
                {
                    drawingContext.DrawEllipse(drawBrush, null, this.SkeletonPointToScreen(joint.Position), JointThickness, JointThickness);
                }
            }
        }

        /// <summary>
        /// Maps a SkeletonPoint to lie within our render space and converts to Point
        /// </summary>
        /// <param name="skelpoint">point to map</param>
        /// <returns>mapped point</returns>
        private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }

        /// <summary>
        /// Draws a bone line between two joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw bones from</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// <param name="jointType0">joint to start drawing from</param>
        /// <param name="jointType1">joint to end drawing at</param>
        private void DrawBone(Skeleton skeleton, DrawingContext drawingContext, JointType jointType0, JointType jointType1)
        {
            Joint joint0 = skeleton.Joints[jointType0];
            Joint joint1 = skeleton.Joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == JointTrackingState.NotTracked ||
                joint1.TrackingState == JointTrackingState.NotTracked)
            {
                return;
            }

            // Don't draw if both points are inferred
            if (joint0.TrackingState == JointTrackingState.Inferred &&
                joint1.TrackingState == JointTrackingState.Inferred)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this.inferredBonePen;
            if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked)
            {
                drawPen = this.trackedBonePen;
            }

            drawingContext.DrawLine(drawPen, this.SkeletonPointToScreen(joint0.Position), this.SkeletonPointToScreen(joint1.Position));
        }

        private void salir_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Handles the checking or unchecking of the seated mode combo box
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        /// 

        public void avanzar()//bajar
        {
            this._nxt.SetOutputState(MotorPort.PortA, -65, MotorModes.On, MotorRegulationMode.Sync, 0, MotorRunState.Running, 20);
            //this._nxt.SetOutputState(MotorPort.PortB, 0, MotorModes.On, MotorRegulationMode.Sync, 0, MotorRunState.Running, 0);

        }


        public void detenerse()
        {
            //this._nxt.PlayTone(601, 1000);
            this._nxt.SetOutputState(MotorPort.PortA, 0, MotorModes.Brake, MotorRegulationMode.Sync, 0, MotorRunState.Running, 0);
            this._nxt.SetOutputState(MotorPort.PortB, 0, MotorModes.Brake, MotorRegulationMode.Sync, 0, MotorRunState.Running, 0);
        }

        public void reversa()//subir
        {
            this._nxt.SetOutputState(MotorPort.PortA, 65, MotorModes.On, MotorRegulationMode.Sync, 0, MotorRunState.Running, 20);
            //this._nxt.SetOutputState(MotorPort.PortB, 75, MotorModes.On, MotorRegulationMode.Sync, 0, MotorRunState.Running, 0);
        }

        public void derecha()
        {

            //this._nxt.SetOutputState(MotorPort.PortA, 80, MotorModes.On, MotorRegulationMode.Sync, 0, MotorRunState.Running, 0);
            this._nxt.SetOutputState(MotorPort.PortB, -65, MotorModes.On, MotorRegulationMode.Sync, 0, MotorRunState.Running, 20);
        }

        public void izquierda()
        {
            // this._nxt.SetOutputState(MotorPort.PortA, -80, MotorModes.On, MotorRegulationMode.Sync, 0, MotorRunState.Running, 0);
            this._nxt.SetOutputState(MotorPort.PortB, 65, MotorModes.On, MotorRegulationMode.Sync, 0, MotorRunState.Running, 20);
        }

    }



}



/*
public void avanzar() {
            this._nxt.SetOutputState(MotorPort.PortA, -75, MotorModes.On, MotorRegulationMode.Sync, 0, MotorRunState.Running, 0);
            this._nxt.SetOutputState(MotorPort.PortB, -75, MotorModes.On, MotorRegulationMode.Sync, 0, MotorRunState.Running, 0);
            
            }


        public void detenerse() {
            this._nxt.PlayTone(601, 1000);
            this._nxt.SetOutputState(MotorPort.PortA, 0, MotorModes.Brake, MotorRegulationMode.Sync, 0, MotorRunState.Running, 0);
            this._nxt.SetOutputState(MotorPort.PortB, 0, MotorModes.Brake, MotorRegulationMode.Sync, 0, MotorRunState.Running, 0);
            }

        public void reversa() {
            this._nxt.SetOutputState(MotorPort.PortA, 75, MotorModes.On, MotorRegulationMode.Sync, 0, MotorRunState.Running, 0);
            this._nxt.SetOutputState(MotorPort.PortB, 75, MotorModes.On, MotorRegulationMode.Sync, 0, MotorRunState.Running, 0);
            }

        public void derecha() {
            
            this._nxt.SetOutputState(MotorPort.PortA, 80, MotorModes.On, MotorRegulationMode.Sync, 0, MotorRunState.Running, 0);
            this._nxt.SetOutputState(MotorPort.PortB, -80, MotorModes.On, MotorRegulationMode.Sync, 0, MotorRunState.Running, 0);
            }
        
        public void izquierda() {
            this._nxt.SetOutputState(MotorPort.PortA, -80, MotorModes.On, MotorRegulationMode.Sync, 0, MotorRunState.Running, 0);
            this._nxt.SetOutputState(MotorPort.PortB, 80, MotorModes.On, MotorRegulationMode.Sync, 0, MotorRunState.Running, 0);
            }




                Motor = new Thread(new ThreadStart(derecha));
                Motor.Start();
                
                msjLego.Text = "Derecha!";

*/
