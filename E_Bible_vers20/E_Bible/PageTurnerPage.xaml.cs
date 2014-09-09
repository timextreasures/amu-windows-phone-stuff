// E_Bible_vers14

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Windows.Resources;
using System.IO;
using System.Windows.Media.Imaging;

namespace E_Bible
{
    public partial class PageTurnerPage : PhoneApplicationPage
    {
        private PageTurn pageTurn = new PageTurn();
        private Parser parser = new Parser();

        private List<String> splittedPages;


        public PageTurnerPage(/*String fileUri, StreamResourceInfo package*/)
        {
            InitializeComponent();

            // Size of the book - pages in landscape and portrait cases
            this.SizeChanged += new SizeChangedEventHandler(MainPage_SizeChanged);

            // For creating new spreads when user turn the page
            this.MouseLeftButtonDown += new MouseButtonEventHandler(checkIfNextPageNeeded);

            // In cases when orientation is changed when PageTurnerPage view is open
            this.OrientationChanged += new EventHandler<OrientationChangedEventArgs>(MainPage_OrientationChanged);

            // this needs to be here !! 
            // When navigation change to PageTurnedPage this constructor is called!!
            // TODO Bad coding, this is needed temporaly because navigation to change page always brings flow here, so this ensure that only when Bible cntent is ready we render
            if (StaticDataForPageChange.textContentIsReady)
            {
                splittedPages = parser.splitTheTextToPages(StaticDataForPageChange.data);

                // FIRST 2 SPREADS 
                renderBook(0);

                StaticDataForPageChange.textContentIsReady = false;

                StaticDataForPageChange.pageNumTxtBoxLeft = pageNumbersInLeftPage;
                StaticDataForPageChange.pageNumTxtBoxRight = pageNumbersInRightPage;

                // first 2 pages - spread num 1
                StaticDataForPageChange.pageNumTxtBoxLeft.Text = StaticDataForPageChange.bookHeader + ": " + Environment.NewLine + 
                    "Page: " + 1 + "/" + StaticDataForPageChange.amountOfPages;
                StaticDataForPageChange.pageNumTxtBoxLeft.FontSize = 16.0;

                StaticDataForPageChange.pageNumTxtBoxRight.Text = StaticDataForPageChange.bookHeader + ": " + Environment.NewLine + 
                    "Page: " + 2 + "/" + StaticDataForPageChange.amountOfPages;
                StaticDataForPageChange.pageNumTxtBoxRight.FontSize = 16.0;
 
            }
        }

        /// <summary>
        /// Set the two page split
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {

           // Landscape
            if (this.Orientation == PageOrientation.LandscapeLeft)
            {
                ZoomTransform.ScaleX = ZoomTransform.ScaleY = (this.ActualWidth / PageTurnCanvas.Width) * 0.95;
                horizonScroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;

                horizonScroller.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;

                // 10.04.2012 Landscape left textbox page - counter location
                pageNumbersInLeftPage.Margin = new Thickness(45, 420, 385, -20); //(80, 380, 340, 0); // left, top, right, bottom: |->, ^_ , <-| , v
                pageNumbersInRightPage.Margin = new Thickness(390, 420, 45, -20);

            }

            // Portrait
            else
            {
                ZoomTransform.ScaleX = (this.ActualHeight / PageTurnCanvas.Height) * 0.75; // 
                ZoomTransform.ScaleY = (this.ActualHeight / PageTurnCanvas.Height) * 0.75; // 

                // positioning: left page on proper place when the flip - page vieww is opened
                ZoomTransform.CenterX = (this.ActualWidth / 2) * 0.5;
                // Lets take horizontal scrollbar to use
                horizonScroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;

                // 10.04.2012 Portrait left textbox page - counter location
                pageNumbersInLeftPage.Margin = new Thickness(6, 680, 564, 20); //(0, 500, 540, 100); // left, top, right, bottom: |->, ^_ , <-| , v
                pageNumbersInRightPage.Margin = new Thickness(442, 680, 145, 20);
            }   
        }

        /// <summary>
        /// In case user change orientation during page - flipping
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MainPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            // Switch the size of the canvases based on an orientation change.

            // Turned to Portrait
            if ((e.Orientation & PageOrientation.Portrait) == (PageOrientation.Portrait))
            {

                ZoomTransform.ScaleX = (this.ActualHeight / PageTurnCanvas.Height) * 0.75; // 
                ZoomTransform.ScaleY = (this.ActualHeight / PageTurnCanvas.Height) * 0.75; //

                // positioning: left page on proper place when the flip - page vieww is opened
                ZoomTransform.CenterX = (this.ActualWidth / 2) * 0.5;

                // Lets take horizontal scrollbar to use
                horizonScroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;

                // 10.04.2012 Portrait textbox page - counter locations
                pageNumbersInLeftPage.Margin = new Thickness(6, 680, 564, 20); //(0, 500, 540, 100);  // left, top, right, bottom: |->, ^_ , <-| , v
                pageNumbersInRightPage.Margin = new Thickness(442, 680, 145, 20);
            }

            // Turned to Landscape mode, move buttonList content to a visible row and column.
            else
            {
                ZoomTransform.ScaleX = ZoomTransform.ScaleY = (this.ActualWidth / PageTurnCanvas.Width) * 0.95;
                horizonScroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;

                horizonScroller.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                
                // TODO: This works proximitely - locate the book horizontally OK - why 1.5 clarify
                ZoomTransform.CenterX = (this.ActualWidth / 1.5);
                // 10.04.2012 Landscape textboxes - Header and counter location
                pageNumbersInLeftPage.Margin = new Thickness(45, 420, 385, -20); //(80, 380, 320, 0); // left, top, right, bottom: |->, ^_ , <-| , v
                pageNumbersInRightPage.Margin = new Thickness(390, 420, 45, -20);
            //    pageNumbersInLeftPage.Visibility = System.Windows.Visibility.Visible;
            }

        }

        /// <summary>
        /// TODO clarify is this right way to handle ????????????
        /// Saves the whole html content to static variable
        /// This html content (StaticDataForPageChange.data) will be splitted to pages in parser.splitTheTextToPages
        /// </summary>
        /// <param name="fileUri"></param>
        public void saveCurrentFlipModeContent(string fileUri, StreamResourceInfo package)
        {

            Stream contentHtml = Application.GetResourceStream(package, new Uri(fileUri, UriKind.Relative)).Stream;
            StreamReader contentReader = new StreamReader(contentHtml);
            string html = contentReader.ReadToEnd();

            // needs to be static
            StaticDataForPageChange.data = html;

        }

        /// <summary>
        /// Event handler for new page flipping, happens right after PageTurn mouse event handling
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void checkIfNextPageNeeded(object sender, EventArgs e)
        {
            if (StaticDataForPageChange.morePages == true)
                renderBook(StaticDataForPageChange.currentMaxPageNumber);
            StaticDataForPageChange.morePages = false;

            // for future purposes, when user continue page flipping after current chapter ends (currently only one Chapter can be read at time)
        //    if(StaticDataForPageChange.newChapterStarting)

        }

        /// <summary>
        /// Generate left page
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <returns>canvas, left canvas to renderBook()</returns>
        private Canvas generateLeftCanvas(int pageNumber)
        {
 
                SolidColorBrush whiteBrush = new SolidColorBrush();
                whiteBrush.Color = Colors.White;
                Canvas canvas = new Canvas();
                canvas.Width = 520;
                canvas.Height = 720;
                canvas.Background = whiteBrush;
                canvas.Margin = new Thickness(0, 0, 0, 0);

                WebBrowser wbLeftPage = new WebBrowser();
                wbLeftPage.NavigateToString(splittedPages[pageNumber]); 
                wbLeftPage.Width = 490;
                wbLeftPage.Height = 700;
                wbLeftPage.Margin = new Thickness(8, 8, 0, 0);
                wbLeftPage.Foreground = new SolidColorBrush(Colors.Magenta);

                // set the padding area to top and to left
                wbLeftPage.SetValue(Canvas.LeftProperty, 20.00);
                wbLeftPage.SetValue(Canvas.TopProperty, 50.00);

                canvas.Children.Add(wbLeftPage);

                Rectangle rectangle = new Rectangle();
                rectangle.Width = 32;
                rectangle.Height = 720;
                rectangle.Fill = this.Resources["LeftShadow"] as LinearGradientBrush;
                rectangle.Margin = new Thickness(490, 0, 0, 0);
                canvas.Children.Add(rectangle);

                return canvas;
        }

        /// <summary>
        /// Generate right page
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <returns>canvas, right canvas to renderBook()</returns>
        private Canvas generateRightCanvas(int pageNumber)
        {
 
                SolidColorBrush whiteBrush = new SolidColorBrush();
                whiteBrush.Color = Colors.White;
                Canvas canvas = new Canvas();
                canvas.Width = 520;
                canvas.Height = 720;
                canvas.Background = whiteBrush;
                canvas.Margin = new Thickness(520, 0, 0, 0);

                // Amu
                WebBrowser wbRightPage = new WebBrowser();
                
                // If there is odd number of pages last page in right will be empty (in future it will be filled with next chapter's first page)
                if (!(parser.pageCount < (pageNumber + 1)))
                {
                    wbRightPage.NavigateToString(splittedPages[pageNumber]);
                }
                wbRightPage.Width = 490;
                wbRightPage.Height = 700;
                wbRightPage.Margin = new Thickness(0, 8, 0, 0);
                wbRightPage.Foreground = new SolidColorBrush(Colors.Magenta);

                //   set the padding area to top and to left
                wbRightPage.SetValue(Canvas.LeftProperty, 20.00);
                wbRightPage.SetValue(Canvas.TopProperty, 50.00);
                canvas.Children.Add(wbRightPage);

                Rectangle rectangle = new Rectangle();
                rectangle.Width = 32;
                rectangle.Height = 720;
                rectangle.Fill = this.Resources["RightShadow"] as LinearGradientBrush;
                rectangle.Margin = new Thickness(0, 0, 0, 0);
                canvas.Children.Add(rectangle);

                return canvas;

        }

        /// <summary>
        /// renderBook()
        /// Add the content to pages
        /// </summary>
        public void renderBook(int pages)
        {
            int count = 0; // pages - 2;
            int total = pages + 2; // parser.pageCount;
     
            Canvas leftCanvas = new Canvas(), rightCanvas = new Canvas();

            // Lets ensure that we stop page creation in time
            if (parser.pageCount >= (pages + 1))
            {
                // Lets make current and next spread (aukeama)
                for (; pages <= total; pages += 2) //parser.pageCount - 1; count += 2)
                {
                    // Lets skip the last rounds if there is no content
                    if (parser.pageCount >= (pages + 1))
                    {
                        leftCanvas = generateLeftCanvas(pages);
                        PageTurnCanvas.Children.Add(leftCanvas);
                    }
                    // Note: last right page will be empty if there was no content (right page always Even and left page Odd) 
                    // if 17 pages 18'th page will be empty (in this moment - later it will be filled with first page of next chapter)
                    // Lets skip the last rounds if there was no content
                    if (parser.pageCount >= (pages + 2))
                    {
                        rightCanvas = generateRightCanvas(pages + 1);
                        PageTurnCanvas.Children.Add(rightCanvas);
                    }
                    // Mouse event handlings for left and right Canvases
                    pageTurn.AddSpread(leftCanvas, rightCanvas);

                }
            }
            StaticDataForPageChange.currentMaxPageNumber = total + 2; // total starts from zero thats why 
            pageTurn.Initialize(PageTurnCanvas);
            pageTurn.Sensitivity = 1.0; // 1.2;

            PageTurnCanvas.Visibility = Visibility.Visible;
            
        }
        

        /// <summary>
        /// FadeOutAndCollapse() - not used anymore (Amu 27.04.2012)
        /// Shows the chapter - name text - box few seconds on the view every time user turn page
        /// </summary>
        /// <param name="target"></param>
        public void FadeOutAndCollapse(UIElement target)
        {
            DoubleAnimation da = new DoubleAnimation();
            da.From = 1.0;
            da.To = 0.0;
            da.Duration = TimeSpan.FromSeconds(3.00);
            da.AutoReverse = false;

            Storyboard.SetTargetProperty(da, new PropertyPath("Opacity"));
            Storyboard.SetTarget(da, target);

            Storyboard sb = new Storyboard();
            sb.Children.Add(da);

            EventHandler eh = null;
            eh = (s, args) =>
            {
                target.Visibility = Visibility.Collapsed;
                sb.Stop();
                sb.Completed -= eh;
            };
            sb.Completed += eh;

            sb.Begin();
        }


    }

}