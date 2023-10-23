using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Globalization;
using System.Threading;

using YarpManagerCS;


namespace iClipsBrain
{
    /// <summary>
    /// Interaction logic for ECSWin.xaml
    /// </summary>
    public partial class MoodView : Window
    {
        public static readonly RoutedEvent NewMoodEvent = EventManager.RegisterRoutedEvent("NewMoodEventHandler", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MoodView));

        public event RoutedEventHandler NewMoodEventHandler
        {
            add { AddHandler(NewMoodEvent, value); }
            remove { RemoveHandler(NewMoodEvent, value); }
        }

        private Point _currentMood;
        public Point currentMood
        {
            get { return _currentMood; }
            set { SetMood(value); }
        }

        private DispatcherTimer timer;
        private Point inc = new Point();
        private int desiredsteps = 1000;
        private int stepcounter = 0;

        private YarpPort yarpPortReceiver;

        public MoodView(YarpPort yarpPortReceiver)
        {
            this.yarpPortReceiver = yarpPortReceiver;
        }

        string receivedMood = "";
        CancellationTokenSource cts = new CancellationTokenSource();

        double speed = 0.01;

        public MoodView()
        {
            InitializeComponent();

            currentMood = new Point(0, 0);

            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(timer_Tick);

            SetMood(currentMood);

            ThreadPool.QueueUserWorkItem(InitYarp);

        }

        private void InitYarp(object s)
        {

            yarpPortReceiver = new YarpPort();
            yarpPortReceiver.openReceiver("/AttentionModule/mood:o", "/iClipsMonitor/mood:i");

            //if (yarpPortTrigger.NetworkExists())
            //{
            //    this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate () { Ellyarp.Fill = Brushes.Green; }));
            //}

            ThreadPool.QueueUserWorkItem(tok =>
            {
                CancellationToken token = (CancellationToken)tok;
                while (!token.IsCancellationRequested)
                {
                    ReceiveDataMood();
                }
            }, cts.Token);




        }

        void ReceiveDataMood()
        {


            yarpPortReceiver.receivedData(out receivedMood);
            if (receivedMood != null && receivedMood != "")
            {
                //Console.WriteLine(receivedSetMotors);
                try
                {
                    receivedMood = receivedMood.Replace(@"\", "").Replace("\"", "");
                    Point p = new Point();
                    p.X = Convert.ToDouble(receivedMood.Split(' ')[0]);
                    p.Y = Convert.ToDouble(receivedMood.Split(' ')[1]);// JsonConvert.DeserializeObject<Point>(receivedECS);




                    this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                               new Action(delegate ()
                               {
                                   SetMood(p);

                               }));







                }
                catch (Exception exc)
                {
                    Console.WriteLine("Error XML Set motors: " + exc.Message);
                }



            }
        }


        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            timer.Stop();
        }

        public void SetMood(Point position)
        {
            _currentMood = position;
            DrawCurrentPoint(currentMood);
            RaiseEvent(new RoutedEventArgs(MoodView.NewMoodEvent, currentMood));
        }


        public void LoadLookAtPoint(string label, Point position)
        {
            DrawCurrentPoint(label, position);
        }


        private void DrawCurrentPoint(string label, Point pp)
        {

            double x = (pp.X) * (ECSCanvas.ActualWidth);
            double y = (pp.Y) * (ECSCanvas.ActualHeight);

            Ellipse p = new Ellipse();
            p.Fill = Brushes.ForestGreen;
            p.StrokeThickness = 1;
            p.Stroke = Brushes.Black;
            p.Width = 12;
            p.Height = 12;
            p.Margin = new Thickness(x - 6, y - 6, 0, 0);
            ECSCanvas.Children.Add(p);

            Label l = new Label();
            l.Content = label;
            l.Foreground = Brushes.Black;


            l.Margin = new Thickness(x, y + 10, 0, 0);



            ECSCanvas.Children.Add(l);
        }


        private void DrawCurrentPoint(Point point)
        {
            double x = (point.X + 1 ) * (ECSCanvas.Width)/2;
            double y = (-point.Y + 1 ) * ((ECSCanvas.Height)/2);

            //double y = ECSCanvas.ActualHeight - ((point.Y + 1) * (ECSCanvas.ActualHeight));

            CurrentECSLabel.Margin = new Thickness(x, y + 5, 0, 0);
            CurrentECSLabel.Content = ("(" + (decimal.Round((decimal)point.X, 2)).ToString() + ", " + (decimal.Round((decimal)point.Y, 2)).ToString() + ")");
            Position_Star.Margin = new Thickness(x - 6, y - 6, 0, 0);
        }


        private void MouseOnCanvas(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Cross;
        }

        private void MouseClickOnCanvas(object sender, MouseButtonEventArgs e)
        {
            Point mouseposition = new Point((Mouse.GetPosition(ECSCanvas).X / ECSCanvas.ActualWidth), (Mouse.GetPosition(ECSCanvas).Y / ECSCanvas.ActualHeight));
            SetMood(mouseposition);
        }

        private void StartAnimation(Point newposition, int speed)
        {
            timer.Interval = new TimeSpan(speed);
            inc.X = (newposition.X - currentMood.X) / desiredsteps;
            inc.Y = (newposition.Y - currentMood.Y) / desiredsteps;
            stepcounter = 0;
            timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            //if (stepcounter < desiredsteps)
            //{
            //    currentMood.X = currentMood.X + inc.X;
            //    currentMood.Y = currentMood.Y + inc.Y;
            //    SetMood(currentMood);
            //    stepcounter++;
            //}
        }

        protected override void OnClosed(EventArgs e)
        {
            cts.Cancel();

            //if (yarpPortRecever.PortExists("/AttentionModule/mode:o"))
            //    yarpPortRecever.Disconect("/iClipsMonitor/mode:i", "/AttentionModule/mode:o");

            timer.Stop();

            if (yarpPortReceiver != null)
                yarpPortReceiver.Close();
        }

       

        //private void Speed_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    try
        //    {
        //        double new_speed = Convert.ToDouble(txtSpeed.Text, NumberFormatInfo.InvariantInfo);
        //        if (new_speed > 0 || speed <= 1)
        //        {
        //            speed = new_speed;
        //        }
        //        else
        //            txtSpeed.Text = String.Format(speed.ToString(CultureInfo.InvariantCulture));

        //    }
        //    catch
        //    {

        //        txtSpeed.Text = String.Format(speed.ToString(CultureInfo.InvariantCulture));
        //    }
        //}
    }
}
