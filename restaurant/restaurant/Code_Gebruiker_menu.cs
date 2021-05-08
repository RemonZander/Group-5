using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant
{
    public partial class Code_Gebruiker_menu
    {
        Database database = new Database();
        IO io = new IO();
        Testing_class testClass = new Testing_class();

        public void Debug()
        {
            if (database.menukaart == null)
            {
                database.menukaart = new Menukaart();
                database.menukaart.gerechten = new List<Gerechten>();
                database.menukaart.gerechten.AddRange(testClass.Get_standard_dishes());
            }
            else
            {
                database.menukaart.gerechten = testClass.Get_standard_dishes();
            }
            List<Gerechten> test = GetMenukaart(new List<string> { "lactose intolerantie" });

            List<int> num = new List<int> { 223541, 220793 };
            MakeCustomerReservation(DateTime.Now, num, 6, false);
            num = new List<int> { 223167 };
            MakeCustomerReservation(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day + 3), num, 2, true);
        }

        public Code_Gebruiker_menu()
        {
            database = io.GetDatabase();
        }

        #region Reserveringen
        /// <summary>
        /// Voor het krijgen van de oude of nieuwe reserveringen van een klant
        /// </summary>
        /// <param name="klant">de klant van wie je de reservering wilt ophalen </param>
        /// <param name="toekomstReserveringen">if false, pakt alleen de oude reserveringen. if true, pakt alleen de toekomst reserveringen</param>
        /// <returns>Een list met Reserveringen</returns>
        public List<Reserveringen> GetCustomerReservation(Klantgegevens klant, bool toekomstReserveringen)
        {
            database = io.GetDatabase();
            List<Reserveringen> reservering = new List<Reserveringen>();
            //voor elke reservering in de database, voor elk klantnummer van een reservering
            foreach (var reserveringen in database.reserveringen)
            {
                foreach (var klantnummer in reserveringen.klantnummers)
                {
                    //voegt alleen reserveringen in de toekomst toe
                    if (toekomstReserveringen && klant.klantnummer == klantnummer && reserveringen.datum > DateTime.Now)
                    {
                        reservering.Add(reserveringen);
                    }
                    //voegt alle oude reservering toe van de klant
                    else if (!toekomstReserveringen && klant.klantnummer == klantnummer && reserveringen.datum < DateTime.Now)
                    {
                        reservering.Add(reserveringen);
                    }
                }
            }
            return reservering;
        }

        /// <summary>
        /// Voor het krijgen van alle Reserveringen van een klant
        /// </summary>
        /// <param name="klant">De klant van wie je de reserveringen wil</param>
        /// <returns>Een list met alle reserveringen van de klant</returns>
        public List<Reserveringen> GetCustomerReservation(Klantgegevens klant)
        {
            database = io.GetDatabase();
            List<Reserveringen> reservering = new List<Reserveringen>();
            //voor elke reservering in de database, voor elk klantnummer van een reservering
            foreach (var reserveringen in database.reserveringen)
            {
                foreach (var klantnummer in reserveringen.klantnummers)
                {
                    //voegt alle reserveringen van de klant toe
                    if (klant.klantnummer == klantnummer)
                    {
                        reservering.Add(reserveringen);
                    }
                }
            }
            return reservering;
        }

        /// <summary>
        /// maakt een incomplete reservering aan (een medewerker moet nog een tafel toewijzen)
        /// </summary>
        /// <param name="date">De datum van de reservering</param>
        /// <param name="klantnummers">De klantnummer(s) van de klant(en)</param>
        /// <param name="aantalMensen">Het aantal mensen waarvoor gereserveerd word</param>
        /// <param name="raamTafel">Of de klant een tafel bij het raam wilt</param>
        public void MakeCustomerReservation(DateTime date, List<int> klantnummers, int aantalMensen, bool raamTafel)
        {
            database = io.GetDatabase();
            if (database.reserveringen == null)
            {
                database.reserveringen = new List<Reserveringen>();
            }

            Reserveringen reservering = new Reserveringen
            {

                //zet het ID van de reservering naar +1 van het aantal dat al is gemaakt
                ID = database.reserveringen.Count + 1,

                //aantal mensen
                aantal = aantalMensen,

                //voeg alle klantnummers toe
                klantnummers = new List<int>(klantnummers),

                datum = date
            };

            //voeg de reservering toe aan de database
            database.reserveringen.Add(reservering);
            io.Savedatabase(database);
        }

        /// <summary>
        /// Verwijderd een reservering
        /// </summary>
        /// <param name="reserveringen">De reservering die verwijderd moet worden</param>
        public void RemoveReservations(Reserveringen reserveringen)
        {
            database = io.GetDatabase();
            database.reserveringen.Remove(reserveringen);
            io.Savedatabase(database);
        }
        #endregion

        #region Menukaart
        /// <summary>
        /// Voor het verkrijgen van de menukaart
        /// </summary>
        /// <returns>Een list met de menukaart</returns>
        public List<Gerechten> GetMenukaart()
        {
            database = io.GetDatabase();
            return database.menukaart.gerechten;
        }

        /// <summary>
        /// Filtert de menukaart op allergenen : een lijst met mogelijke allergenen zou makkelijk zijn hierzo ergens
        /// </summary>
        /// <param name="allergenen">Een list bestaande uit allergenen: een lijst met mogelijke allergenen zou makkelijk zijn hierzo ergens</param>
        /// <returns>De menukaart zonder gerechten die gemarkeerd zijn met de gegeven allergenen</returns>
        public List<Gerechten> GetMenukaart(List<string> allergenen)
        {
            //maakt nieuwe lijst met de menukaart
            List<Gerechten> menulist = new List<Gerechten>(GetMenukaart());

            //voor alles in menukaart, voor alle allergenen
            for (int i = 0; i < menulist.Count; i++)
            {
                for (int j = 0; j < allergenen.Count; j++)
                {
                    //als ingredient == allergeen zet die naar null
                    if (menulist[i].allergenen.Contains(allergenen[j]))
                    {
                        menulist[i] = null;
                    }
                }
            }
            menulist.RemoveAll(x => x == null);
            return menulist;
        }

        /// <summary>
        /// Filtert menukaart de op prijs
        /// </summary>
        /// <param name="toLower">if true, returned alles gelijk aan of lager dan de ingevoerde prijs. if false, returned alles met een hogere dan ingevoerde prijs</param>
        /// <param name="price">de waarde waarop gefilterd moet worden</param>
        /// <returns>De menukaart Gefilterd op prijs </returns>
        public List<Gerechten> GetMenukaart(double price, bool toLower)
        {
            //maakt nieuwe lijst met de menukaart
            List<Gerechten> menulist = new List<Gerechten>(GetMenukaart());

            //voor alles in menukaart, voor alle allergenen
            for (int i = 0; i < menulist.Count; i++)
            {
                if (toLower && menulist[i].prijs > price)
                {
                    menulist[i] = null;
                }
                else if (!toLower && menulist[i].prijs <= price)
                {
                    menulist[i] = null;
                }
            }
            menulist.RemoveAll(x => x == null);
            return menulist;
        }

        /// <summary>
        /// Filtert de menukaart op populair
        /// </summary>
        /// <returns>De menukaart gefilterd op populair</returns>
        public List<Gerechten> GetMenukaartPopulair()
        {
            //maakt nieuwe lijst met de menukaart
            List<Gerechten> menulist = new List<Gerechten>(GetMenukaart());

            //voor alles in menukaart
            for (int i = 0; i < menulist.Count; i++)
            {
                //als het NIET populair is zet het naar null
                if (!menulist[i].is_populair)
                {
                    menulist[i] = null;
                }
            }
            //verwijder alle nulls
            menulist.RemoveAll(x => x == null);
            return menulist;
        }

        /// <summary>
        /// Filtert de menukaart op speciaal
        /// </summary>
        /// <returns>De menukaart gefilterd op speciaal </returns>
        public List<Gerechten> GetMenukaartSpeciaal()
        {
            //maakt nieuwe lijst met de menukaart
            List<Gerechten> menulist = new List<Gerechten>(GetMenukaart());

            //voor alles in menukaart, voor alle allergenen
            for (int i = 0; i < menulist.Count; i++)
            {
                if (!menulist[i].special)
                {
                    menulist[i] = null;
                }
            }
            menulist.RemoveAll(x => x == null);
            return menulist;
        }
        #endregion

        #region Reviews
        /// <summary>
        /// Maakt een review aan
        /// </summary>
        /// <param name="rating">De Beoordeling</param>
        /// <param name="klant">De klant die de review maakt</param>
        /// <param name="message">Het bericht dat de klant erbij wilt zetten</param>
        /// <param name="reservering">De reservering waarop de review is gegeven</param>
        /// <param name="anoniem">if true, zal geen persoonlijke gegevens returnen bij GetReviews</param>
        public void MakeReview(int rating, Klantgegevens klant, string message, Reserveringen reservering, bool anoniem)
        {
            database = io.GetDatabase();
            if (database.reviews == null)
            {
                database.reviews = new List<Review>();
            }

            Review review = new Review
            {
                Rating = rating,
                Klantnummer = klant.klantnummer,
                message = message,
                reservering_ID = reservering.ID,
                annomeme = anoniem,
                datum = DateTime.Now
            };
            if (database.reviews.Count == 0)
            {
                review.ID = 0;
            }
            else
            {
                review.ID = database.reviews[database.reviews.Count - 1].ID + 1;
            }

            database.reviews.Add(review);
            io.Savedatabase(database);
        }

        /// <summary>
        /// Herschrijft een review
        /// </summary>
        /// <param name="reviewID">Het ID van de Review</param>
        /// <param name="rating">De beoordeling van</param>
        /// <param name="klant">De klant die de review geeft</param>
        /// <param name="message">Het bericht dat de klant wil achterlaten</param>
        /// <param name="anoniem">if true, zal geen persoonlijke gegevens returnen bij GetReviews</param>
        public void OverwriteReview(int reviewID, int rating, Klantgegevens klant, string message, bool anoniem)
        {
            //pakt de database
            database = io.GetDatabase();

            //voor alle reviews in de database
            for (int i = 0; i < database.reviews.Count; i++)
            {
                //als reviewID in de database gelijk staat aan gegeven reviewID en voor opgegeven klant, moet niet hebben dat mensen andere reviews kunnen aanpassen
                if (reviewID == database.reviews[i].ID && klant.klantnummer == database.reviews[i].Klantnummer)
                {
                    //maakt een nieuwe review op dezelfde locatie als de oude met informatie van de oude en de nieuwe
                    database.reviews[i] = new Review
                    {
                        Rating = rating,
                        annomeme = anoniem,
                        datum = database.reviews[i].datum,
                        ID = database.reviews[i].ID,
                        Klantnummer = database.reviews[i].Klantnummer,
                        message = message,
                        reservering_ID = database.reviews[i].reservering_ID,
                    };
                    //stop the count
                    break;
                }
            }
            //save de database
            io.Savedatabase(database);
        }

        /// <summary>
        /// Verwijderd een review
        /// </summary>
        /// <param name="reviewID">Het ID van de review</param>
        /// <param name="klant">De klant die de review heeft gegeven</param>
        public void DeleteReview(int reviewID, Klantgegevens klant)
        {
            //pakt de database
            database = io.GetDatabase();

            //voor alle reviews in de database
            for (int i = 0; i < database.reviews.Count; i++)
            {
                //als reviewID in de database gelijk staat aan gegeven reviewID en voor opgegeven klant, moet niet hebben dat mensen andere reviews kunnen aanpassen
                if (reviewID == database.reviews[i].ID && klant.klantnummer == database.reviews[i].Klantnummer)
                {
                    //delete de review
                    database.reviews.RemoveAt(i);

                    //stop the count
                    break;
                }
            }
            //save de database
            io.Savedatabase(database);
        }
        #endregion

        #region Feedback
        /// <summary>
        /// Maakt feedback aan
        /// </summary>
        /// <param name="werknemer">De werknemer waarvoor de feedback is</param>
        /// <param name="klant">De klant die de feedback geeft</param>
        /// <param name="message">Het bericht die de klant achterlaat</param>
        /// <param name="reservering">De reservering van de klant</param>
        /// <param name="anoniem">if true, zal geen persoonlijke gegevens returnen bij GetReviews</param>
        public void MakeFeedback(Werknemer werknemer,Klantgegevens klant, string message, Reserveringen reservering, bool anoniem)
        {
            //pakt database
            database = io.GetDatabase();
            
            //als feedback lijst nog niet bestaat maak die aan
            if (database.feedback == null)
            {
                database.feedback = new List<Feedback>();
            }

            //maakt feedback
            Feedback feedback = new Feedback
            {
                recipient = werknemer.ID,
                annomeme = anoniem,
                datum = DateTime.Now,
                Klantnummer = klant.klantnummer,
                message = message,
                reservering_ID = reservering.ID,
            };
            
            //als er geen feedback in de lijst is, zet ID naar 1, anders feebackID is laatste feedbackID+1
            if (database.feedback.Count == 0)
            {
                feedback.ID = 0;
            }
            else
            {
                feedback.ID = database.feedback[database.feedback.Count - 1].ID + 1;
            }
            //slaat de database op
            io.Savedatabase(database);
        }

        /// <summary>
        /// Overschrijft feedback met een nieuwe en vervangt de feedback op de plaats van gegeven ID
        /// </summary>
        /// <param name="feedbackID">Het ID van de feedback</param>
        /// <param name="klant">De klant die de feedback geeft</param>
        /// <param name="message">Het bericht dat de klant wilt achterlaten</param>
        /// <param name="anoniem">if true, zal geen persoonlijke gegevens returnen bij GetReviews</param>
        public void OverwriteFeedback(int feedbackID, Klantgegevens klant, string message, bool anoniem)
        {
            //pakt de database
            database = io.GetDatabase();

            //voor alle feedbacks in de database
            for (int i = 0; i < database.feedback.Count; i++)
            {
                //als feedbackID in de database gelijk staat aan gegeven feedbackID en voor opgegeven klant, moet niet hebben dat mensen andere feedback kunnen aanpassen
                if (feedbackID == database.feedback[i].ID && klant.klantnummer == database.feedback[i].Klantnummer)
                {
                    //maakt een nieuwe feeback op dezelfde locatie als de oude met informatie van de oude en de nieuwe
                    database.feedback[i] = new Feedback
                    {
                        recipient = database.feedback[i].recipient,
                        annomeme = anoniem,
                        datum = database.feedback[i].datum,
                        ID = database.feedback[i].ID,
                        Klantnummer = database.feedback[i].Klantnummer,
                        message = message,
                        reservering_ID = database.feedback[i].reservering_ID,
                    };
                    //stop the count
                    break;
                }
            }
            //save de database
            io.Savedatabase(database);
        }

        /// <summary>
        /// delete feedback voor gegeven feedbackID
        /// </summary>
        /// <param name="feedbackID">Het ID van de feedback</param>
        /// <param name="klant">De klant die de feedback heeft gemaakt</param>
        public void DeleteFeedback(int feedbackID, Klantgegevens klant)
        {
            //pakt de database
            database = io.GetDatabase();

            //voor alle feedback in de database
            for (int i = 0; i < database.feedback.Count; i++)
            {
                //als feedbackID in de database gelijk staat aan gegeven feedbackID en voor opgegeven klant, moet niet hebben dat mensen andere feedback kunnen aanpassen
                if (feedbackID == database.feedback[i].ID && klant.klantnummer == database.feedback[i].Klantnummer)
                {
                    //delete de review
                    database.feedback.RemoveAt(i);

                    //stop the count
                    break;
                }
            }
            //save de database
            io.Savedatabase(database);
        }
        #endregion

        #region Deprecated
        [Obsolete("getCustomerReservation is vervangen met GetCustomerReservation.")]
        public List<Reserveringen> getCustomerReservation(Klantgegevens klant)
        {
            database = io.GetDatabase();
            List<Reserveringen> reservering = new List<Reserveringen>();
            //voor elke reservering in de database, voor elk klantnummer van een reservering
            foreach (var reserveringen in database.reserveringen)
            {
                foreach (var klantnummer in reserveringen.klantnummers)
                {
                    //voegt alle reserveringen van de klant toe
                    if (klant.klantnummer == klantnummer)
                    {
                        reservering.Add(reserveringen);
                    }
                }
            }
            return reservering;
        }
        
        [Obsolete("makeReview is vervangen met MakeReview.")]
        public void makeReview(int rating, Klantgegevens klant, string message, Reserveringen reservering, bool anoniem)
        {
            database = io.GetDatabase();
            if (database.reviews == null)
            {
                database.reviews = new List<Review>();
            }

            Review review = new Review
            {
                Rating = rating,
                Klantnummer = klant.klantnummer,
                message = message,
                reservering_ID = reservering.ID,
                annomeme = anoniem,
                datum = DateTime.Now
            };
            if (database.reviews.Count == 0)
            {
                review.ID = 0;
            }
            else
            {
                review.ID = database.reviews[database.reviews.Count - 1].ID + 1;
            }

            database.reviews.Add(review);
            io.Savedatabase(database);
        }

        [Obsolete("getReviews is vervangen met GetReviews.")]
        public List<Review> getReviews()
        {
            database = io.GetDatabase();
            List<Review> reviewList = new List<Review>();
            if (database.reviews == null)
            {
                return reviewList;
            }
            else
            {
                //laat 50 reviews zien, als er zoveel zijn
                for (int i = 0, j = 0; i < database.reviews.Count && j < 50; i++, j++)
                {
                    if (database.reviews[i].annomeme)
                    {
                        Review temp = database.reviews[i];
                        temp.Klantnummer = 0;
                        temp.reservering_ID = 0;
                        reviewList.Add(temp);
                    }
                    else
                    {
                        reviewList.Add(database.reviews[i]);
                    }
                }
                return reviewList;
            }
        }

        [Obsolete("overwriteReview is vervangen met OverwriteReview.")]
        public void overwriteReview(int reviewID, int rating, Klantgegevens klant, string message, bool anoniem)
        {
            //pakt de database
            database = io.GetDatabase();

            //voor alle reviews in de database
            for (int i = 0; i < database.reviews.Count; i++)
            {
                //als reviewID in de database gelijk staat aan gegeven reviewID en voor opgegeven klant, moet niet hebben dat mensen andere reviews kunnen aanpassen
                if (reviewID == database.reviews[i].ID && klant.klantnummer == database.reviews[i].Klantnummer)
                {
                    //maakt een nieuwe review op dezelfde locatie als de oude met informatie van de oude en de nieuwe
                    database.reviews[i] = new Review
                    {
                        Rating = rating,
                        annomeme = anoniem,
                        datum = database.reviews[i].datum,
                        ID = database.reviews[i].ID,
                        Klantnummer = database.reviews[i].Klantnummer,
                        message = message,
                        reservering_ID = database.reviews[i].reservering_ID,
                    };
                    //stop the count
                    break;
                }
            }
            //save de database
            io.Savedatabase(database);
        }

        [Obsolete("deleteReview is vervangen met DeleteReview.")]
        public void deleteReview(int reviewID, Klantgegevens klant)
        {
            //pakt de database
            database = io.GetDatabase();

            //voor alle reviews in de database
            for (int i = 0; i < database.reviews.Count; i++)
            {
                //als reviewID in de database gelijk staat aan gegeven reviewID en voor opgegeven klant, moet niet hebben dat mensen andere reviews kunnen aanpassen
                if (reviewID == database.reviews[i].ID && klant.klantnummer == database.reviews[i].Klantnummer)
                {
                    //delete de review
                    database.reviews.RemoveAt(i);

                    //stop the count
                    break;
                }
            }
            //save de database
            io.Savedatabase(database);
        }

        #endregion
    }
}