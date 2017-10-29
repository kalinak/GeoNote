using GeoNote.Model.Entities;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoNote.Model
{
    public class GeoNoteDataContext : DataContext
    {
        public GeoNoteDataContext(string connection)
            : base(connection)
        {
            
        }

        public Table<Note> Notes
        {
            get
            {
                return GetTable<Note>();
            }
        }
    }
}
