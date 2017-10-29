using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using GeoNote.Model.Entities;
using Infrastructure;
using Infrastructure.Audio;
using GeoNote.Model;
using Microsoft.Phone.Tasks;
using System.Device.Location;

namespace GeoNote
{
    public partial class NoteDetailsPage : PhoneApplicationPage
    {
        private Note displayedNote;

        private bool editingNote;
 
        public NoteDetailsPage()
        {
            InitializeComponent();

            Note passedNote = (Note)NavigationService.GetLastNavigationData();
            displayedNote = GeoNoteDataService.GetNoteById(passedNote.Id); // we need a fresh object from db, as if the one passed was just saved the data provider throws sql exception
            this.DataContext = displayedNote;

            txtDate.Text = "Details for note created on " + displayedNote.TimeCreated.Value.ToShortDateString() + " : " + displayedNote.TimeCreated.Value.ToShortTimeString();            
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            AudioHelpers.Play(displayedNote.AudioFilePath);
        }

        private void deleteBtn_Click(object sender, EventArgs e)
        {
            GeoNoteDataService.DeleteNote(displayedNote);
            txtDate.Text = "Note deleted!";
        }

        private void editBtn_Click(object sender, EventArgs e)
        {
            if (editingNote)
                return;
            
            txtText.Visibility = Visibility.Collapsed;
            txtTextEdit.Visibility = Visibility.Visible;
            txtDate.Text = "Editing note";

            txtTextEdit.Text = displayedNote.Text;
            editingNote = true;
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            if (!editingNote)
                return;

            txtText.Visibility = Visibility.Visible;
            txtTextEdit.Visibility = Visibility.Collapsed;

            displayedNote.Text = txtTextEdit.Text;
            GeoNoteDataService.UpdateChangedEntities();

            txtDate.Text = "Note saved";
            editingNote = false;
        }

        private void mSetDestination_Click(object sender, System.EventArgs e)
        {
            MapsDirectionsTask mapsDirectionsTask = new MapsDirectionsTask();

            // If you set the geocoordinate parameter to null, the label parameter is used as a search term.
            LabeledMapLocation destination = new LabeledMapLocation (displayedNote.Text, new GeoCoordinate(displayedNote.Latitude, displayedNote.Longitude));

            mapsDirectionsTask.End = destination;
            

            // If mapsDirectionsTask.Start is not set, the user's current location is used as the start point.
            //  NavigationService.Navigate("/MainPage.xaml",     

            mapsDirectionsTask.Show();
        }
    }
}