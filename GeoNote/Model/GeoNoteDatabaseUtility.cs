using GeoNote.Model.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoNote.Model
{
    public class GeoNoteDatabaseUtility : DatabaseUtility
    {

		static string databaseFileName = "isostore:GeoNote.sdf";

		public GeoNoteDatabaseUtility(string databaseFileName = null)
			: base(databaseFileName ?? GeoNoteDatabaseUtility.databaseFileName)
		{
			/* Intentionally left blank. */
		}
		
		public static string DefaultDatabaseFileName
		{
			get
			{
				return databaseFileName;
			}
			set
			{
				databaseFileName = value;
			}
		}

		public GeoNoteDataContext CreateContext()
		{
			GeoNoteDataContext result = new GeoNoteDataContext(ConnectionString);
			return result;
		}

		public void InitializeDatabase(bool wipe = false)
		{
			GeoNoteDataContext dataContext = new GeoNoteDataContext(ConnectionString);
			InitializeDatabase(dataContext, wipe);
		}
	}
}
