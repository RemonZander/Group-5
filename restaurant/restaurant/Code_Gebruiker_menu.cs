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
            List<Gerechten> test = Getmenukaart(new List<string> { "lactose intolerantie" });

            List<int> num = new List<int> { 223541, 220793 };
            MakeCustomerReservation(DateTime.Now, num, 6, false);
            num = new List<int> { 223167 };
            MakeCustomerReservation(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day + 3), num, 2, true);
        }

        public Code_Gebruiker_menu()
        {
            database = io.Getdatabase();
        }

        #region Reserveringen
        //pakt de reservering van een klant
        public Reserveringen Get_reservation(Klantgegevens klant)
        {
            Reserveringen reservering = new Reserveringen();
            //voor elke reservering in de database, voor elk klantnummer
            foreach (var reserveringen in database.reserveringen)
            {
                foreach (var klantnummer in reserveringen.klantnummers)
                {
                    //als de klant is gevonden en datum is in de toekomst, voeg de reservering toe
                    if (klant.klantnummer == klantnummer && reserveringen.datum > DateTime.Now)
                    {
                        reservering = reserveringen;
                    }
                }
            }
            return reservering;
        }

        //maakt een incomplete reservering aan (een medewerker moet nog een tafel toewijzen) en voegt deze toe aan de database
        public void MakeCustomerReservation(DateTime date, List<int> klantnummers, int aantalMensen, bool raamTafel)
        {
            database = io.Getdatabase();
            if (database.reserveringen == null)
            {
                database.reserveringen = new List<Reserveringen>();
            }

            Reserveringen reservering = new Reserveringen
            {

                //zet het ID van de reservering naar +1 van het aantal dat al is gemaakt
                ID = database.reserveringen.Count + 1,

                //aatnal mensen
                aantal = aantalMensen,

                //voeg alle klantnummers toe
                klantnummers = new List<int>(klantnummers),

                datum = date
            };

            //voeg de reservering toe aan de database
            database.reserveringen.Add(reservering);
            io.Savedatabase(database);
        }

        //reset de database
        public void Remove_reservations(Reserveringen reserveringen)
        {
            database = io.Getdatabase();
            database.reserveringen.Remove(reserveringen);
            io.Savedatabase(database);
        }
        #endregion

        #region Menukaart
        //pakt menukaart uit de database
        public List<Gerechten> Getmenukaart()
        {
            return database.menukaart.gerechten;
        }

        //verwijderd allergenen uit de menukaart en returned een gefilterde menukaart
        public List<Gerechten> Getmenukaart(List<string> allergenen)
        {
            //maakt nieuwe lijst met de menukaart
            List<Gerechten> menulist = new List<Gerechten>(Getmenukaart());

            //voor alles in menukaart, voor alle allergenen
            for (int i = 0; i < database.menukaart.gerechten.Count; i++)
            {
                for (int j = 0; j < allergenen.Count; j++)
                {
                    //als ingredient == allergeen zet die naar null
                    if (database.menukaart.gerechten[i].allergenen.Contains(allergenen[j]))
                    {
                        menulist[i] = null;
                    }
                }
            }
            menulist.RemoveAll(x => x == null);
            return menulist;
        }

        /// <summary>
        /// filtered menukaart prijs
        /// </summary>
        /// <param name="toLower">if true, returned alles gelijk aan of lager dan de ingevoerde prijs. if false, returned alles met een hogere dan ingevoerde prijs</param>
        public List<Gerechten> Getmenukaart(int price, bool toLower)
        {
            //maakt nieuwe lijst met de menukaart
            List<Gerechten> menulist = new List<Gerechten>(Getmenukaart());

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

        public List<Gerechten> GetmenukaartPopulair()
        {
            //maakt nieuwe lijst met de menukaart
            List<Gerechten> menulist = new List<Gerechten>(Getmenukaart());

            //voor alles in menukaart, voor alle allergenen
            for (int i = 0; i < menulist.Count; i++)
            {
                if (!menulist[i].is_populair)
                {
                    menulist[i] = null;
                }
            }
            menulist.RemoveAll(x => x == null);
            return menulist;
        }

        public List<Gerechten> GetmenukaartSpeciaal()
        {
            //maakt nieuwe lijst met de menukaart
            List<Gerechten> menulist = new List<Gerechten>(Getmenukaart());

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
        //maakt een review
        public void makeReview(int rating, Klantgegevens klant, string message, Reserveringen reservering, bool anoniem)
        {
            database = io.Getdatabase();
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
                anomiem = anoniem,
                datum = DateTime.Now
            };
            if (database.reviews.Count == 0)
            {
                review.ID = 1;
            }
            else
            {
                review.ID = database.reviews[database.reviews.Count - 1].ID + 1;
            }

            database.reviews.Add(review);
            io.Savedatabase(database);
        }

        //returned max 50 reviews, als de review anoniem is verandert de naam en reservering nummer naar 0
        public List<Review> getReviews()
        {
            io.Getdatabase();
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
                    if (database.reviews[i].anomiem)
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

        //overwrite een review met 
        public void overwriteReview(int reviewID, int rating, Klantgegevens klant, string message, bool anoniem)
        {
            //pakt de database
            database = io.Getdatabase();

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
                        anomiem = anoniem,
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

        //delete een review
        public void deleteReview(int reviewID, Klantgegevens klant)
        {
            //pakt de database
            database = io.Getdatabase();

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
        //maakt feedback aan
        public void makeFeedback(Werknemer werknemer,Klantgegevens klant, string message, Reserveringen reservering, bool anoniem)
        {
            //pakt database
            database = io.Getdatabase();
            
            //als feedback lijst nog niet bestaat maak die aan
            if (database.feedback == null)
            {
                database.feedback = new List<Feedback>();
            }

            //maakt feedback
            Feedback feedback = new Feedback
            {
                //recipient = 
                anomiem = anoniem,
                datum = DateTime.Now,
                Klantnummer = klant.klantnummer,
                message = message,
                reservering_ID = reservering.ID,
            };
            
            //als er geen feedback in de lijst is, zet ID naar 1, anders feebackID is laatste feedbackID+1
            if (database.feedback.Count == 0)
            {
                feedback.ID = 1;
            }
            else
            {
                feedback.ID = database.feedback[database.feedback.Count - 1].ID + 1;
            }
            //slaat de database op
            io.Savedatabase(database);
        }
        
        //returned 50 items, als er zoveel zijn
        public List<Feedback> getFeedback()
        {
            //pakt de database
            io.Getdatabase();
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
                for (int i = 0, j = 0; i < database.feedback.Count && j < 50; i++, j++)
                {
                    if (database.feedback[i].anomiem)
                    {
                        Feedback temp = database.feedback[i];
                        temp.Klantnummer = 0;
                        temp.reservering_ID = 0;
                        feedbackList.Add(temp);
                    }
                    else
                    {
                        feedbackList.Add(database.feedback[i]);
                    }
                }
                return feedbackList;
            }
        }

        //herschrijft feedback voor gegeven feedbackID
        public void overwriteFeedback(int feedbackID, Klantgegevens klant, string message, bool anoniem)
        {
            //pakt de database
            database = io.Getdatabase();

            //voor alle feedbacks in de database
            for (int i = 0; i < database.feedback.Count; i++)
            {
                //als feedbackID in de database gelijk staat aan gegeven feedbackID en voor opgegeven klant, moet niet hebben dat mensen andere feedback kunnen aanpassen
                if (feedbackID == database.feedback[i].ID && klant.klantnummer == database.feedback[i].Klantnummer)
                {
                    //maakt een nieuwe feeback op dezelfde locatie als de oude met informatie van de oude en de nieuwe
                    database.feedback[i] = new Feedback
                    {
                        //recipient = 
                        anomiem = anoniem,
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

        //delete feedback voor gegeven feedbackID
        public void deleteFeedback(int feedbackID, Klantgegevens klant)
        {
            //pakt de database
            database = io.Getdatabase();

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
    }
}