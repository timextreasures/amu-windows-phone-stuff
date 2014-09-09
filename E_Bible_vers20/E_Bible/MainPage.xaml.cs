// E_Bible_vers19 - continue branch 1 (vers17) (E_Bible_vers18 was IsolateStorage experiment (branch 2) for getting link pictures also to show in html pages)
// rendering pages in one turn changed from 4 to 2 (works more smooth now?)
// Some TODO fixings
// Add error note if toc.ncx file cnat be founded from ePub
// Add fourth book - icon: KJV Bible

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
using System.Xml;
using System.Reflection;
using System.Xml.Linq;  // AMU for parsering HTML needs also reference System.Xml.Ling.dll (XDocument)
using HtmlAgilityPack;
using System.Windows.Media.Imaging;

namespace E_Bible
{
    public partial class MainPage : PhoneApplicationPage
    {
        private StreamResourceInfo _zipPackageResInfo;
        private String _opfPath = "", _tocPath;
        private PageTurnerPage _pageTurner = new PageTurnerPage();

        // Default ePub 
        private String _ePubFile = "ub-SRV.epub";
        // Constructor
        public MainPage()
        {
            
            InitializeComponent();

            // Set the backround picture to mainview 
            ImageBrush iB = new ImageBrush();
            iB.ImageSource = (ImageSource)new ImageSourceConverter().ConvertFromString("images/LakeView-withText.png");
            //Other way to set the background pic even though didnt work
            // iB.ImageSource = new BitmapImage(new Uri("/images/mainBackground-2.png", UriKind.Relative));

            LayoutRoot.Background = iB;
        }

        /// <summary>
        /// Event method when some book icon has been clicked
        /// Choose what book's glossary will be handled. 
        /// TODO: fix later - now only hard coded books are available, in future books can be added and removed by Zune ??
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bookIconClick(object sender, RoutedEventArgs e)
        {
            // TODO test opacy 0.5 for Grid and opacy 1.0 to WebBrowser (background pic will be on the back of glossary)
            // 17.04 Amu - Start main glossary view will be hided and HTML_Content_onMainPage (glossary of opened book) will appear (Visible)
            HTML_Content_onMainPage.Visibility = System.Windows.Visibility.Visible;
            HTML_Content_onMainPage.Background.Opacity = 0.7;

            //     ContentPanel.Background
            ContentPanel.Opacity = 0.5;

            Button whichButton = sender as Button;
            String name = whichButton.Name;

            String data = e.OriginalSource.ToString();

            if (name == "firstButton")
            {
                _ePubFile = "doyle-lost-world.epub";

                // TODO posible background modification in future - book's first page on bottom of glossary
                //ImageBrush iB = new ImageBrush();
                //iB.ImageSource = (ImageSource)new ImageSourceConverter().ConvertFromString("images/bookImage-NotFound.png");
                //HTML_Content_onMainPage.Background = iB;
            }
            if (name == "secondButton")
            {
                _ePubFile = "A VOYAGE TO LILLIPUT.epub";
            }
 
            if (name == "thirdButton")
            {
                _ePubFile = "ub-SRV.epub";
            }

            if (name == "fourthdButton")
            {
                _ePubFile = "pg10.epub";
            }

            initializeGlossaryView();
        }

        /// <summary>
        /// Initializing the glossary view for choosed book
        /// </summary>
        /// <param name="firstInit"></param>
        public void initializeGlossaryView()
        {
            _zipPackageResInfo = getPackageInfo();

            bool getPathOK = false;

            _tocPath = getPath(_zipPackageResInfo, "toc.ncx", out getPathOK);

            if (!getPathOK)
            {
                Parser parser = new Parser();
                string errorNote = "<p><center>Error: Cant set path to toc.nxc file, file is propably not part of ePub</center></p>";

                HTML_Content_onMainPage.NavigateToString(errorNote);
                StaticDataForPageChange.htmlMainViewContent = errorNote;
            }

            else
                setGlossaryView();

        }

        /// <summary>
        /// Get EPUB - zip package resource Info
        /// </summary>
        /// <returns></returns>
        private StreamResourceInfo getPackageInfo()
        {
  
            // Alternative way: Orifginal EPUB "pg10.epub works also OK with next syntax when Build action property for pg10.epub is "Resource"!!
            System.Windows.Resources.StreamResourceInfo zipInfo = Application.GetResourceStream(new Uri("E_Bible;component/Resources/pg10.epub", UriKind.Relative));

            // WORKS OK WHEN PROPERTIES ARE CONTENT (also accessing zipped content)  
            System.Windows.Resources.StreamResourceInfo zipInfox = Application.GetResourceStream(new Uri("Resources/" + _ePubFile, UriKind.Relative));

            return new StreamResourceInfo(zipInfox.Stream, null); // TODO does this returning "new" instance work OK?
        }

        /// <summary> **********************************************
        /// Get file path info
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        public String getPath(StreamResourceInfo package, String fileName, out bool everythingOK)
        {
            BinaryReader reader = new BinaryReader(package.Stream);
            package.Stream.Seek(0, SeekOrigin.Begin);

            string name = null;

            while (ParseFileHeader(reader, out name))
            {
                // If file founded, also folder-name saved if file is under folder
                if (name.Contains(fileName))
                {
                    // Save the folder - name if there was sub - folder for HTML content
                    if (name.Contains("/"))
                        StaticDataForPageChange.htmlContentFolderName = name.Substring(0, name.IndexOf("/") + 1); 
                    else
                        StaticDataForPageChange.htmlContentFolderName = "";
                    everythingOK = true;
                    return name;
                }
            }
            everythingOK = false;
            return "File path info not found"; 
        }

        /// <summary>
        /// Aid method for getting the file path
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        private static bool ParseFileHeader(BinaryReader reader, out string filename)
        {
            filename = null;
            if (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                int headerSignature = reader.ReadInt32();
                if (headerSignature == 67324752) //PKZIP
                {
                    reader.BaseStream.Seek(14, SeekOrigin.Current); //ignore unneeded values

                    int compressedSize = reader.ReadInt32();
                    int unCompressedSize = reader.ReadInt32();
                    short fileNameLenght = reader.ReadInt16();

                    short extraFieldLenght = reader.ReadInt16();
                    filename = new string(reader.ReadChars(fileNameLenght));

                    if (string.IsNullOrEmpty(filename))
                        return false;

                    //Seek to the next file header
                    reader.BaseStream.Seek(extraFieldLenght + compressedSize, SeekOrigin.Current);
                    if (unCompressedSize == 0) //Directory or not supported. Skip it
                        return ParseFileHeader(reader, out filename);
                    else
                        return true;
                }

            }
            return false;

        }
       
        /// <summary>
        /// Get the OPF file out from zip package 
        /// Parse the OPF file for generating the hyperlink - list (mainView glossary) of the Bible's / book's chapters. 
        /// Set the content to static String parameter
        /// </summary>
        /// <returns></returns>
        private void setGlossaryView() 
        {
            // idea parsing content.opf (as EPUB specs says: http://stackoverflow.com/questions/4283170/iphone-reading-epub-files)
            Stream tocFileStream = getFileStreamFromEPub(_tocPath);
            StreamReader contentTocreader = new StreamReader(tocFileStream);
            string tocContent = contentTocreader.ReadToEnd();

            Parser parser = new Parser();
            string glossaryContent = parser.mainGlossaryViewFromTOCfile(tocContent);

            HTML_Content_onMainPage.NavigateToString(glossaryContent);
            StaticDataForPageChange.htmlMainViewContent = glossaryContent;

        }

        private Stream getFileStreamFromEPub(String pathAndFile)
        {
            Stream fileStream = Application.GetResourceStream(getPackageInfo(), new Uri(pathAndFile, UriKind.Relative)).Stream;
            return fileStream;
        }

        /// <summary>
        ///  When hyperlink clicked flow jumps to here
        ///  Lets save all needed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void navigatingOnBrowser(object sender, NavigatingEventArgs e)
        {

            // Lets keep the original content on browser page:
            HTML_Content_onMainPage.NavigateToString(StaticDataForPageChange.htmlMainViewContent);
            // jump to TextBlock page - this has been done in TextBlockPage.cs
            // NOTICE THAT CONSTRUCTOR OF PAGE IS CALLED EVERYTIME WHEN THIS HAS BEEN CALLED !!
            NavigationService.Navigate(new Uri("/PageTurnerPage.xaml", UriKind.Relative));

            // Save the name of clicked HTML
            Uri link = e.Uri;

            // Lets set the Book header which will be shown on the bottom with current page / amount of pages text box
            for(int i = 0; i < StaticDataForPageChange.ChapterNameList.Count(); i++)
            {
                glossaryInfo gI = StaticDataForPageChange.ChapterNameList[i];
                if(gI.content == link.AbsolutePath.Replace("%20", " ")) // if HTML contains spaces those are previewed in AbsolutePath as "20%"
                    StaticDataForPageChange.bookHeader = gI.header;
            }
            glossaryInfo gg = StaticDataForPageChange.ChapterNameList[2];
            
            // break the uri to pieces it contains 2 parts separated with ":" sometimes contains third part separated with "#" as: "about:htmlpage.html#linkInsideTheText"
            String[] nameSplit = link.AbsoluteUri.Split(new Char[] { ':', '#' }); // about:www.guttenberg ... #<book name>

            _pageTurner.saveCurrentFlipModeContent(StaticDataForPageChange.htmlContentFolderName + nameSplit[1], _zipPackageResInfo);

            StaticDataForPageChange.textContentIsReady = true;
        }


    }
}