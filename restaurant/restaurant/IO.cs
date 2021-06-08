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

            string output = JsonConvert.SerializeObject(database.eigenaar, Formatting.Indented);
            File.WriteAllText(@"..\database\eigenaar.Json", output);

            output = JsonConvert.SerializeObject(database.feedback, Formatting.Indented);
            File.WriteAllText(@"..\database\feedback.Json", output);

            output = JsonConvert.SerializeObject(database.ingredienten, Formatting.Indented);
            File.WriteAllText(@"..\database\ingredienten.Json", output);

            output = JsonConvert.SerializeObject(database.ingredientenNamen, Formatting.Indented);
            File.WriteAllText(@"..\database\ingredientenNamen.Json", output);

            output = JsonConvert.SerializeObject(database.inkomsten, Formatting.Indented);
            File.WriteAllText(@"..\database\inkomsten.Json", output);

            output = JsonConvert.SerializeObject(database.login_gegevens, Formatting.Indented);
            File.WriteAllText(@"..\database\klantgegevens.Json", output);

            output = JsonConvert.SerializeObject(database.menukaart, Formatting.Indented);
            File.WriteAllText(@"..\database\menukaart.Json", output);

            output = JsonConvert.SerializeObject(database.reserveringen, Formatting.Indented);
            File.WriteAllText(@"..\database\reserveringen.Json", output);

            output = JsonConvert.SerializeObject(database.reviews, Formatting.Indented);
            File.WriteAllText(@"..\database\reviews.Json", output);

            output = JsonConvert.SerializeObject(database.tafels, Formatting.Indented);
            File.WriteAllText(@"..\database\tafels.Json", output);

            output = JsonConvert.SerializeObject(database.uitgaven, Formatting.Indented);
            File.WriteAllText(@"..\database\uitgaven.Json", output);

            output = JsonConvert.SerializeObject(database.werknemers, Formatting.Indented);
            File.WriteAllText(@"..\database\werknemers.Json", output);
        }
        
        /// <summary>
        /// Voor het ophalen van de hele database
        /// </summary>
        /// <returns>Returned de Database</returns>
        public Database GetDatabase()
        {
            Database database = new Database();

            if (!File.Exists(@"..\database\reserveringen.Json")) return database;
            string output = File.ReadAllText(@"..\database\eigenaar.Json");
            database.eigenaar = JsonConvert.DeserializeObject<Eigenaar>(output);

            output = File.ReadAllText(@"..\database\feedback.Json");
            database.feedback = JsonConvert.DeserializeObject<List<Feedback>>(output);

            output = File.ReadAllText(@"..\database\ingredienten.Json");
            database.ingredienten = JsonConvert.DeserializeObject<List<Ingredient>>(output);

            output = File.ReadAllText(@"..\database\ingredientenNamen.Json");
            database.ingredientenNamen = JsonConvert.DeserializeObject<List<IngredientType>>(output);

            output = File.ReadAllText(@"..\database\inkomsten.Json");
            database.inkomsten = JsonConvert.DeserializeObject<Inkomsten>(output);

            output = File.ReadAllText(@"..\database\klantgegevens.Json");
            database.login_gegevens = JsonConvert.DeserializeObject<List<Login_gegevens>>(output);

            output = File.ReadAllText(@"..\database\menukaart.Json");
            database.menukaart = JsonConvert.DeserializeObject<Menukaart>(output);

            output = File.ReadAllText(@"..\database\reserveringen.Json");
            database.reserveringen = JsonConvert.DeserializeObject<List<Reserveringen>>(output);

            output = File.ReadAllText(@"..\database\reviews.Json");
            database.reviews = JsonConvert.DeserializeObject<List<Review>>(output);

            output = File.ReadAllText(@"..\database\uitgaven.Json");
            database.uitgaven = JsonConvert.DeserializeObject<Uitgaven>(output);

            output = File.ReadAllText(@"..\database\werknemers.Json");
            database.werknemers = JsonConvert.DeserializeObject<List<Werknemer>>(output);

/*            List<Tafels> temp = new List<Tafels>();
            for (int i = 1; i <= 20; i++)
            {
                Tafels tafel = new Tafels
                {
                    ID = i,
                    Zetels = 4
                };

                if (i % 2 != 0) tafel.isRaam = true;

                temp.Add(tafel);
            }
            database.tafels = temp;*/
            
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
            //als reserveringen niet bestaat
            
            //maakt een lijst met tuples die beheert alle beschikbare plekken op int aantal dagen
            List<Tuple<DateTime, List<Tafels>>> beschikbaar = new List<Tuple<DateTime, List<Tafels>>>();

            //maakt een starttijd starttijd
            DateTime possibleTime = new DateTime(date.Year, date.Month, date.Day, 10, 0, 0);

            //44+1 kwaterieren van 1000 tot 2100
            for (int i = 0; i < 45; i++)
            {
                //voegt een tuple toe voor ieder kwartier
                beschikbaar.Add(Tuple.Create(possibleTime, database.tafels));
                possibleTime = possibleTime.AddMinutes(15);
            }

            if (database.reserveringen == null)
            {
                return beschikbaar;
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
            //als reserveringen niet bestaat
            if (database.reserveringen == null)
            {
                return new List<Tuple<DateTime, List<Tafels>>>();
            }
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
                //1-8 want er zitten 8 kwartieren in 2uur
                //1-120 want er zitten 120 minuten in 2 uur
                for (int b = 1; b <= 8; b++)
                {
                    //als location+b out of range gaat, break
                    if ((location + b) >= beschikbaar.Count)
                    {
                        for (int i = 0; i < 8-b; i++)
                        {
                            beschikbaar[location - (b+i)] = Tuple.Create(reservering.datum.AddMinutes(15 * (-b-i)), beschikbaar[location - (b+i)].Item2.Except(removed_tables).ToList());
                            if (beschikbaar[location - (b+i)].Item2.Count == 0)
                            {
                                beschikbaar.RemoveAt(location - (b+i));
                            }
                        }
                        break;
                    }
                    //haalt alle tafels uit beschikbaar die in removed_tables staan
                    //zorgt ervoor dat alle reserveringen binnen de uren blijven (10:00 tot 21:00)
                    if (reservering.datum.AddMinutes(15 * b).Hour < 22)
                    {
                        beschikbaar[location + b] = Tuple.Create(reservering.datum.AddMinutes(15 * b), beschikbaar[location + b].Item2.Except(removed_tables).ToList());
                    }
                    if (location - b >= 0 && reservering.datum.AddMinutes(15 * -b).Hour > 9)
                    {
                        beschikbaar[location - b] = Tuple.Create(reservering.datum.AddMinutes(15 * -b), beschikbaar[location - b].Item2.Except(removed_tables).ToList());
                        if (beschikbaar[location - b].Item2.Count == 0)
                        {
                            beschikbaar.RemoveAt(location - b);
                        }
                    }
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
                    //661 minuten van 1000 tot 2100
                    for (int i = 0; i < 45; i++)
                    {
                        //voegt alle beschikbare tijden en tafels aan beschikbaar voor ieder kwartier
                        beschikbaar.Add(Tuple.Create(possibleTime, database.tafels));
                        possibleTime = possibleTime.AddMinutes(15);
                        //possibleTime = possibleTime.AddMinutes(1);
                    }
                }
                possibleTime = new DateTime(DateTime.Now.Year, maanden, start_dag, 10, 0, 0);
            }
            return beschikbaar;
        }

        /// <summary>
        /// Voor het omzetten van een reservering naar betaald
        /// </summary>
        /// <param name="reserveringen">De reservering die je naam betaald wilt zetten</param>
        public void ReserveringBetalen(Reserveringen reserveringen)
        {
            Database database = GetDatabase();
            for (int i = 0; i < database.reserveringen.Count; i++)
            {
                if (database.reserveringen[i].ID == reserveringen.ID)
                {
                    database.reserveringen[i] = new Reserveringen
                    {
                        ID = reserveringen.ID,
                        datum = reserveringen.datum,
                        klantnummer = reserveringen.klantnummer,
                        aantal = reserveringen.aantal,
                        tafels = reserveringen.tafels,
                        gerechten_ID = reserveringen.gerechten_ID,
                        tafel_bij_raam = reserveringen.tafel_bij_raam,
                        isBetaald = true,
                    };
                    break;
                }
            }
            Savedatabase(database);
        }
        #endregion

        #region Gettters
        /// <summary>
        /// Ophalen van alle reviews, gesorteerd op datum
        /// </summary>
        /// <returns>Een list met alle reviews</returns>
        public List<Review> GetReviews()
        {
            database = GetDatabase();
            if (database.reviews == null)
            {
                return new List<Review>();
            }
            return new List<Review>(database.reviews);
        }

        /// <summary>
        /// Ophalen van een bepaald aantal reviews
        /// </summary>
        /// <param name="max">de hoeveelheid reviews die hij moet ophalen</param>
        /// <returns>Een list met max aantal reviews </returns>
        public List<Review> GetReviews(int max)
        {
            database = GetDatabase();
            
            if (database.reviews == null)
            {
                return new List<Review>();
            }
            else
            {
                List<Review> reviewList = new List<Review>();
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
            if (database.reviews == null)
            {
                return new List<Review>();
            }
            else
            {
                List<Review> reviewList = new List<Review>();
                //voor iedere feedback met hetzelfde klantnummer als het klantnummer van de klant, voeg deze toe aan een lijst en return de lijst
                foreach (var review in database.reviews)
                {
                    if (review.klantnummer == klant.klantnummer)
                    {
                        reviewList.Add(review);
                    }
                }
                return reviewList;
            }
        }

        /// <summary>
        /// Ophalen van alle feedback
        /// </summary>
        /// <returns>Een list met alle feedback</returns>
        public List<Feedback> GetFeedback()
        {
            database = GetDatabase();
            if (database.feedback == null)
            {
                return new List<Feedback>();
            }
            return new List<Feedback>(database.feedback);
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
            //als er geen feedback is return de lege lijst
            if (database.feedback == null)
            {
                return new List<Feedback>();
            }
            else
            {
                List<Feedback> feedbackList = new List<Feedback>();
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
            if(database.feedback == null)
            {
                return new List<Feedback>();
            }
            else
            {
                List<Feedback> feedbackList = new List<Feedback>();
                //voor iedere feedback met hetzelfde klantnummer als het klantnummer van de klant, voeg deze toe aan een lijst en return de lijst
                foreach (var feedback in database.feedback)
                {
                    if (feedback.klantnummer == klant.klantnummer)
                    {
                        feedbackList.Add(feedback);
                    }
                }
                return feedbackList;
            }
        }

        /// <summary>
        /// Ophalen van klantgegevens met ID
        /// </summary>
        /// <param name="ID">Het ID van de klant</param>
        /// <returns>Klantgegevens van de klant</returns>
        public List<Klantgegevens> GetCustomer(List<int> ID)
        {
            database = GetDatabase();
            List<Klantgegevens> klantgegevens = new List<Klantgegevens>();
            if (database.login_gegevens == null)
            {
                database.login_gegevens = new List<Login_gegevens>();
                return klantgegevens;
            }
            else if (database.login_gegevens[0].klantgegevens == null)
            {
                return klantgegevens;
            }
            else
            {
                //als de login_gegevens niet gesorteerd is, sorteer deze op klantnummer en sla deze op
                if (database.login_gegevens != database.login_gegevens.OrderBy(s => s.klantgegevens.klantnummer).ToList())
                {
                    database.login_gegevens = database.login_gegevens.OrderBy(s => s.klantgegevens.klantnummer).ToList();
                    Savedatabase(database);
                }

                //zoek voor klantgegevens op ID in de gegeven list
                for (int a = 0; a < ID.Count; a++)
                {
                    if (ID[a] != -1)
                    {
                        klantgegevens.Add(database.login_gegevens[ID[a]].klantgegevens);
                    }
                    else
                    {
                        klantgegevens.Add(new Klantgegevens());
                    }
                }
                return klantgegevens;
            }
        }

        /// <summary>
        ///  Voor het ophalen van de naam van een medewerker met een ID
        /// </summary>
        /// <param name="employeeID">Het ID van de medewerker</param>
        /// <returns>De voornaam(Item1) en achternaam(Item2) van de medewerker</returns>
        public (string, string) GetEmployee(int employeeID)
        {
            database = GetDatabase();
            if (database.werknemers == null)
            {
                return (null, null);
            }
            for (int i = 0; i < database.werknemers.Count; i++)
            {
                if (database.werknemers[i].ID == employeeID)
                {
                    return (database.werknemers[i].login_gegevens.klantgegevens.voornaam, database.werknemers[i].login_gegevens.klantgegevens.achternaam);
                }
            }
            return (null, null);
        }

        /// <summary>
        /// Voor het ophalen van alle medewerkers
        /// </summary>
        /// <returns>Een list met alle medewerkers</returns>
        public List<Werknemer> GetEmployee()
        {
            database = GetDatabase();
            if (database.werknemers == null)
            {
                return new List<Werknemer>();
            }
            return new List<Werknemer>(database.werknemers);
        }

        /// <summary>
        /// Voor het krijgen van de eigenaar
        /// </summary>
        /// <returns>De eigenaar</returns>
        public Eigenaar GetEigenaar()
        {
            database = GetDatabase();
            if(database.eigenaar == null)
            {
                return new Eigenaar();
            }
            else
            {
                return database.eigenaar;
            }
        }

        /// <summary>
        /// Voor het ophalen van alle reserveringen
        /// </summary>
        /// <returns>Een list met alle reserveringen</returns>
        public List<Reserveringen> GetReservations()
        {
            database = GetDatabase();
            if (database.reserveringen == null)
            {
                return new List<Reserveringen>();
            }
            return new List<Reserveringen>(database.reserveringen);
        }

        /// <summary>
        /// voor het ophalen van alle gerechten die staan bij een reservering
        /// </summary>
        /// <param name="reservering">De reservering</param>
        /// <returns>Een list met alle gerechten</returns>
        public List<Gerechten> GetGerechtenReservering(Reserveringen reservering)
        {
            if (reservering.gerechten_ID == null || reservering.gerechten_ID.Count == 0)
            {
                return new List<Gerechten>();
            }
            database = GetDatabase();

            List<Gerechten> gerechten = new List<Gerechten>();
            //voor alle gerechten van de reservering, voor alle gerechten op de menukaart, als matched, voeg deze toe aan de lijst
            for (int i = 0; i < reservering.gerechten_ID.Count; i++)
            {
                for (int j = 0; j < database.menukaart.gerechten.Count; j++)
                {
                    if (reservering.gerechten_ID[i] == database.menukaart.gerechten[j].ID)
                    {
                        gerechten.Add(database.menukaart.gerechten[j]);
                    }
                }
            }
            return gerechten;
        }

        public List<Dranken> GetDrankenReservering(Reserveringen reservering)
        {
            if (reservering.gerechten_ID == null || reservering.gerechten_ID.Count == 0)
            {
                return new List<Dranken>();
            }
            database = GetDatabase();

            List<Dranken> dranken = new List<Dranken>();
            //voor alle gerechten van de reservering, voor alle gerechten op de menukaart, als matched, voeg deze toe aan de lijst
            for (int i = 0; i < reservering.dranken_ID.Count; i++)
            {
                for (int j = 0; j < database.menukaart.dranken.Count; j++)
                {
                    if (reservering.dranken_ID[i] == database.menukaart.dranken[j].ID)
                    {
                        dranken.Add(database.menukaart.dranken[j]);
                    }
                }
            }
            return dranken;
        }

        /// <summary>
        /// voor het verkrijgen van alle ingrediënten
        /// </summary>
        /// <returns>een list met alle ingrediënten</returns>
        public List<IngredientType> ingredientNamen()
        {
            database = GetDatabase();

            return database.ingredientenNamen;
        }
        #endregion

        #region sorteren
        /// <summary>
        /// Ordered reserveringen in de database op ID (ASCENDING)
        /// </summary>
        /// <param name="database">De database</param>
        /// <returns>De database met de reserveringen geordened op ID</returns>
        public Database OrderReserveringID(Database database)
        {
            //ordered bij een lambda, in dit geval ID
            database.reserveringen = database.reserveringen.OrderBy(s => s.ID).ToList();
            return database;
        }

        /// <summary>
        /// Ordered reserveringen in de database op datum (ASCENDING)
        /// </summary>
        /// <returns>De database geordened op datum</returns>
        public Database OrderReserveringDatum(Database database)
        {
            //ordered bij een lambda, in dit geval datum
            database.reserveringen = database.reserveringen.OrderBy(s => s.datum).ToList();
            return database;
        }

        /// <summary>
        /// Ordered de reviews in de database op datum (ASCENDING)
        /// </summary>
        /// <param name="database">de database</param>
        /// <returns>De database met de reviews geordened op datum</returns>
        public Database OrderReviewDatum(Database database)
        {
            //ordered met een lambda, in dit geval datum
            database.reviews = database.reviews.OrderBy(s => s.datum).ToList();
            return database;
        }

        /// <summary>
        /// Ordered de feedback in de database op datum (ASCENDING)
        /// </summary>
        /// <param name="database">de database</param>
        /// <returns>De database met de feedback geordened op datum</returns>
        public Database OrderFeedbackDatum(Database database)
        {
            //ordered met een lambda, in dit geval datum
            database.feedback = database.feedback.OrderBy(s => s.datum).ToList();
            return database;
        }
        #endregion

        #region Deprecated
        
        #endregion
    }
}