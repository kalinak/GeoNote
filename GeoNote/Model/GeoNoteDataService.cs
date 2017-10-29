using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoNote.Model.Entities;

namespace GeoNote.Model
{
    public static class GeoNoteDataService
    {
        static readonly GeoNoteDatabaseUtility geoNoteDatabaseUtility = new GeoNoteDatabaseUtility();
        public static GeoNoteDataContext dataContext;

        static GeoNoteDataService()
        {
            dataContext = geoNoteDatabaseUtility.CreateContext();
        }

        public static void SaveNote(double latitude, double longitude, string text = null, string pictureFilePath = null, string audioFilePath = null)
        {
            Note noteToInsert = new Note
            {
                Latitude = latitude,
                Longitude = longitude,
                Text = text,
                PictureFilePath = pictureFilePath,
                AudioFilePath = audioFilePath,
                TimeCreated = DateTime.Now,
                Id = -100
            };

            var notes = new List<Note> { noteToInsert };
            dataContext.Notes.InsertAllOnSubmit(notes);

            dataContext.SubmitChanges();
        }

        public static IQueryable<Note> GetAllNotes()
        {
            return from n in dataContext.Notes select n;
        }

        public static IQueryable<Note> GetNotesWithIdBiggerThan(int id)
        {
            return from note in dataContext.Notes
                   where note.Id > id
                   select note;
        }

        public static int GetMaxNoteId()
        {
            int count = (from note in dataContext.Notes
                         select note.Id).Count();

            if (count == 0)
                return 0;

            return (from note in dataContext.Notes
                    select note.Id).Max();
        }

        public static int GetNotesCount()
        {
            return (from note in dataContext.Notes
                    select note.Id).Count();
        }

        public static void DeleteNote(Note note)
        {
            dataContext.Notes.DeleteOnSubmit(note);
            dataContext.SubmitChanges();
        }

        public static void UpdateChangedEntities()
        {
            dataContext.SubmitChanges();
        }

        public static Note GetNoteById(int id)
        {
            var queriedNote = from note in dataContext.Notes
                              where note.Id == id
                              select note;

            if (queriedNote.Count() == 1)
                return queriedNote.FirstOrDefault();

            return null;

        }
    }
}
