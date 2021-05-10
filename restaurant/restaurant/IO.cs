using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using System.IO;

namespace restaurant
{
    public partial class IO
    {
        
        private Database database = new Database();
        
        #region Database Functionality
        /// <summary>
        /// Opslaan van de database
        /// </summary>
        /// <param name="database">De database die moet worden opgeslagen</param>
        public void Savedatabase(Database database)
        {
            if (!FileSystem.DirectoryExists(@"..\database\"))
            {
                FileSystem.CreateDirectory(@"..\database\");
            }


            string output = JsonConvert.SerializeObject(database, Formatting.Indented);
            // @ neemt tekst letterlijk, geen \n bijv.
            File.WriteAllText(@"..\database\database.Json", output);
        }
        
        /// <summary>
        /// Voor het ophalen van de database
        /// </summary>
        /// <returns>Returned de Database</returns>
        public Database GetDatabase()
        {
            Database database = new Database();

            if (!File.Exists(@"..\database\database.Json")) return database;
            string output = File.ReadAllText(@"..\database\database.Json");
            database = JsonConvert.DeserializeObject<Database>(output);

            List<Tafels> temp = new List<Tafels>();
            for (int i = 0; i < 100; i++)
            {
                Tafels tafel = new Tafels
                {
                    ID = i,
                    Zetels = 4
                };

                if (i % 2 != 0) tafel.isRaam = true;

                temp.Add(tafel);
            }
            database.tafels = temp;

            return database;
        }

        /// <summary>
        /// Reset de database
        /// </summary>
        public void ResetFilesystem()
        {
            try
            {
                FileSystem.DeleteDirectory(@"..\database\", DeleteDirectoryOption.DeleteAllContents);
            }
            catch
            {
            }

            if (!FileSystem.DirectoryExists(@"..\database\"))
            {
                FileSystem.CreateDirectory(@"..\database\");
            }
        }
        #endregion

        #region Reservering
        /// <summary>
        /// Alle beschikbare tijden en tafels voor de ingevoerde dag
        /// </summary>
        /// <param name="date">De datum waarop je de beschikbaarheid wilt zien</param>
        /// <returns>Een Tuple met de datum en een list met alle tafels die nog beschikbaar zijn</returns>
        public List<Tuple<DateTime, List<Tafels>>> ReserveringBeschikbaarheid(DateTime date)
        {
            database = GetDatabase();
            //maakt een lijst met tuples die beheert alle beschikbare plekken op int aantal dagen
            List<Tuple<DateTime, List<Tafels>>> beschikbaar = new List<Tuple<DateTime, List<Tafels>>>();

            //vult de List met alle beschikbare momenten en tafels
            DateTime possibleTime = new DateTime(date.Year, date.Month, date.Day, 10, 0, 0);

            //45 kwaterieren van 1000 tot 2100
            for (int i = 0; i < 45; i++)
            {
                beschikbaar.Add(Tuple.Create(possibleTime, database.tafels));
                possibleTime = possibleTime.AddMinutes(15);
            }

            //voor elke reservering die gemaakt is
            foreach (var reservering in database.reserveringen)
            {
                //op de dag die is ingevoerd, pak alle beschikbare tijden en tafels
                if (reservering.datum >= new DateTime(date.Year, date.Month, date.Day, 10, 0, 0) && reservering.datum <= new DateTime(date.Year, date.Month, date.Day, 21, 0, 0))
                {
                    beschikbaar = VerwijderReservering(beschikbaar, reservering);
                }
            }
            return beschikbaar;
        }

        /// <summary>
        /// Alle beschikbare tijden tussen de ingevoerde maanden en dagen
        /// </summary>
        /// <param name="start_maand">De maand waar je begint</param>
        /// <param name="eind_maand">De maand waar je eindigt</param>
        /// <param name="start_dag">De dag waar je begint</param>
        /// <param name="eind_dag">De dag waar je eindigt</param>
        /// <returns>Een Tuple met de datum en een list met alle tafels die nog beschikbaar zijn</returns>
        public List<Tuple<DateTime, List<Tafels>>> ReserveringBeschikbaarheid(int start_maand, int eind_maand, int start_dag, int eind_dag)
        {
            database = GetDatabase();
            //maakt een lijst met tuples die beheert alle beschikbare plekken op int aantal dagen
            List<Tuple<DateTime, List<Tafels>>> beschikbaar = BerekenTotaleBeschikbaarheid(start_maand, eind_maand, start_dag, eind_dag);


            //verantwoordelijk voor het communiceren met de database
            foreach (var reservering in database.reserveringen)
            {
                //voor de datum tussen nu en de ingevoerde dag
                if (reservering.datum >= new DateTime(DateTime.Now.Year, start_maand, start_dag, 10, 0, 0) && reservering.datum <= new DateTime(DateTime.Now.Year, eind_maand, eind_dag, 21, 0, 0))
                {
                    beschikbaar = VerwijderReservering(beschikbaar, reservering);
                }
            }
            return beschikbaar;
        }

        //verwijderd de reservering uit beschikbaar en returned beschikbaar
        private List<Tuple<DateTime, List<Tafels>>> VerwijderReservering(List<Tuple<DateTime, List<Tafels>>> beschikbaar, Reserveringen reservering)
        {
            //bevat alle tafels
            List<Tafels> tempTableList = new List<Tafels>();
            //bevat alle tafels die al geboekt zijn en haalt die later weg
            List<Tafels> removed_tables = new List<Tafels>();
            //slaat de locatie op van 
            int location = 0;
            
            //gaat door alle gereserveerde tafels in die reservering en haalt deze weg
            for (int d = 0; d < beschikbaar.Count; d++)
            {
                //als beschikbare datum staat gelijk aan een datum met een reservering
                if (beschikbaar[d].Item1 == reservering.datum)
                {
                    tempTableList = new List<Tafels>(beschikbaar[d].Item2);
                    //voor alle tafels die een reservering hebben op die tijd, haal die weg uit beschikbaar en voeg die toe aan removed_tabels
                    foreach (var tafel in reservering.tafels)
                    {
                        tempTableList.Remove(tafel);
                        removed_tables.Add(tafel);
                    }
                    location = d;
                    //stopt de if statement en gaat verder door beschikbaar
                    break;
                }
            }

            //als er geen tafels meer vrij zijn haalt hij de tafel weg
            if (tempTableList.Count == 0)
            {
                beschikbaar.RemoveAt(location);
            }
            //maakt tuple met tafels die wel beschikbaar zijn
            else
            {
                //maakt op locatie in beschikbaar, een tuple met reservering datum en alle ongeboekte tafels
                beschikbaar[location] = Tuple.Create(reservering.datum, tempTableList);
                // ,1-8 want er zitten 8 kwartieren in 2uur
                for (int b = 1; b <= 8; b++)
                {
                    //als location+b out of range gaat, break
                    if ((location + b) >= beschikbaar.Count) break;
                    //haalt alle tafels uit beschikbaar die in removed_tables staan
                    beschikbaar[location + b] = Tuple.Create(reservering.datum.AddMinutes(15 * b), beschikbaar[location + b].Item2.Except(removed_tables).ToList());
                    //als er helemaal geen tafels meer beschikbaar zijn voor een gegeven tijd, haal die weg
                    if (beschikbaar[location + b].Item2.Count == 0)
                    {
                        beschikbaar.RemoveAt(location + b);
                    }
                }
            }
            return beschikbaar;
        }
        
        //maakt alle mogelijke plekken aan voor een starttijd en een eindtijd
        private List<Tuple<DateTime, List<Tafels>>> BerekenTotaleBeschikbaarheid(int start_maand, int eind_maand, int start_dag, int eind_dag)
        {
            List<Tuple<DateTime, List<Tafels>>> beschikbaar = new List<Tuple<DateTime, List<Tafels>>>();

            //vult de List met alle beschikbare momenten en tafels
            DateTime possibleTime = new DateTime(DateTime.Now.Year, start_maand, start_dag, 10, 0, 0);
            for (int maanden = start_maand; maanden <= eind_maand; maanden++)
            {
                for (int days = start_dag; days <= eind_dag; days++)
                {
                    //gaat naar de volgende dag met de openingsuren
                    possibleTime = new DateTime(DateTime.Now.Year, maanden, days, 10, 0, 0);
                    //45 kwaterieren van 1000 tot 2100
                    for (int i = 0; i < 45; i++)
                    {
                        //voegt alle beschikbare tijden en tafels aan beschikbaar voor ieder kwartier
                        beschikbaar.Add(Tuple.Create(possibleTime, database.tafels));
                        possibleTime = possibleTime.AddMinutes(15);
                    }
                }
                possibleTime = new DateTime(DateTime.Now.Year, maanden, start_dag, 10, 0, 0);
            }
            return beschikbaar;
        }
        #endregion

        #region Get Reviews&Feedback
        /// <summary>
        /// Ophalen van alle reviews
        /// </summary>
        /// <returns>Een list met alle reviews</returns>
        public List<Review> GetReviews()
        {
            database = GetDatabase();
            if (database.reviews != null)
            {
                return database.reviews;
            }
            return new List<Review>();
        }

        /// <summary>
        /// Ophalen van een bepaald aantal reviews
        /// </summary>
        /// <param name="max">de hoeveelheid reviews die hij moet ophalen</param>
        /// <returns>Een list met max aantal reviews </returns>
        public List<Review> GetReviews(int max)
        {
            database = GetDatabase();
            List<Review> reviewList = new List<Review>();
            if (database.reviews == null)
            {
                return reviewList;
            }
            else
            {
                //laat max reviews zien, als er zoveel zijn
                for (int i = 0, j = 0; i < database.reviews.Count && j < max; i++, j++)
                {
                    reviewList.Add(database.reviews[i]);
                }
                return reviewList;
            }
        }

        /// <summary>
        /// Ophalen van alle reviews van een klant
        /// </summary>
        /// <param name="klant">de klant waarvan je de reviews wilt hebben</param>
        /// <returns>Een list met alle reviews van een klant</returns>
        public List<Review> GetReviews(Klantgegevens klant)
        {
            //pakt de database
            database = GetDatabase();
            List<Review> reviewList = new List<Review>();
            //voor iedere review met hetzelfde klantnummer als de gegeven klant, voeg deze toe aan de lijst en return de lijst
            foreach (var review in database.reviews)
            {
                if (review.Klantnummer == klant.klantnummer)
                {
                    reviewList.Add(review);
                }
            }
            return reviewList;
        }

        /// <summary>
        /// Ophalen van alle feedback
        /// </summary>
        /// <returns>Een list met alle feedback</returns>
        public List<Review> GetFeedback()
        {
            database = GetDatabase();
            if (database.reviews != null)
            {
                return database.reviews;
            }
            return new List<Review>();
        }

        /// <summary>
        /// Ophalen van een aantal feedback
        /// </summary>
        /// <param name="max">De maximale aantal van feedback die je ophaalt</param>
        /// <returns>Een list met max aantal feedback</returns>
        public List<Feedback> GetFeedback(int max)
        {
            //pakt de database
            database = GetDatabase();
            //maakt een lege feedbacklijst aan
            List<Feedback> feedbackList = new List<Feedback>();
            //als er geen feedback is return de lege lijst
            if (database.feedback == null)
            {
                return feedbackList;
            }
            else
            {
                //sla tot 50 items op en return deze lijst
                for (int i = 0, j = 0; i < database.feedback.Count && j < max; i++, j++)
                {
                    feedbackList.Add(database.feedback[i]);
                }
                return feedbackList;
            }
        }

        /// <summary>
        /// Ophalen van alle feedback van een klant
        /// </summary>
        /// <param name="klant">De klant waarvan je alle feedback wilt</param>
        /// <returns>Een list met alle feedback van een klant</returns>
        public List<Feedback> GetFeedback(Klantgegevens klant)
        {
            database = GetDatabase();
            List<Feedback> feedbackList = new List<Feedback>();
            //voor iedere feedback met hetzelfde klantnummer als het klantnummer van de klant, voeg deze toe aan een lijst en return de lijst
            foreach (var feedback in database.feedback)
            {
                if (feedback.Klantnummer == klant.klantnummer)
                {
                    feedbackList.Add(feedback);
                }
            }
            return feedbackList;
        }
        #endregion

        #region sorteren
        /// <summary>
        /// ordered reserveringen op ID (ASCENDING)
        /// </summary>
        /// <returns>De database geordened op ID</returns>
        public Database OrderReserveringID(Database database)
        {
            //ordered bij een lambda, in dit geval ID
            database.reserveringen = database.reserveringen.OrderBy(s => s.ID).ToList();
            return database;
        }

        /// <summary>
        /// ordered reserveringen op datum (ASCENDING)
        /// </summary>
        /// <returns>De database geordened op datum</returns>
        public Database OrderReserveringDatum(Database database)
        {
            //ordered bij een lambda, in dit geval datum
            database.reserveringen = database.reserveringen.OrderBy(s => s.datum).ToList();
            return database;
        }
        #endregion

        /// <summary>
        /// Ophalen van klantgegevens met ID
        /// </summary>
        /// <param name="ID">Het ID van de klant</param>
        /// <returns>Klantgegevens van de klant</returns>
        public List<Klantgegevens> GetCustomer(List<int> ID)
        {
            database = GetDatabase();

            if (database.login_gegevens != database.login_gegevens.OrderBy(s => s.klantgegevens.klantnummer).ToList())
            {
                database.login_gegevens = database.login_gegevens.OrderBy(s => s.klantgegevens.klantnummer).ToList();
                Savedatabase(database);
            }

            List<Klantgegevens> klantgegevens = new List<Klantgegevens>();

            for (int a = 0; a < ID.Count; a++)
            {
                klantgegevens.Add(database.login_gegevens[ID[a]].klantgegevens);
            }
            
            return klantgegevens;
        }

        #region Deprecated
        [Obsolete("Reservering_beschikbaarheid graag vervangen met ReserveringBeschikbaarheid.")]
        public List<Tuple<DateTime, List<Tafels>>> Reservering_beschikbaarheid(DateTime date)
        {
            database = GetDatabase();
            //maakt een lijst met tuples die beheert alle beschikbare plekken op int aantal dagen
            List<Tuple<DateTime, List<Tafels>>> beschikbaar = new List<Tuple<DateTime, List<Tafels>>>();

            //vult de List met alle beschikbare momenten en tafels
            DateTime possibleTime = new DateTime(date.Year, date.Month, date.Day, 10, 0, 0);

            //45 kwaterieren van 1000 tot 2100
            for (int i = 0; i < 45; i++)
            {
                beschikbaar.Add(Tuple.Create(possibleTime, database.tafels));
                possibleTime = possibleTime.AddMinutes(15);
            }

            //voor elke reservering die gemaakt is
            foreach (var reservering in database.reserveringen)
            {
                //op de dag die is ingevoerd, pak alle beschikbare tijden en tafels
                if (reservering.datum >= new DateTime(date.Year, date.Month, date.Day, 10, 0, 0) && reservering.datum <= new DateTime(date.Year, date.Month, date.Day, 21, 0, 0))
                {
                    beschikbaar = VerwijderReservering(beschikbaar, reservering);
                }
            }
            return beschikbaar;
        }
        
        [Obsolete("Reset_filesystem graag vervangen met ResetFilesystem.")]
        public void Reset_filesystem()
        {
            try
            {
                FileSystem.DeleteDirectory(@"..\database\", DeleteDirectoryOption.DeleteAllContents);
            }
            catch
            {
            }

            if (!FileSystem.DirectoryExists(@"..\database\"))
            {
                FileSystem.CreateDirectory(@"..\database\");
            }
        }
        
        [Obsolete("Getdatabase graag vervangen met GetDatabase.")]
        public Database Getdatabase()
        {
            Database database = new Database();

            if (!File.Exists(@"..\database\database.Json")) return database;
            string output = File.ReadAllText(@"..\database\database.Json");
            database = JsonConvert.DeserializeObject<Database>(output);

            List<Tafels> temp = new List<Tafels>();
            for (int i = 0; i < 100; i++)
            {
                Tafels tafel = new Tafels
                {
                    ID = i,
                    Zetels = 4
                };

                if (i % 2 != 0) tafel.isRaam = true;

                temp.Add(tafel);
            }
            database.tafels = temp;

            return database;
        }
        #endregion
    }
}