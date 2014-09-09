// Amu 20.02 copy from web example, modified
using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Windows.Markup;
using Microsoft.Phone.Controls;

namespace E_Bible
{

    public class PageTurn : PhoneApplicationPage
    {

        public PageTurn()
        {

        }
        //n Amu 11.04
        private List<String> splittedPages;
        private Parser parser = new Parser();

        // Amu 05.04.2012
        private int _currentLeftPage = 1;
        
        private int _index = 0;            // Current spread index
        private double _width;             // Width of each page
        private double _height;            // Height of each page
        private int _count = 0;            // Number of even/odd page pairs
        private bool _turning = false;     // True if page turn in progress
        private bool _animating = false;   // True if animated turn completion in progress
        private double _percent = 0.0;     // Percent turned (0 = 0%, 1 = 100%)
        private double _startPos = -1;     // X coordinate of initial mouse click
        private int _direction = 0;        // -1 = Turning left, 1 = Turning right
        private double _step = 0.025;      // Step size for animations
        private double _shadowWidth = 16;  // Maximum shadow width
        private double _shadowBreak = 5;   // Number of degrees required for shadow to attain maximum width
        private double _sensitivity = 1.0; // Higher number -> More mouse movement required for full turn

        private FrameworkElement _owner = null;                      // Owner of mouse capture
        private List<Canvas> _evens = new List<Canvas>();            // Even pages
        private List<Canvas> _odds = new List<Canvas>();             // Odd pages
        private Polygon _shadow = null;                              // Polygon object used to draw shadow
        private Storyboard _timer = null;                            // Storyboard timer for animations
        private Canvas _canvas = null;                               // Canvas containing pages
        private Canvas _workingOdd = null;                           // Working right page
        private Canvas _workingEven = null;                          // Working left page

        private PathGeometry _oddClipRegion = null;
        private LineSegment _oddClipRegionLineSegment1 = null;
        private LineSegment _oddClipRegionLineSegment2 = null;
        private PathGeometry _evenClipRegion = null;
        private LineSegment _evenClipRegionLineSegment1 = null;
        private LineSegment _evenClipRegionLineSegment2 = null;
        private LineSegment _evenClipRegionLineSegment3 = null;
        private TransformGroup _transformGroup = null;
        private RotateTransform _rotateTransform = null;
        private TranslateTransform _translateTransform = null;

        // XAML definition for clip region used on right-hand pages
        private const string _opg =
            "<PathGeometry xmlns=\"http://schemas.microsoft.com/client/2007\">" +
            "<PathFigure StartPoint=\"0,0\">" +
            "<LineSegment />" +
            "<LineSegment />" +
            "</PathFigure>" +
            "</PathGeometry>";

        // XAML definition for clip region used on left-hand pages
        private const string _epg =
            "<PathGeometry xmlns=\"http://schemas.microsoft.com/client/2007\">" +
            "<PathFigure StartPoint=\"0,0\">" +
            "<LineSegment Point=\"0,0\" />" +
            "<LineSegment Point=\"0,0\" />" +
            "<LineSegment Point=\"0,0\" />" +
            "</PathFigure>" +
            "</PathGeometry>";

        // XAML definition for transforms used on left-hand pages
        private const string _tg =
            "<TransformGroup xmlns=\"http://schemas.microsoft.com/client/2007\">" +
            "<RotateTransform />" +
            "<TranslateTransform />" +
            "</TransformGroup>";

        // XAML definition for Storyboard timer
        private const string _sb = "<Storyboard xmlns=\"http://schemas.microsoft.com/client/2007\" Duration=\"0:0:0.01\" />";

        // XAML definition for shadow polygon
        private const string _poly =
            "<Polygon xmlns=\"http://schemas.microsoft.com/client/2007\" Canvas.ZIndex=\"4\" Fill=\"Black\" Opacity=\"0.2\" Points=\"0,0 0,0 0,0 0,0\" Visibility=\"Collapsed\">" +
            "<Polygon.Clip>" +
            "<RectangleGeometry Rect=\"0,0,{0},{1}\" />" +
            "</Polygon.Clip>" +
            "</Polygon>";

        ////////////////////////////////////////////////////////////////////////
        // Events
        public event EventHandler PageTurned;

        ////////////////////////////////////////////////////////////////////////
        // Properties

        public int CurrentSpreadIndex
        {
            get { return _index; }
        }

        public int SpreadCount
        {
            get { return _count; }
        }

        public double Sensitivity
        {
            get { return _sensitivity; }
            set { _sensitivity = value; }
        }

        ////////////////////////////////////////////////////////////////////////
        // Public methods

        public void Initialize(Canvas canvas)
        {
            _canvas = canvas;

            // Create a Storyboard for timing animations
            _timer = (Storyboard)XamlReader.Load(_sb);
            _timer.Completed += new EventHandler(OnTimerTick);

            // Create a PathGeometry for clipping right-hand pages
            _oddClipRegion = (PathGeometry)XamlReader.Load(_opg);
            _oddClipRegionLineSegment1 = (LineSegment)_oddClipRegion.Figures[0].Segments[0];
            _oddClipRegionLineSegment2 = (LineSegment)_oddClipRegion.Figures[0].Segments[1];

            // Create a PathGeometry for clipping left-hand pages
            string xaml = String.Format(_epg, _evens[0].Height);
            _evenClipRegion = (PathGeometry)XamlReader.Load(xaml);
            _evenClipRegionLineSegment1 = (LineSegment)_evenClipRegion.Figures[0].Segments[0];
            _evenClipRegionLineSegment2 = (LineSegment)_evenClipRegion.Figures[0].Segments[1];
            _evenClipRegionLineSegment3 = (LineSegment)_evenClipRegion.Figures[0].Segments[2];

            // Create a TransformGroup for transforming left-hand pages
            _transformGroup = (TransformGroup)XamlReader.Load(_tg);
            _rotateTransform = (RotateTransform)_transformGroup.Children[0];
            _translateTransform = (TranslateTransform)_transformGroup.Children[1];

            // Initialize internal variables
            _count = _evens.Count;
            _width = _evens[0].Width;
            _height = _evens[0].Height;

            // Create a Polygon to provide shadow during page turns
            _shadow = (Polygon)XamlReader.Load(String.Format(_poly, _width * 2, _height));
            _canvas.Children.Add(_shadow);

            // Initialize z-order
            InitializeZOrder();
        }

        public void AddSpread(Canvas left, Canvas right)
        {
            left.MouseLeftButtonDown += new MouseButtonEventHandler(OnBeginRightTurn);
            left.MouseMove += new MouseEventHandler(OnContinueRightTurn);
            left.MouseLeftButtonUp += new MouseButtonEventHandler(OnEndRightTurn);
            _evens.Add(left);

            right.MouseLeftButtonDown += new MouseButtonEventHandler(OnBeginLeftTurn);
            right.MouseMove += new MouseEventHandler(OnContinueLeftTurn);
            right.MouseLeftButtonUp += new MouseButtonEventHandler(OnEndLeftTurn);
            _odds.Add(right);
        }

        public void GoToSpread(int index)
        {
            if (index != _index && index > 0 && index < _count)
            {
                _index = index;
                InitializeZOrder();

                if (PageTurned != null)
                    PageTurned(this, EventArgs.Empty);
            }
        }

        ////////////////////////////////////////////////////////////////////////
        // Event handlers

        private void OnBeginRightTurn(Object sender, MouseButtonEventArgs e)
        {
            if (_animating)
                return; // Do nothing if animation in progress

            // Do nothing if trying to turn right but first
            // page is displayed
            if (_index == 0)
                return;

            // Start a right turn
            _turning = true;
            _direction = 1;
            _percent = 1.0;
            _startPos = e.GetPosition((FrameworkElement)sender).X;
            _owner = (FrameworkElement)sender;
            ((FrameworkElement)sender).CaptureMouse();

            // Turn page to specified angle
            TurnTo(_percent);

            // Cache references to "working" pages
            _workingOdd = _odds[_index - 1];
            _workingEven = _evens[_index];

            // Assign clipping regions and transforms to relevant canvases
            _workingOdd.Clip = _oddClipRegion;
            _workingEven.Clip = _evenClipRegion;
            _workingEven.RenderTransform = _transformGroup;

            // Set z-indexes for a right turn
            _evens[_index].SetValue(Canvas.ZIndexProperty, 3);
            _evens[_index - 1].SetValue(Canvas.ZIndexProperty, 0);
            _odds[_index - 1].SetValue(Canvas.ZIndexProperty, 2);


        }

        private void OnContinueRightTurn(Object sender, MouseEventArgs e)
        {
            if (_animating)
                return; // Do nothing if animation in progress

            if (_turning)
            {
                // Compute change in X
                double dx = e.GetPosition((FrameworkElement)(((FrameworkElement)sender).Parent)).X - _startPos;

                // If mouse moved left, update _startPos so page
                // begins turning with first move right
                if (dx < 0)
                {
                    _startPos = e.GetPosition((FrameworkElement)(((FrameworkElement)sender).Parent)).X;
                    return;
                }

                // Compute turn percentage based on change in X
                double percent = 1.0 - (dx / (_width * _sensitivity));

                if (percent > 1.0)
                    percent = 1.0;
                else if (percent < 0.0)
                    percent = 0.0;

                // Exit now if no change
                if (percent == _percent)
                    return;

                // Update percent turned
                _percent = percent;

                // Turn page to specified angle
                TurnTo(this._percent);
            }
        }

        private void OnEndRightTurn(Object sender, MouseButtonEventArgs e)
        {
            if (_animating)
                return; // Do nothing if animation in progress

            if (_turning)
            {
                // Amu 05.04.2012
           //     _currentLeftPage = ((_index + 1) * 2) - 1;
                _currentLeftPage = _currentLeftPage - 2;
                CompleteTurn();
            }
        }

        private void OnBeginLeftTurn(Object sender, MouseButtonEventArgs e)
        {
            if (_animating)
                return; // Do nothing if animation in progress

            
            // Do nothing if trying to turn left but last
            // page is displayed
            // Amu 27.04.2012 _index increase per 2 every bookRender() in PageTurnerPage class
            if (_index == _count - 1)
            {

                return;
            }
            // Start a left turn
            _turning = true;
            _direction = -1;
            _percent = 0.0;
            _startPos = e.GetPosition((FrameworkElement)sender).X;
            _owner = (FrameworkElement)sender;
            ((FrameworkElement)sender).CaptureMouse();

            // Turn page to specified angle
            TurnTo(_percent);

            // Cache references to "working" pages
            _workingOdd = _odds[_index];
            _workingEven = _evens[_index + 1];

            // Assign clipping regions and transforms to relevant canvases
            _workingOdd.Clip = _oddClipRegion;
            _workingEven.Clip = _evenClipRegion;
            _workingEven.RenderTransform = _transformGroup;

            // Set z-indexes for a left turn
            _evens[_index + 1].SetValue(Canvas.ZIndexProperty, 2);
            _odds[_index + 1].SetValue(Canvas.ZIndexProperty, 0);
        }

        private void OnContinueLeftTurn(Object sender, MouseEventArgs e)
        {
            if (_animating)
                return; // Do nothing if animation in progress

            if (_turning)
            {
                // Compute change in X
                double dx = _startPos - e.GetPosition((FrameworkElement)sender).X;

                // If mouse moved right, update _startPos so page
                // begins turning with first move left
                if (dx < 0)
                {
                    _startPos = e.GetPosition((FrameworkElement)sender).X;
                    return;
                }

                // Compute turn percentage based on change in X
                double percent = dx / (_width * _sensitivity);

                if (percent > 1.0)
                    percent = 1.0;
                else if (percent < 0.0)
                    percent = 0.0;

                // Exit now if no change
                if (percent == _percent)
                    return;

                // Update percent turned
                _percent = percent;

                // Turn page to specified angle
                TurnTo(_percent);
            }
        }

        private void OnEndLeftTurn(Object sender, MouseButtonEventArgs e)
        {
            if (_animating)
                return; // Do nothing if animation in progress

            if (_turning)
            {
                // Amu 05.04.2012 - _index starts from zero thats why "+ 1"
             //   _currentLeftPage = ((_index + 1) * 2) - 1;
                _currentLeftPage = _currentLeftPage + 2;
                CompleteTurn();
            }
         }

        private void OnTimerTick(Object sender, EventArgs e)
        {
            _percent += _step;

            if (_percent < 0.0)
                _percent = 0.0;
            else if (_percent > 1.0)
                _percent = 1.0;

            TurnTo(_percent);

            if (_percent == 0.0)
            {
                if (_direction == 1)
                {
                    _index--;
                    if (PageTurned != null)
                        PageTurned(this, EventArgs.Empty);
                }
                Reset();
            }
            else if (_percent == 1.0)
            {
                if (_direction == -1)
                {
                    _index++;
                    if (PageTurned != null)
                        PageTurned(this, EventArgs.Empty);
                }
                Reset();
            }
            else
                _timer.Begin();
        }

        ////////////////////////////////////////////////////////////////////////
        // Helper methods

        private void TurnTo(double percent)
        {
            // Compute angle of rotation
            double degrees = 45 - (percent * 45);
            double radians = degrees * Math.PI / 180;

            // Compute x coordinates along bottom of canvas
            double dx1 = _width - (percent * _width);
            double dx2 = _width - dx1;

            // Compute tangent of rotation angle
            double tan = Math.Tan(radians);

            // Configure clipping region for right-hand page
            double p2y;

            if (tan == 0)
                p2y = _height;
            else
                p2y = _height + (dx1 / tan);

            double p3x = p2y * tan;

            _oddClipRegionLineSegment1.Point = new Point(0, p2y);
            _oddClipRegionLineSegment2.Point = new Point(p3x, 0);

            // Configure clipping region for left-hand page
            double p7x = dx2 - (_height * tan);

            if (p7x >= 0.0) // 4-corner clipping region
            {
                _evenClipRegion.Figures[0].StartPoint = new Point(0, 0);
                _evenClipRegionLineSegment1.Point = new Point(0, _height);
                _evenClipRegionLineSegment2.Point = new Point(dx2, _height);
                _evenClipRegionLineSegment3.Point = new Point(p7x, 0);
            }
            else // 3-corner clipping region
            {
                double y = _height - (dx2 / tan);
                _evenClipRegion.Figures[0].StartPoint = _evenClipRegionLineSegment3.Point = new Point(0, y);
                _evenClipRegionLineSegment1.Point = new Point(0, _height);
                _evenClipRegionLineSegment2.Point = new Point(dx2, _height);
            }

            // Apply clipping regions and transforms
            _rotateTransform.CenterX = dx2;
            _rotateTransform.CenterY = _height;
            _rotateTransform.Angle = 2 * degrees;

            _translateTransform.X = 2 * (_width - dx2);

            // Configure shadow
            if (percent == 0.0 || percent == 1.0)
            {
                _shadow.Visibility = Visibility.Collapsed;
                return;
            }

            _shadow.Visibility = Visibility.Visible;

            double min = this._shadowBreak;
            double max = 45 - this._shadowBreak;
            double width;

            if (degrees > min && degrees < max)
                width = _shadowWidth;
            else
            {
                if (degrees <= min)
                    width = (degrees / _shadowBreak) * _shadowWidth;
                else // degrees >= max
                    width = ((45 - degrees) / _shadowBreak) * _shadowWidth;
            }

            double x1 = _width + dx1 + (_height * tan);
            double x2 = _width + dx1;
            double y2 = _height;
            double x3 = x2 + width;
            double y3 = _height;
            double x4 = x1 + width;

            _shadow.Points[0] = new Point(x1, 0);
            _shadow.Points[1] = new Point(x2, y2);
            _shadow.Points[2] = new Point(x3, y3);
            _shadow.Points[3] = new Point(x4, 0);
        }

        private void CompleteTurn()
        {
            if (_percent == 0.0)
            {
                if (_direction == 1)
                {
                    _index--;
                    if (PageTurned != null)
                        PageTurned(this, EventArgs.Empty);
                }
                Reset();
                // Amu 10.04
                updatePageinfo();

                return;
            }

            if (_percent == 1.0)
            {
                if (_direction == -1)
                {
                    _index++;
                    if (PageTurned != null)
                        PageTurned(this, EventArgs.Empty);
                }
                Reset();
                // Amu 10.04
                updatePageinfo();

                return;
            }

            // Amu 10.04.2012
            updatePageinfo();

            if (_percent < 0.5)
                _step = -Math.Abs(_step);
            else
                _step = Math.Abs(_step);

            _animating = true;
            _timer.Begin();
        }

        // Amu 10.04 new method
        private void updatePageinfo()
        {
            // Amu 05.04.2012
            // TODO: maybe not the best possible way to handle
            // --> Amu 20.02 - Amu 20.02 need to use static textBox, object instance dont work. Set in PageTurnerPage constructor
            StaticDataForPageChange.pageNumTxtBoxLeft.Visibility = Visibility.Visible;
            StaticDataForPageChange.pageNumTxtBoxRight.Visibility = Visibility.Visible;

            // Amu 05.04
            StaticDataForPageChange.pageNumTxtBoxLeft.Text = StaticDataForPageChange.bookHeader + ": " + Environment.NewLine + "Page: " + _currentLeftPage + "/" + StaticDataForPageChange.amountOfPages;
            StaticDataForPageChange.pageNumTxtBoxLeft.FontSize = 16.0;

            StaticDataForPageChange.pageNumTxtBoxRight.Text = StaticDataForPageChange.bookHeader + ": " + Environment.NewLine + "Page: " + (_currentLeftPage + 1) + "/" + StaticDataForPageChange.amountOfPages;
            StaticDataForPageChange.pageNumTxtBoxRight.FontSize = 16.0;

            StaticDataForPageChange.leftPageNumber = _currentLeftPage;
            
            // Amu 12.04
            // Lets create more pages
            if(_currentLeftPage >= StaticDataForPageChange.currentMaxPageNumber - 1)
                StaticDataForPageChange.morePages = true;

        }

        private void Reset()
        {
            _turning = false;
            _animating = false;
            _direction = 0;

            if (_owner != null)
                _owner.ReleaseMouseCapture();
            _owner = null;

            if (_workingOdd != null && _workingOdd.Clip != null)
                _workingOdd.Clip = null;
            if (_workingEven != null && _workingEven.Clip != null)
                _workingEven.Clip = null;
            if (_workingEven != null && _workingEven.RenderTransform != null)
                _workingEven.RenderTransform = null;

            _workingOdd = null;
            _workingEven = null;

            _shadow.Visibility = Visibility.Collapsed;

            InitializeZOrder();
        }

        private void InitializeZOrder()
        {
            for (int i = 0; i < _count; i++)
            {
                _evens[i].SetValue(Canvas.ZIndexProperty, (i == _index) ? 1 : -1);
                _odds[i].SetValue(Canvas.ZIndexProperty, (i == _index) ? 1 : -1);
            }
        }

    }
}
