﻿#pragma checksum "C:\Users\ivanh\Downloads\GeoNote\GeoNote\GeoNote\GeoNote\NoteDetailsPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "CC6A411E7897C5941BD81DFF7BD741D3"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
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


namespace GeoNote {
    
    
    public partial class NoteDetailsPage : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal Microsoft.Phone.Shell.ApplicationBar appBar;
        
        internal Microsoft.Phone.Shell.ApplicationBarIconButton deleteBtn;
        
        internal Microsoft.Phone.Shell.ApplicationBarIconButton editBtn;
        
        internal Microsoft.Phone.Shell.ApplicationBarIconButton saveBtn;
        
        internal Microsoft.Phone.Shell.ApplicationBarIconButton setDestButton;
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.Grid ContentPanel;
        
        internal System.Windows.Controls.TextBlock txtDate;
        
        internal System.Windows.Controls.TextBlock txtText;
        
        internal System.Windows.Controls.TextBox txtTextEdit;
        
        internal System.Windows.Controls.Button btnPlay;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/GeoNote;component/NoteDetailsPage.xaml", System.UriKind.Relative));
            this.appBar = ((Microsoft.Phone.Shell.ApplicationBar)(this.FindName("appBar")));
            this.deleteBtn = ((Microsoft.Phone.Shell.ApplicationBarIconButton)(this.FindName("deleteBtn")));
            this.editBtn = ((Microsoft.Phone.Shell.ApplicationBarIconButton)(this.FindName("editBtn")));
            this.saveBtn = ((Microsoft.Phone.Shell.ApplicationBarIconButton)(this.FindName("saveBtn")));
            this.setDestButton = ((Microsoft.Phone.Shell.ApplicationBarIconButton)(this.FindName("setDestButton")));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.ContentPanel = ((System.Windows.Controls.Grid)(this.FindName("ContentPanel")));
            this.txtDate = ((System.Windows.Controls.TextBlock)(this.FindName("txtDate")));
            this.txtText = ((System.Windows.Controls.TextBlock)(this.FindName("txtText")));
            this.txtTextEdit = ((System.Windows.Controls.TextBox)(this.FindName("txtTextEdit")));
            this.btnPlay = ((System.Windows.Controls.Button)(this.FindName("btnPlay")));
        }
    }
}

