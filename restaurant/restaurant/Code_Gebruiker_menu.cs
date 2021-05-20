using System;
using System.Collections.Generic;
using System.Linq;
//using System.Text;

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

            int num = 44532;
            MakeCustomerReservation(DateTime.Now, num, 6, false);
            num = 22367;
            MakeCustomerReservation(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day + 3), num, 2, true);
        }

        public Code_Gebruiker_menu()
        {
            
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
            //voor elke reservering in de database
            foreach (var reserveringen in database.reserveringen)
            {
                //als reservering overeenkomt met klantnummer, voegt alleen reserveringen in de toekomst toe
                if (toekomstReserveringen && klant.klantnummer == reserveringen.klantnummer && reserveringen.datum > DateTime.Now)
                {
                    reservering.Add(reserveringen);
                }
                //als reservering overeenkomt met klantnummer, voegt alle oude reservering toe van de klant
                else if (!toekomstReserveringen && klant.klantnummer == reserveringen.klantnummer && reserveringen.datum < DateTime.Now)
                {
                    reservering.Add(reserveringen);
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
                //voegt alle reserveringen van de klant toe
                if (klant.klantnummer == reserveringen.klantnummer)
                {
                    reservering.Add(reserveringen);
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
        public void MakeCustomerReservation(DateTime date, int klantnummer, int aantalMensen, bool raamTafel)
        {
            database = io.GetDatabase();
            //als er nog geen reserveringen list is in de database, maak die aan
            if (database.reserveringen == null)
            {
                database.reserveringen = new List<Reserveringen>();
            }

            Reserveringen reservering = new Reserveringen
            {
                aantal = aantalMensen,
                klantnummer = klantnummer,
                tafel_bij_raam = raamTafel,
                datum = date,
                tafels = new List<Tafels>(),
            };
            //voor de eerste moet het ID 0 zijn, daarna blijven optellen vanaf het laatste ID
            //heb geen database.reserveringen.Count+1 omdat als je er een verwijderd dan krijg je dubbele IDs
            if (database.reserveringen.Count == 0)
            {
                reservering.ID = 0;
            }
            else
            {
                reservering.ID = database.reserveringen[database.reserveringen.Count - 1].ID + 1;
            }

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

        //mist nog een adres dus nog niet compleet af
        /// <summary>
        /// maakt een reservering aan voor bezorging
        /// </summary>
        /// <param name="klant">De klantgegevens</param>
        /// <param name="bestelling">De list met alle gerechten</param>
        public void ReservationDelivery(Klantgegevens klant, List<Gerechten> bestelling)
        {
            database = io.GetDatabase();
            Reserveringen reservering = new Reserveringen()
            {
                datum = DateTime.Now,
                klantnummer = klant.klantnummer,
                isBezorging = true,
            };

            if (database.reserveringen.Count == 0)
            {
                reservering.ID = 0;
            }
            else
            {
                reservering.ID = database.reserveringen[database.reserveringen.Count - 1].ID + 1;
            }
            List<int> gerechtenID = new List<int>();
            for (int i = 0; i < bestelling.Count; i++)
            {
                gerechtenID.Add(bestelling[i].ID);
            }
            database.reserveringen.Add(reservering);
            io.Savedatabase(database);
        }

        /// <summary>
        /// maakt een reservering aan voor afhaal
        /// </summary>
        /// <param name="klant">De klantgegevens</param>
        /// <param name="bestelling">De List met alle gerechten die besteld zijn</param>
        public void ReservationTakeout(Klantgegevens klant, List<Gerechten> bestelling)
        {
            database = io.GetDatabase();
            Reserveringen reservering = new Reserveringen()
            {
                datum = DateTime.Now,
                klantnummer = klant.klantnummer,
            };

            if (database.reserveringen.Count == 0)
            {
                reservering.ID = 0;
            }
            else
            {
                reservering.ID = database.reserveringen[database.reserveringen.Count - 1].ID + 1;
            }
            List<int> gerechtenID = new List<int>();
            for (int i = 0; i < bestelling.Count; i++)
            {
                gerechtenID.Add(bestelling[i].ID);
            }
            database.reserveringen.Add(reservering);
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
            //als er nog geen menukaart is, maak een aan
            if (database.menukaart == null)
            {
                database.menukaart = new Menukaart();
                //ga ervan uit dat als ik een nieuwe menukaart maak dat er geen gerechten op staan dus maak ff lege list<Gerechten> om te returnen
                database.menukaart.gerechten = new List<Gerechten>();
            }
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
            //verwijder alle null
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

            //voor alles in menukaart
            for (int i = 0; i < menulist.Count; i++)
            {
                //if true en prijs is hoger, zet naar null
                if (toLower && menulist[i].prijs > price)
                {
                    menulist[i] = null;
                }
                //if false en prijs is lager zet naar null
                else if (!toLower && menulist[i].prijs <= price)
                {
                    menulist[i] = null;
                }
            }
            //verwijder alle null
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
        /// Maakt een non-anonieme review aan
        /// </summary>
        /// <param name="rating">De Beoordeling</param>
        /// <param name="klant">De klant die de review maakt</param>
        /// <param name="message">Het bericht dat de klant erbij wilt zetten</param>
        /// <param name="reservering">De reservering waarop de review is gegeven</param>
        public void MakeReview(int rating, Klantgegevens klant, string message, Reserveringen reservering)
        {
            database = io.GetDatabase();
            //als er geen reviews zijn maak een nieuwe list voor reviews
            if (database.reviews == null)
            {
                database.reviews = new List<Review>();
            }
            //maak een review met alle gegevens
            Review review = new Review
            {
                Rating = rating,
                Klantnummer = klant.klantnummer,
                message = message,
                reservering_ID = reservering.ID,
                annomeme = false,
                datum = DateTime.Now
            };
            //als er geen reviews zijn zet ID naar 0, else pak het laatste ID en doe die +1
            if (database.reviews.Count == 0)
            {
                review.ID = 0;
            }
            else
            {
                review.ID = database.reviews[database.reviews.Count - 1].ID + 1;
            }
            //add de review aan de database en sla die op
            database.reviews.Add(review);
            io.Savedatabase(database);
        }

        /// <summary>
        /// maakt een anonieme review aan
        /// </summary>
        /// <param name="rating">De beoordeling van de review</param>
        /// <param name="message">Het bericht bij de review</param>
        public void MakeReview(int rating, string message)
        {
            database = io.GetDatabase();
            //als er geen reviews zijn maak een nieuwe list voor reviews
            if (database.reviews == null)
            {
                database.reviews = new List<Review>();
            }
            //maak een review met alle gegevens
            Review review = new Review
            {
                Rating = rating,
                Klantnummer = -1,
                message = message,
                reservering_ID = -1,
                annomeme = true,
                datum = new DateTime()
            };
            //als er geen reviews zijn zet ID naar 0, else pak het laatste ID en doe die +1
            if (database.reviews.Count == 0)
            {
                review.ID = 0;
            }
            else
            {
                review.ID = database.reviews[database.reviews.Count - 1].ID + 1;
            }
            //add de review aan de database en sla die op
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
        public void OverwriteReview(int reviewID, int rating, Klantgegevens klant, string message)
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
                        annomeme = false,
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
        /// Om een review anoniem te maken
        /// </summary>
        /// <param name="reviewID">Het ID van de Review</param>
        /// <param name="rating">De beoordeling van de review</param>
        /// <param name="message">De inhoud van de review</param>
        public void OverwriteReview(int reviewID, int rating, string message)
        {
            //pakt de database
            database = io.GetDatabase();

            //voor alle reviews in de database
            for (int i = 0; i < database.reviews.Count; i++)
            {
                //als reviewID in de database gelijk staat aan gegeven reviewID en voor opgegeven klant, moet niet hebben dat mensen andere reviews kunnen aanpassen
                if (reviewID == database.reviews[i].ID)
                {
                    //maakt een nieuwe review op dezelfde locatie als de oude met informatie van de oude en de nieuwe
                    database.reviews[i] = new Review
                    {
                        Rating = rating,
                        annomeme = true,
                        datum = new DateTime(),
                        ID = database.reviews[i].ID,
                        Klantnummer = -1,
                        message = message,
                        reservering_ID = -1,
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
        /// Maakt non-anoniem feedback aan gericht aan de medewerker
        /// </summary>
        /// <param name="werknemer">De werknemer waarvoor de feedback is</param>
        /// <param name="klant">De klant die de feedback geeft</param>
        /// <param name="message">Het bericht die de klant achterlaat</param>
        /// <param name="reservering">De reservering van de klant</param>
        public void MakeFeedback(Werknemer werknemer, Klantgegevens klant, string message, int ReserveringID)
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
                annomeme = false,
                datum = DateTime.Now,
                Klantnummer = klant.klantnummer,
                message = message,
                reservering_ID = ReserveringID,
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
            database.feedback.Add(feedback);
            //slaat de database op
            io.Savedatabase(database);
        }

        /// <summary>
        /// Maakt non-anoniem feedback aan gericht aan de eigenaar
        /// </summary>
        /// <param name="eigenaar">De eigenaar</param>
        /// <param name="klant">De klant die de feedback geeft</param>
        /// <param name="message">Het bericht die de klant achterlaat</param>
        /// <param name="ReserveringID">De reservering van de klant</param>
        public void MakeFeedback(Eigenaar eigenaar, Klantgegevens klant, string message, int ReserveringID)
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
                recipient = eigenaar.ID,
                annomeme = false,
                datum = DateTime.Now,
                Klantnummer = klant.klantnummer,
                message = message,
                reservering_ID = ReserveringID,
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
            database.feedback.Add(feedback);
            //slaat de database op
            io.Savedatabase(database);
        }

        /// <summary>
        /// Maakt anoniem feedback aan gericht aan de eigenaar
        /// </summary>
        /// <param name="werknemer">De eigenaar</param>
        /// <param name="message">De feedback</param>
        public void MakeFeedback(Eigenaar eigenaar, string message)
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
                recipient = eigenaar.ID,
                annomeme = false,
                datum = DateTime.Now,
                Klantnummer = -1,
                message = message,
                reservering_ID = -1,
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
            database.feedback.Add(feedback);
            //slaat de database op
            io.Savedatabase(database);
        }

        /// <summary>
        /// Maakt anoniem feedback aan gericht aan een werknemer
        /// </summary>
        /// <param name="werknemer">De werknemer voor wie de feedback is</param>
        /// <param name="message">De inhoud van de feedback</param>
        public void MakeFeedback(Werknemer werknemer, string message)
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
                annomeme = false,
                datum = DateTime.Now,
                Klantnummer = -1,
                message = message,
                reservering_ID = -1,
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
            database.feedback.Add(feedback);
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
        public void OverwriteFeedback(int feedbackID, Klantgegevens klant, string message)
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
                        annomeme = false,
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
         /// Overschrijven van feedback naar anoniem
         /// </summary>
         /// <param name="feedbackID">Het ID van de feedback</param>
         /// <param name="message">De inhoud van de feedback</param>
        public void OverwriteFeedback(int feedbackID, string message)
        {
            //pakt de database
            database = io.GetDatabase();

            //voor alle feedbacks in de database
            for (int i = 0; i < database.feedback.Count; i++)
            {
                //als feedbackID in de database gelijk staat aan gegeven feedbackID en voor opgegeven klant, moet niet hebben dat mensen andere feedback kunnen aanpassen
                if (feedbackID == database.feedback[i].ID)
                {
                    //maakt een nieuwe feeback op dezelfde locatie als de oude met informatie van de oude en de nieuwe
                    database.feedback[i] = new Feedback
                    {
                        recipient = database.feedback[i].recipient,
                        annomeme = true,
                        datum = new DateTime(),
                        ID = database.feedback[i].ID,
                        Klantnummer = -1,
                        message = message,
                        reservering_ID = -1,
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

        #endregion
    }
    #region Screens
    class MakeFeedbackScreen : Screen
    {
        //het ID van dit scherm
        private readonly int huidigScherm = 5;
        //het ID van klanten menu
        private readonly int vorigScherm = 0;
        public MakeFeedbackScreen()
        {

        }
        public override int DoWork()
        {
            bool feedbackVoorEigenaar = false;
            Werknemer feedbackMedewerker = new Werknemer();

            #region Check1
            Database database = io.GetDatabase();
            //pak alle oude reserveringen
            List<Reserveringen> reserveringen = new List<Reserveringen>(code_gebruiker.GetCustomerReservation(ingelogd.klantgegevens, false));
            //lijst met alle reserveringen die al een review hebben
            List<Reserveringen> feedbackReservervations = new List<Reserveringen>();
            //lijst met alle feedback van een klant
            List<Feedback> feedback = new List<Feedback>(io.GetFeedback(ingelogd.klantgegevens));

            //slaat alle reserveringen die al een review hebben op en except deze uit de lijst zodat je alleen niet reviewde reserveringen overhoudt
            for (int i = 0; i < reserveringen.Count; i++)
            {
                for (int j = 0; j < feedback.Count; j++)
                {
                    if (reserveringen[i].ID == feedback[j].reservering_ID)
                    {
                        feedbackReservervations.Add(reserveringen[i]);
                        break;
                    }
                }
            }
            reserveringen = reserveringen.Except(feedbackReservervations).ToList();
            if (reserveringen.Count == 0)
            {
                Console.WriteLine(GetGFLogo());
                Console.WriteLine("Er staan momenteel geen Reserveringen open voor feedback.");
                Console.WriteLine("Druk op een toets om terug te gaan.");
                Console.ReadKey();
                return vorigScherm;
            }
            #endregion


            Console.WriteLine(GetGFLogo(4));

            //screen voor feedback maken
            Console.Clear();
            Console.WriteLine(GetGFLogo(4));
            Console.WriteLine("Hier kunt u feedback aanmaken.");
            Console.WriteLine("U kunt kiezen tussen of u anoniem feedback wilt geven.");
            Console.WriteLine("Anoniem houdt in:");
            Console.WriteLine("-> U naam word niet opgeslagen met feedback.");
            Console.WriteLine("-> feedback word niet gekoppeld aan een reservering.");
            Console.WriteLine("-> U kunt feedback niet aanpassen of verwijderen.");
            Console.WriteLine("[1] Normaal");
            Console.WriteLine("[2] Anoniem");
            Console.WriteLine("[3] Terug");
            (string, int)key = AskForInput(huidigScherm);

            if (key.Item2 != -1)
            {
                return huidigScherm;
            }

            //reservering ID heb ik later nodig buiten de scope
            int ReserveringID = -1;

            //feedback normaal
            if (key.Item1 == "1")
            {
                //bool voor de do-while loop
                bool repeat = false;

                //voor het kiezen van een reservering en krijgen van ReserveringID
                do
                {
                    Console.Clear();
                    Console.WriteLine(GetGFLogo());

                    for (int i = 0; i < reserveringen.Count; i++)
                    {
                        Console.WriteLine(reserveringen[i].ID + " | " + reserveringen[i].aantal + " | " + reserveringen[i].datum);
                    }
                    Console.WriteLine("\"ID | aantal mensen | datum\"");
                    Console.WriteLine("Is het formaat van deze weergave.");
                    Console.WriteLine("Met het ID kunt u selecteren over welk bezoek u een review wilt schrijven.");
                    Console.WriteLine("Het ID van u reservering:");

                    key = AskForInput(huidigScherm);
                    if (key.Item2 != -1)
                    {
                        return huidigScherm;
                    }

                    //is om te kijken of choice een int is
                    try
                    {
                        //kijkt of de invoer in de lijst is, zo niet dan invalid input
                        if (!reserveringen.Select(i => i.ID).ToList().Contains(Convert.ToInt32(key.Item1)))
                        {
                            Console.WriteLine("\nU moet wel een reservering invoeren.");
                            Console.WriteLine("Druk op een toets om verder te gaan.");
                            Console.ReadKey();
                        }
                        else
                        {
                            //ID van reservering
                            ReserveringID = Convert.ToInt32(key.Item1);
                            //breek de loop
                            repeat = true;
                        }
                    }
                    //geen int ingevoerd
                    catch
                    {
                        Console.WriteLine("\nU moet wel een reservering invoeren.");
                        Console.WriteLine("Druk op een toets om verder te gaan.");
                        Console.ReadKey();
                    }
                } while (!repeat);

                //zolang feedback geen recipient heeft voer dit uit
                do
                {
                    Console.Clear();
                    Console.WriteLine(GetGFLogo());
                    //maak lijst met medewerkers en voeg alle medewerkers toe
                    List<Werknemer> Medewerkers = new List<Werknemer>(database.werknemers);



                    //als er geen medewerkers zijn, is feedback voor eigenaar
                    if (Medewerkers.Count == 0)
                    {
                        Console.WriteLine("Er zijn geen medewerkers");
                        Console.WriteLine("De feedback zal naar de eigenaar gaan.");
                        Console.WriteLine("Druk op een toets om verder te gaan");
                        feedbackVoorEigenaar = true;
                    }
                    //als er wel medewerkers zijn
                    else
                    {
                        for (int j = 0; j < Medewerkers.Count; j++)
                        {
                            Console.WriteLine("Naam:  " + Medewerkers[j].login_gegevens.klantgegevens.voornaam + " " + Medewerkers[j].login_gegevens.klantgegevens.achternaam);
                        }
                        Console.WriteLine("Voer de voornaam en achternaam in van de medewerker en druk op Enter.");
                        Console.WriteLine("Als u geen medewerker wilt invoeren, voer dan niks in en druk dan op Enter.");
                        Console.WriteLine("LET OP! het is hoofdletter gevoelig.");

                        //item 1 is de naam van de medewerker
                        (string, int) choiceMedewerker = AskForInput(huidigScherm);
                        //als escape, ga terug
                        if (choiceMedewerker.Item2 != -1)
                        {
                            return choiceMedewerker.Item2;
                        }
                        //als leeg, is voor eigenaar
                        if (choiceMedewerker.Item1 == "")
                        {
                            feedbackVoorEigenaar = true;
                        }
                        //als match met werknemer sla deze op
                        for (int i = 0; i < Medewerkers.Count; i++)
                        {
                            if (Medewerkers[i].login_gegevens.klantgegevens.voornaam + " " + Medewerkers[i].login_gegevens.klantgegevens.achternaam == choiceMedewerker.Item1)
                            {
                                feedbackMedewerker = Medewerkers[i];
                                break;
                            }
                        }
                    }
                    if (feedbackVoorEigenaar == false && feedbackMedewerker.login_gegevens == null)
                    {
                        Console.WriteLine("\nEr is geen medewerker met deze naam.");
                        Console.WriteLine("druk op een toets om opnieuw te proberen.");
                        Console.ReadKey();
                    }
                } while (!((feedbackVoorEigenaar != false && feedbackMedewerker.login_gegevens == null) || (feedbackVoorEigenaar == false && feedbackMedewerker.login_gegevens != null)));

                Console.Clear();
                Console.WriteLine(GetGFLogo());

                Console.WriteLine("Als laatste, de inhoud van u feedback.");
                Console.WriteLine("Met Enter gaat u verder op een nieuwe regel.");
                Console.WriteLine("Als u klaar bent met het typen, type dan \"klaar\" op een nieuwe regel om het feedback aan te maken.");
                Console.WriteLine("De inhoud van u feedback:");

                string message = "";
                string Line = Console.ReadLine();
                message = Line;
                while (Line != "klaar")
                {
                    Line = Console.ReadLine();
                    if (Line == "klaar")
                    {
                        break;
                    }
                    else
                    {
                        message += " " + Line;
                    }
                }
                if (feedbackVoorEigenaar)
                {
                    code_gebruiker.MakeFeedback(database.eigenaar, ingelogd.klantgegevens, message, ReserveringID);
                    Console.WriteLine("Succesvol feedback gemaakt.");
                    Console.WriteLine("druk op een toets om terug te gaan.");
                    Console.ReadKey();
                    return vorigScherm;
                }
                else
                {
                    code_gebruiker.MakeFeedback(feedbackMedewerker, ingelogd.klantgegevens, message, ReserveringID);
                    Console.WriteLine("Succesvol feedback gemaakt.");
                    Console.WriteLine("druk op een toets om terug te gaan.");
                    Console.ReadKey();
                    return vorigScherm;
                }
            }

            //feedback anoniem
            else if (key.Item1 == "2")
            {
                //zolang feedback geen recipient heeft voer dit uit
                do
                {
                    Console.Clear();
                    Console.WriteLine(GetGFLogo());
                    //maak lijst met medewerkers en voeg alle medewerkers toe
                    List<Werknemer> Medewerkers = new List<Werknemer>(database.werknemers);



                    //als er geen medewerkers zijn, is feedback voor eigenaar
                    if (Medewerkers.Count == 0)
                    {
                        Console.WriteLine("\nEr zijn geen medewerkers");
                        Console.WriteLine("De feedback zal naar de eigenaar gaan.");
                        Console.WriteLine("Druk op een toets om verder te gaan");
                        feedbackVoorEigenaar = true;
                    }
                    //als er wel medewerkers zijn
                    else
                    {
                        for (int j = 0; j < Medewerkers.Count; j++)
                        {
                            Console.WriteLine("Naam:  " + Medewerkers[j].login_gegevens.klantgegevens.voornaam + " " + Medewerkers[j].login_gegevens.klantgegevens.achternaam);
                        }
                        Console.WriteLine("Voer de voornaam en achternaam in van de medewerker en druk op Enter.");
                        Console.WriteLine("Als u geen medewerker wilt invoeren, voer dan niks in en druk dan op Enter.");
                        Console.WriteLine("LET OP! het is hoofdletter gevoelig.");

                        //item 1 is de naam van de medewerker
                        (string, int) choiceMedewerker = AskForInput(huidigScherm);
                        //als escape, ga terug
                        if (choiceMedewerker.Item2 != -1)
                        {
                            return choiceMedewerker.Item2;
                        }
                        //als leeg, is voor eigenaar
                        if (choiceMedewerker.Item1 == "")
                        {
                            feedbackVoorEigenaar = true;
                        }
                        //als match met werknemer sla deze op
                        for (int i = 0; i < Medewerkers.Count; i++)
                        {
                            if (Medewerkers[i].login_gegevens.klantgegevens.voornaam + " " + Medewerkers[i].login_gegevens.klantgegevens.achternaam == choiceMedewerker.Item1)
                            {
                                feedbackMedewerker = Medewerkers[i];
                                break;
                            }
                        }
                    }
                    if (feedbackVoorEigenaar == false && feedbackMedewerker.login_gegevens == null)
                    {
                        Console.WriteLine("\nEr is geen medewerker met deze naam.");
                        Console.WriteLine("druk op een toets om opnieuw te proberen.");
                        Console.ReadKey();
                    }
                } while (!((feedbackVoorEigenaar != false && feedbackMedewerker.login_gegevens == null) || (feedbackVoorEigenaar == false && feedbackMedewerker.login_gegevens != null)));

                Console.Clear();
                Console.WriteLine(GetGFLogo());

                Console.WriteLine("Als laatste, de inhoud van u feedback.");
                Console.WriteLine("Met Enter gaat u verder op een nieuwe regel.");
                Console.WriteLine("Als u klaar bent met het typen, type dan \"klaar\" op een nieuwe regel om het feedback aan te maken.");
                Console.WriteLine("De inhoud van u feedback:");

                string message = "";
                string Line = Console.ReadLine();
                message = Line;
                while (Line != "klaar")
                {
                    Line = Console.ReadLine();
                    if (Line == "klaar")
                    {
                        break;
                    }
                    else
                    {
                        message += " " + Line;
                    }
                }
                if (feedbackVoorEigenaar)
                {
                    code_gebruiker.MakeFeedback(database.eigenaar, message);
                    Console.WriteLine("\nSuccesvol feedback gemaakt.");
                    Console.WriteLine("druk op een toets om terug te gaan.");
                    Console.ReadKey();
                    return vorigScherm;
                }
                else
                {
                    code_gebruiker.MakeFeedback(feedbackMedewerker, message);
                    Console.WriteLine("\nSuccesvol feedback gemaakt.");
                    Console.WriteLine("druk op een toets om terug te gaan.");
                    Console.ReadKey();
                    return vorigScherm;
                }
            }

            //terug
            else if (key.Item1 == "3")
            {
                //een scherm terug
                return huidigScherm;
            }

            else if (key.Item1 == "4")
            {
                Console.WriteLine("\nSuccesvol uitgelogd");
                Console.WriteLine("Druk op een toets om terug te gaan.");
                Console.ReadKey();
                //logout sequence
                logoutUpdate = true;
                Logout();
                return 0;
            }

            else
            {
                //error message
                Console.WriteLine("\nU moet wel een juiste keuze maken...");
                Console.WriteLine("Druk op een knop om opniew te kiezen.");
                Console.ReadKey();
                return huidigScherm;
            }
        }
            

        public override List<Screen> Update(List<Screen> screens)
        {
            DoLogoutOnEveryScreen(screens);
            return screens;
        }
    }
    #endregion
}