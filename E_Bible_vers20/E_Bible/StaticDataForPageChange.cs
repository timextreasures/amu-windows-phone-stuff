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

namespace E_Bible
{
    public class StaticDataForPageChange
    {
        // made for getting data during page change - first page change lead to page's constructor and all neww data will be lost
        // http://forums.create.msdn.com/forums/p/66396/573232.aspx
        public static String data = "";
        public static String htmlMainViewContent = "";
        public static String BibleBook = "";
        public static String bookHeader = "";
        public static String htmlName = "";
        public static List<glossaryInfo> ChapterNameList = null;

        public static bool textContentIsReady = false;

        // PageTurnerPage.xaml.cs and PageTurner
        public static TextBox pageNumTxtBoxLeft = null;
        public static TextBox pageNumTxtBoxRight = null;

        // MainPage.xaml.cs
        public static String htmlContentFolderName = "";

        // PageTurn.cs
        public static int leftPageNumber = 0;
        public static int currentMaxPageNumber = 0;
        public static int amountOfPages = 0;
        public static bool morePages = false;
        public static bool newChapterStarting = false;
    }
}
