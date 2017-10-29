using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Collections.ObjectModel;
using GeoNote.Model.Entities;
using GeoNote.Model;
using System.Windows.Data;
using Infrastructure;
using System.ComponentModel;

namespace GeoNote
{
    public partial class NotesListPage : PhoneApplicationPage
    {
        private ObservableCollection<Note> notes;

        private CollectionViewSource notesCollectionViewSource;

        public NotesListPage()
        {
            InitializeComponent();
        }

        private void SortByProximity_Click(object sender, System.EventArgs e)
        {
            foreach (Note note in notes)
            {
                note.CurrentLocation = App.CurrentLocation;
            }            

            notesCollectionViewSource.SortDescriptions.Clear();
            notesCollectionViewSource.SortDescriptions.Add(new SortDescription("DistanceToCurrent", ListSortDirection.Ascending)); 
        }

        private void TextBlock_BindingValidationError(object sender, ValidationErrorEventArgs e)
        {

        }

        private void txtFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtFilter.Text.Length == 0)
            {
                notesCollectionViewSource.View.Filter = null;
                return;
            }

            notesCollectionViewSource.View.Filter = new Predicate<Object>(note => ((Note)note).Text.Contains(txtFilter.Text));
        }

        private void notesList_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (notesList.SelectedIndex == -1)
                return;

            NavigationService.Navigate("/NoteDetailsPage.xaml", (Note)notesList.SelectedItem);
        }

        private void mTextOnly_Click(object sender, EventArgs e)
        {
            notesCollectionViewSource.View.Filter = new Predicate<Object>(note => ((Note)note).Text.Length > 0);
        }

        private void mPhotoOnly_Click(object sender, EventArgs e)
        {
            notesCollectionViewSource.View.Filter = new Predicate<Object>(note => ((Note)note).HasImage);
        }

        private void mShowAll_Click(object sender, EventArgs e)
        {
            notesCollectionViewSource.View.Filter = null;
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            notes = new ObservableCollection<Note>(GeoNoteDataService.GetAllNotes());

            notesCollectionViewSource = this.Resources["notesCollectionViewSource"] as CollectionViewSource;
            notesCollectionViewSource.Source = notes;
        }
    }
}