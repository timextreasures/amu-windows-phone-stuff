﻿#pragma checksum "C:\Users\ArtsiM\Documents\Visual Studio 2012\Projects\E_Bible_vers19 - Cleaned official version\E_Bible\PageTurnerPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "BAB223014AB46912B5917B74B0D359F0"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18033
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Phone.Controls;
using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace E_Bible {
    
    
    public partial class PageTurnerPage : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.ScrollViewer horizonScroller;
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.Canvas PageTurnCanvas;
        
        internal System.Windows.Media.ScaleTransform ZoomTransform;
        
        internal System.Windows.Controls.TextBox pageNumbersInLeftPage;
        
        internal System.Windows.Controls.TextBox pageNumbersInRightPage;
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Windows.Application.LoadComponent(this, new System.Uri("/E_Bible;component/PageTurnerPage.xaml", System.UriKind.Relative));
            this.horizonScroller = ((System.Windows.Controls.ScrollViewer)(this.FindName("horizonScroller")));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.PageTurnCanvas = ((System.Windows.Controls.Canvas)(this.FindName("PageTurnCanvas")));
            this.ZoomTransform = ((System.Windows.Media.ScaleTransform)(this.FindName("ZoomTransform")));
            this.pageNumbersInLeftPage = ((System.Windows.Controls.TextBox)(this.FindName("pageNumbersInLeftPage")));
            this.pageNumbersInRightPage = ((System.Windows.Controls.TextBox)(this.FindName("pageNumbersInRightPage")));
        }
    }
}

