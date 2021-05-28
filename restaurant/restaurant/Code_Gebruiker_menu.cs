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

        #region Account
        public void DeleteAccount(Klantgegevens klant)
        {
            Database database = io.GetDatabase();
            if (database.login_gegevens != null)
            {
                for (int i = 0; i < database.login_gegevens.Count; i++)
                {
                    if (database.login_gegevens[i].klantgegevens != null && klant.klantnummer == database.login_gegevens[i].klantgegevens.klantnummer)
                    {
                        //database.login_gegevens[i].klantgegevens = new Klantgegevens();
                        database.login_gegevens.RemoveAt(i);
                    }
                }
            }
        }
        #endregion

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
        public void DeleteReservations(Reserveringen reservering)
        {
            Database database = io.GetDatabase();
            for (int i = 0; i < database.reserveringen.Count; i++)
            {
                if (reservering.ID == database.reserveringen[i].ID)
                {
                    database.reserveringen.RemoveAt(i);
                    break;
                }
            }
            io.Savedatabase(database);
        }

        /// <summary>
        /// Voor het herschrijven van een klanten reservering
        /// </summary>
        /// <param name="reservering">De reservering die je gaat overschrijven</param>
        /// <param name="aantalMensen">het aantal mensen</param>
        /// <param name="datum">de datum waarop je de reservering wilt hebben</param>
        /// <param name="tafelBijRaam">of je een tafel bij het raam wilt</param>
        public void OverwriteReservation(Reserveringen reservering, int aantalMensen, DateTime datum, bool tafelBijRaam)
        {
            Database database = io.GetDatabase();
            if (database.reserveringen != null)
            {
                for (int i = 0; i < database.reserveringen.Count; i++)
                {
                    if (database.reserveringen[i].ID == reservering.ID)
                    {
                        Reserveringen newReservering = new Reserveringen
                        {
                            ID = reservering.ID,
                            aantal = aantalMensen,
                            datum = datum,
                            klantnummer = reservering.klantnummer,
                            tafel_bij_raam = tafelBijRaam,
                        };
                        database.reserveringen[i] = newReservering;
                        io.Savedatabase(database);
                    }
                }
            }
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
                klantnummer = klant.klantnummer,
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
                klantnummer = -1,
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
                if (reviewID == database.reviews[i].ID && klant.klantnummer == database.reviews[i].klantnummer)
                {
                    //maakt een nieuwe review op dezelfde locatie als de oude met informatie van de oude en de nieuwe
                    database.reviews[i] = new Review
                    {
                        Rating = rating,
                        annomeme = false,
                        datum = database.reviews[i].datum,
                        ID = database.reviews[i].ID,
                        klantnummer = database.reviews[i].klantnummer,
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
                        klantnummer = -1,
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
                if (reviewID == database.reviews[i].ID && klant.klantnummer == database.reviews[i].klantnummer)
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
                klantnummer = klant.klantnummer,
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
                klantnummer = klant.klantnummer,
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
                klantnummer = -1,
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
                klantnummer = -1,
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
                if (feedbackID == database.feedback[i].ID && klant.klantnummer == database.feedback[i].klantnummer)
                {
                    //maakt een nieuwe feeback op dezelfde locatie als de oude met informatie van de oude en de nieuwe
                    database.feedback[i] = new Feedback
                    {
                        recipient = database.feedback[i].recipient,
                        annomeme = false,
                        datum = database.feedback[i].datum,
                        ID = database.feedback[i].ID,
                        klantnummer = database.feedback[i].klantnummer,
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
                        klantnummer = -1,
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
                if (feedbackID == database.feedback[i].ID && klant.klantnummer == database.feedback[i].klantnummer)
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
                Console.WriteLine(GFLogo);
                Console.WriteLine("Er staan momenteel geen Reserveringen open voor feedback.");
                Console.WriteLine("Druk op een toets om terug te gaan.");
                Console.ReadKey();
                return vorigScherm;
            }
            #endregion

            //screen voor feedback maken
            Console.Clear();
            Console.WriteLine(GetGFLogo(true));
            Console.WriteLine("Hier kunt u feedback aanmaken.");
            Console.WriteLine("U kunt kiezen of u anoniem feedback wilt geven.");
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
                    Console.WriteLine(GetGFLogo(true));

                    for (int i = 0; i < reserveringen.Count; i++)
                    {
                        Console.WriteLine(reserveringen[i].ID + new string(' ', 8 - reserveringen[i].ID.ToString().Length) + "| " + reserveringen[i].aantal + new string(' ', 5 - reserveringen[i].aantal.ToString().Length) + "| " + reserveringen[i].datum);
                    }
                    Console.WriteLine("Hier ziet u alle reserveringen waarover u nog feedback kunt geven.");
                    Console.WriteLine("het formaat van deze weergave is:  ID | aantal mensen | datum");
                    Console.WriteLine("Met het ID kunt u selecteren over welk bezoek u een review wilt schrijven.");
                    Console.WriteLine("Het ID van u reservering:");

                    key = AskForInput(huidigScherm);
                    if (key.Item2 != -1)
                    {
                        return huidigScherm;
                    }

                    //als input is 0, logout
                    if (key.Item1 == "0")
                    {
                        Console.WriteLine("\nSuccesvol uitgelogd");
                        Console.WriteLine("Druk op een toets om terug te gaan.");
                        Console.ReadKey();
                        //logout sequence
                        logoutUpdate = true;
                        Logout();
                        return 0;
                    }

                    //is om te kijken of choice een int is
                    try
                    {
                        //kijkt of de invoer in de lijst is, zo niet dan invalid input
                        if (!reserveringen.Select(i => i.ID).ToList().Contains(Convert.ToInt32(key.Item1)))
                        {
                            Console.WriteLine("\nU moet wel geldige Reservering invoeren.");
                            Console.WriteLine("Druk op een toets om opnieuw te kiezen.");
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
                    Console.WriteLine(GetGFLogo(true));
                    //maak lijst met medewerkers en voeg alle medewerkers toe
                    List<Werknemer> Medewerkers = new List<Werknemer>(io.GetEmployee());



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
                        if(choiceMedewerker.Item1 == "0")
                        {
                            Console.WriteLine("\nSuccesvol uitgelogd");
                            Console.WriteLine("Druk op een toets om terug te gaan.");
                            Console.ReadKey();
                            //logout sequence
                            logoutUpdate = true;
                            Logout();
                            return 0;
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
                Console.WriteLine(GFLogo);

                if (feedbackVoorEigenaar)
                {
                    Eigenaar eigenaar = io.GetEigenaar();
                    Console.WriteLine($"De feedback die u gaat maken is voor  {eigenaar.login_gegevens.klantgegevens.voornaam} {eigenaar.login_gegevens.klantgegevens.achternaam}");
                }
                else
                {
                    Console.WriteLine("De feedback die u gaat maken is voor  " + feedbackMedewerker.login_gegevens.klantgegevens.voornaam + " " + feedbackMedewerker.login_gegevens.klantgegevens.achternaam);
                }
                Console.WriteLine("De inhoud van uw feedback.");
                Console.WriteLine("Met Enter gaat u verder op een nieuwe regel.");
                Console.WriteLine("Als u klaar bent met het typen, type dan  klaar  op een nieuwe regel om het feedback aan te maken.");
                Console.WriteLine("De inhoud van uw feedback:\n");

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
                    code_gebruiker.MakeFeedback(io.GetEigenaar(), ingelogd.klantgegevens, message, ReserveringID);
                    Console.WriteLine("Succesvol feedback gemaakt.");
                    Console.WriteLine("druk op een toets om terug te gaan.");
                    Console.ReadKey();
                    return huidigScherm;
                }
                else
                {
                    code_gebruiker.MakeFeedback(feedbackMedewerker, ingelogd.klantgegevens, message, ReserveringID);
                    Console.WriteLine("Succesvol feedback gemaakt.");
                    Console.WriteLine("druk op een toets om terug te gaan.");
                    Console.ReadKey();
                    return huidigScherm;
                }
            }

            //feedback anoniem
            else if (key.Item1 == "2")
            {
                //zolang feedback geen recipient heeft voer dit uit
                do
                {
                    Console.Clear();
                    Console.WriteLine(GetGFLogo(true));
                    //maak lijst met medewerkers en voeg alle medewerkers toe
                    List<Werknemer> Medewerkers = new List<Werknemer>(io.GetEmployee());



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
                        if (choiceMedewerker.Item1 == "0")
                        {
                            Console.WriteLine("\nSuccesvol uitgelogd");
                            Console.WriteLine("Druk op een toets om terug te gaan.");
                            Console.ReadKey();
                            //logout sequence
                            logoutUpdate = true;
                            Logout();
                            return 0;
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
                Console.WriteLine(GFLogo);
                
                if (feedbackVoorEigenaar)
                {
                    Eigenaar eigenaar = io.GetEigenaar();
                    Console.WriteLine($"De feedback die u gaat maken is voor  {eigenaar.login_gegevens.klantgegevens.voornaam} {eigenaar.login_gegevens.klantgegevens.achternaam}");
                }
                else
                {
                    Console.WriteLine("De feedback die u gaat maken is voor  " + feedbackMedewerker.login_gegevens.klantgegevens.voornaam + " " + feedbackMedewerker.login_gegevens.klantgegevens.achternaam);
                }
                Console.WriteLine("De inhoud van uw feedback.");
                Console.WriteLine("Met Enter gaat u verder op een nieuwe regel.");
                Console.WriteLine("Als u klaar bent met het typen, type dan  klaar  op een nieuwe regel om het feedback aan te maken.");
                Console.WriteLine("De inhoud van uw feedback:\n");

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
                    code_gebruiker.MakeFeedback(io.GetEigenaar(), message);
                    Console.WriteLine("\nSuccesvol feedback gemaakt.");
                    Console.WriteLine("druk op een toets om terug te gaan.");
                    Console.ReadKey();
                    return huidigScherm;
                }
                else
                {
                    code_gebruiker.MakeFeedback(feedbackMedewerker, message);
                    Console.WriteLine("\nSuccesvol feedback gemaakt.");
                    Console.WriteLine("druk op een toets om terug te gaan.");
                    Console.ReadKey();
                    return huidigScherm;
                }
            }

            //terug
            else if (key.Item1 == "3")
            {
                //een scherm terug
                return huidigScherm;
            }
            //logout
            else if (key.Item1 == "0")
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

    class ViewFeedbackScreen : Screen
    {
        public ViewFeedbackScreen()
        {

        }
        public override int DoWork()
        {
            List<Feedback> feedback = new List<Feedback>();
            feedback = io.GetFeedback(ingelogd.klantgegevens).OrderBy(s => s.datum).ToList();
            if (feedback.Count == 0)
            {
                Console.Clear();
                Console.WriteLine(GFLogo);
                Console.WriteLine("U heeft nog geen feedback");
                Console.WriteLine("druk op een knop om terug te gaan");
                Console.ReadKey();
                return 5;
            }

            Console.WriteLine(GetGFLogo(true));
            Console.WriteLine("Hier kunt u uw eigen feedback zien en bewerken:");
            Console.WriteLine("[1] Laat al uw feedback zien");
            Console.WriteLine("[2] Laat al uw feedback zien vanaf een datum (genoteerd als 1-1-2000)");
            //Console.WriteLine("[3] Laat al uw feedback zien op beoordeling, tussen de 1 en de 5");
            Console.WriteLine("[3] Ga terug naar klant menu scherm");

            //als escape, ga terug naar scherm 5
            (string, int) input = AskForInput(5);
            if (input.Item2 != -1)
            {
                return input.Item2;
            }
            if (input.Item1 == "1")
            {
                int page = 0;
                double pos = 0;
                List<string> pages = new List<string>();
                do
                {
                    pages = new List<string>();
                    List<List<string>> feedbackstring = Makedubbelboxes(FeedbackToString(feedback));
                    List<string> boxes = new List<string>();
                    for (int a = 0; a < feedbackstring.Count; a++)
                    {
                        if (a == feedbackstring.Count - 1 && feedbackstring[a][1].Length < 70)
                        {
                            if (a == Convert.ToInt32(Math.Floor(pos / 2)))
                            {
                                //for making the boxes around the text together with adding the bottom text
                                if (a != 0 && a % 6 != 0)
                                {
                                    boxes.Add(BoxAroundText(feedbackstring[a], "#", 2, 0, 104, true, new List<string>{
                                    "[3] Bewerken" + new string(' ', 50 - "[3] Bewerken".Length),
                                    "[4] Verwijderen" + new string(' ', 50 - "[4] Verwijderen".Length),
                                    new string(' ', 50)}));
                                }
                                else
                                {
                                    boxes.Add(BoxAroundText(feedbackstring[a], "#", 2, 0, 50, true, new List<string>{
                                    "[3] Bewerken" + new string(' ', 50 - "[3] Bewerken".Length),
                                    "[4] Verwijderen" + new string(' ', 50 - "[4] Verwijderen".Length),
                                    new string(' ', 50)}));
                                }
                            }
                            else
                            {
                                if (a != 0 && a % 6 != 0)
                                {
                                    boxes.Add(BoxAroundText(feedbackstring[a], "#", 2, 0, 104, true));
                                }
                                else
                                {
                                    boxes.Add(BoxAroundText(feedbackstring[a], "#", 2, 0, 50, true));
                                }

                            }
                        }
                        else
                        {
                            if (a == Convert.ToInt32(Math.Floor(pos / 2)))
                            {
                                if (pos % 2 == 0 || pos == 0)
                                {
                                    boxes.Add(BoxAroundText(feedbackstring[a], "#", 2, 0, 104, true, new List<string>{
                                    "[3] Bewerken" + new string(' ', 50 - "[3] Bewerken".Length) + "##  " + new string(' ', 50),
                                    "[4] Verwijderen" + new string(' ', 50 - "[4] Verwijderen".Length) + "##  " + new string(' ', 50),
                                    new string(' ', 50) + "##  " + new string(' ', 50) }));
                                }
                                else
                                {
                                    boxes.Add(BoxAroundText(feedbackstring[a], "#", 2, 0, 104, true, new List<string> {
                                    new string(' ', 50) + "##  " + "[3] Bewerken" + new string(' ', 50 - "[3] Bewerken".Length),
                                    new string(' ', 50) + "##  " + "[4] Verwijderen" + new string(' ', 50 - "[4] Verwijderen".Length),
                                    new string(' ', 50) + "##  " + new string(' ', 50)}));
                                }
                            }
                            else
                            {
                                boxes.Add(BoxAroundText(feedbackstring[a], "#", 2, 0, 104, true));
                            }
                        }
                    }

                    pages = MakePages(boxes, 3);
                    Console.Clear();
                    Console.WriteLine(GetGFLogo(true));
                    Console.WriteLine($"Dit is uw feedback op pagina {page + 1} van de {pages.Count}:");
                    Console.WriteLine("De Feedback die is geselecteerd is de feedback met de tekst \"Bewerken\" en \"verwijderen\"");
                    Console.WriteLine("U kunt met de pijltestoetsen navigeren door uw feedback,");
                    Console.WriteLine("De tekst \"Bewerken\" en \"verwijderen\" zal dan van positie veranderen in de richting van welke toets u indrukte");

                    //schrijft de pagina samen met de bottom symbols
                    if (feedbackstring[feedbackstring.Count - 1][1].Length < 70 && page == pages.Count - 1)
                    {
                        Console.WriteLine(pages[page] + new string('#', 56));
                    }
                    else
                    {
                        Console.WriteLine(pages[page] + new string('#', 110));
                    }

                    //var result = Nextpage(page, pages.Count - 1, pos, boxes.Count * 2 - 1, 8);
                    (int, int, double) result = (0, 0, 0);
                    if (page < pages.Count - 1)
                    {
                        result = Nextpage(page, pos, boxes.Count * 2 - 1, 8,
                            new List<Tuple<(int, int, double), string>> { Tuple.Create((page + 1, -1, (page + 1) * 6.0), "D1"), Tuple.Create((page, 8, pos), "D2"), Tuple.Create((-1, -1, pos), "D3"), Tuple.Create((-2, -2, pos), "D4") },
                            new List<string> { "[1] Volgende pagina", "[2] Terug" });
                    }
                    else
                    {
                        result = Nextpage(page, pos, boxes.Count*2-1, 8,
                            new List<Tuple<(int, int, double), string>> { Tuple.Create((page, 8, pos), "D1"), Tuple.Create((-1, -1, pos), "D3"), Tuple.Create((-2, -2, pos), "D4") },
                            new List<string> { "[1] Terug" });
                    }
                    pos = result.Item3;
                    if (result.Item2 != -1 && result.Item2 != -2)
                    {
                        return result.Item2;
                    }
                    else if (result.Item1 == -1 && result.Item2 == -1)
                    {
                        return EditFeedback(MakeFeedbackBox(feedback[Convert.ToInt32(pos)]), feedback[Convert.ToInt32(pos)]);
                    }
                    else if (result.Item1 == -2 && result.Item2 == -2)
                    {
                        Console.Clear();
                        Console.WriteLine(GetGFLogo(true));
                        Console.WriteLine(MakeFeedbackBox(feedback[Convert.ToInt32(pos)]) + "\n");
                        Console.WriteLine("Weet u zeker dat u deze feedback wilt verwijderen?");
                        Console.WriteLine("[1] Ja");
                        Console.WriteLine("[2] Nee");

                        input = AskForInput(5);
                        if (input.Item2 != -1)
                        {
                            return input.Item2;
                        }
                        else if (input.Item1 == "0")
                        {
                            Console.WriteLine("\nSuccesvol uitgelogd");
                            Console.WriteLine("Druk op een toets om terug te gaan.");
                            Console.ReadKey();
                            //logout sequence
                            logoutUpdate = true;
                            Logout();
                            return 0;
                        }
                        else if (input.Item1 == "1")
                        {
                            code_gebruiker.DeleteFeedback(feedback[Convert.ToInt32(pos)].ID, ingelogd.klantgegevens);
                            Console.WriteLine("\n Feedback is verwijderd");
                            Console.WriteLine("Druk op een knop om verder te gaan...");
                            Console.ReadKey();
                            return 5;

                        }
                        else if (input.Item1 == "2")
                        {
                            return 5;
                        }
                        else
                        {
                            Console.WriteLine("\n U moet wel een jusite keuze maken");
                            Console.WriteLine("Druk op een knop om verder te gaan...");
                            Console.ReadKey();
                            return 5;
                        }
                    }
                    page = result.Item1;
                } while (true);
            }
            else if (input.Item1 == "2")
            {
                Console.WriteLine("\n Vul hieronder de datum in vanaf wanneer u uw reviews wilt zien");
                (string, int) choice = AskForInput(5);
                if (choice.Item2 != -1)
                {
                    return choice.Item2;
                }
                int page = 0;
                try
                {

                    DateTime date = Convert.ToDateTime(choice.Item1);
                    if (date >= DateTime.Now)
                    {
                        Console.WriteLine("\nU moet wel een datum in het verleden invoeren.");
                        Console.WriteLine("Druk op en knop om verder te gaan.");
                        Console.ReadKey();
                        return 5;
                    }
                    double pos = 0;
                    List<string> pages = new List<string>();
                    do
                    {
                        pages = new List<string>();
                        List<Feedback> feedbackfiler = feedback.Where(d => d.datum >= date).ToList();
                        List<List<string>> feedbackstring = Makedubbelboxes(FeedbackToString(feedback));
                        List<string> boxes = new List<string>();
                        for (int a = 0; a < feedbackstring.Count; a++)
                        {
                            if (a == feedbackstring.Count - 1 && feedbackstring[a][1].Length < 70)
                            {
                                if (a == Convert.ToInt32(Math.Floor(pos / 2)))
                                {
                                    if (a != 0 && a % 6 != 0)
                                    {
                                        boxes.Add(BoxAroundText(feedbackstring[a], "#", 2, 0, 104, true, new List<string>{
                                    "[3] Bewerken" + new string(' ', 50 - "[3] Bewerken".Length),
                                    "[4] Verwijderen" + new string(' ', 50 - "[4] Verwijderen".Length),
                                    new string(' ', 50)}));
                                    }
                                    else
                                    {
                                        boxes.Add(BoxAroundText(feedbackstring[a], "#", 2, 0, 50, true, new List<string>{
                                    "[3] Bewerken" + new string(' ', 50 - "[3] Bewerken".Length),
                                    "[4] Verwijderen" + new string(' ', 50 - "[4] Verwijderen".Length),
                                    new string(' ', 50)}));
                                    }
                                }
                                else
                                {
                                    if (a != 0 && a % 6 != 0)
                                    {
                                        boxes.Add(BoxAroundText(feedbackstring[a], "#", 2, 0, 104, true));
                                    }
                                    else
                                    {
                                        boxes.Add(BoxAroundText(feedbackstring[a], "#", 2, 0, 50, true));
                                    }

                                }
                            }
                            else
                            {
                                if (a == Convert.ToInt32(Math.Floor(pos / 2)))
                                {
                                    if (pos % 2 == 0 || pos == 0)
                                    {
                                        boxes.Add(BoxAroundText(feedbackstring[a], "#", 2, 0, 104, true, new List<string>{
                                    "[3] Bewerken" + new string(' ', 50 - "[3] Bewerken".Length) + "##  " + new string(' ', 50),
                                    "[4] Verwijderen" + new string(' ', 50 - "[4] Verwijderen".Length) + "##  " + new string(' ', 50),
                                    new string(' ', 50) + "##  " + new string(' ', 50) }));
                                    }
                                    else
                                    {
                                        boxes.Add(BoxAroundText(feedbackstring[a], "#", 2, 0, 104, true, new List<string> {
                                    new string(' ', 50) + "##  " + "[3] Bewerken" + new string(' ', 50 - "[3] Bewerken".Length),
                                    new string(' ', 50) + "##  " + "[4] Verwijderen" + new string(' ', 50 - "[4] Verwijderen".Length),
                                    new string(' ', 50) + "##  " + new string(' ', 50)}));
                                    }
                                }
                                else
                                {
                                    boxes.Add(BoxAroundText(feedbackstring[a], "#", 2, 0, 104, true));
                                }
                            }
                        }

                        pages = MakePages(boxes, 3);
                        if (pages.Count == 0)
                        {
                            Console.WriteLine("\nVanaf deze datum heeft u nog geen feedback geschreven.");
                            Console.WriteLine("Druk op en knop om verder te gaan.");
                            Console.ReadKey();
                            return 5;
                        }
                        Console.Clear();
                        Console.WriteLine(GetGFLogo(true));
                        Console.WriteLine($"Dit is uw feedback op pagina {page + 1} van de {pages.Count}:");
                        Console.WriteLine("De Feedback die is geselecteerd is de feedback met de tekst \"Bewerken\" en \"verwijderen\"");
                        Console.WriteLine("U kunt met de pijltestoetsen navigeren door uw feedback,");
                        Console.WriteLine("De tekst \"Bewerken\" en \"verwijderen\" zal dan van positie veranderen in de richting van welke toets u indrukte");
                        if (feedbackstring[feedbackstring.Count - 1][1].Length < 70 && page == pages.Count - 1)
                        {
                            Console.WriteLine(pages[page] + new string('#', 56));
                        }
                        else
                        {
                            Console.WriteLine(pages[page] + new string('#', 110));
                        }
                        //var result = Nextpage(page, pages.Count - 1, pos, boxes.Count *2 - 1, 8);
                        (int, int, double) result = (0, 0, 0);
                        if (page < pages.Count - 1)
                        {
                            result = Nextpage(page, pos, boxes.Count * 2 - 1, 8,
                                new List<Tuple<(int, int, double), string>> { Tuple.Create((page + 1, -1, (page + 1) * 6.0), "D1"), Tuple.Create((page, 8, pos), "D2"), Tuple.Create((-1, -1, pos), "D4"), Tuple.Create((-2, -2, pos), "D5") },
                                new List<string> { "[1] Volgende pagina", "[2] Terug" });
                        }
                        else
                        {
                            result = Nextpage(page, pos, boxes.Count * 2 - 1, 8,
                                new List<Tuple<(int, int, double), string>> { Tuple.Create((page, 8, pos), "D1"), Tuple.Create((-1, -1, pos), "D4"), Tuple.Create((-2, -2, pos), "D5") },
                                new List<string> { "[1] Terug" });
                        }
                        pos = result.Item3;
                        if (result.Item2 != -1 && result.Item2 != -1)
                        {
                            return result.Item2;
                        }
                        else if (result.Item1 == -1 && result.Item2 == -1)
                        {
                            return EditFeedback(MakeFeedbackBox(feedbackfiler[Convert.ToInt32(pos)]), feedbackfiler[Convert.ToInt32(pos)]);
                        }
                        else if (result.Item1 == -2 && result.Item2 == -2)
                        {
                            Console.Clear();
                            Console.WriteLine(GetGFLogo(true));
                            Console.WriteLine(MakeFeedbackBox(feedbackfiler[Convert.ToInt32(pos)]) + "\n");
                            Console.WriteLine("Weet u zeker dat u deze feedback wilt verwijderen? ja | nee");

                            input = AskForInput(5);
                            if (input.Item2 != -1)
                            {
                                return input.Item2;
                            }
                            else if (input.Item1 == "0")
                            {
                                Console.WriteLine("\nSuccesvol uitgelogd");
                                Console.WriteLine("Druk op een toets om terug te gaan.");
                                Console.ReadKey();
                                //logout sequence
                                logoutUpdate = true;
                                Logout();
                                return 0;
                            }
                            else if (input.Item1 == "ja")
                            {
                                code_gebruiker.DeleteReview(feedback[Convert.ToInt32(pos)].ID, ingelogd.klantgegevens);
                                Console.WriteLine("\n Feedback is verwijderd");
                                Console.WriteLine("Druk op een knop om verder te gaan...");
                                Console.ReadKey();
                                return 8;

                            }
                            else if (input.Item1 == "nee")
                            {
                                return 8;
                            }
                            else
                            {
                                Console.WriteLine("\n U moet wel een jusite keuze maken");
                                Console.WriteLine("Druk op een knop om verder te gaan...");
                                Console.ReadKey();
                                return 8;
                            }
                        }
                        page = result.Item1;
                    } while (true);
                }
                catch
                {
                    Console.WriteLine("U moet wel een geldige datum invullen op deze manier: 1-1-2000");
                    Console.WriteLine("Druk op en knop om verder te gaan.");
                    Console.ReadKey();
                    return 8;
                }
            }
            else if (input.Item1 == "3")
            {
                return 5;
            }
            else if (input.Item1 == "0")
            {
                logoutUpdate = true;
                Logout();
                return 0;
            }
            else
            {
                Console.WriteLine("U moet wel een juiste keuze maken...");
                Console.WriteLine("Druk op en knop om verder te gaan.");
                Console.ReadKey();
                return 5;
            }
        }

        private int EditFeedback(string feedbackstr, Feedback feedback)
        {
            Feedback newfeedback = new Feedback();
            Console.Clear();
            Console.WriteLine(GetGFLogo(true));
            Console.WriteLine("Hier kunt u uw feedback bewerken:");
            Console.WriteLine(feedbackstr + "\n");

            Console.WriteLine("U kunt kiezen of u anoniem feedback wilt geven.");
            Console.WriteLine("Anoniem houdt in:");
            Console.WriteLine("-> U naam word niet opgeslagen met feedback.");
            Console.WriteLine("-> Feedback word niet gekoppeld aan een reservering.");
            Console.WriteLine("-> U kunt feedback niet aanpassen of verwijderen.");
            Console.WriteLine("Wilt u anoniem feedback geven?");
            Console.WriteLine("[1] Ja");
            Console.WriteLine("[2] Nee");
            (string, int) input = AskForInput(8);
            if (input.Item2 != -1)
            {
                return input.Item2;
            }
            else if (input.Item1 == "1")
            {
                newfeedback.annomeme = true;
                newfeedback.klantnummer = -1;
                newfeedback.ID = feedback.ID;
                newfeedback.reservering_ID = -1;
                newfeedback.datum = new DateTime();
            }
            else if (input.Item1 == "2")
            {
                newfeedback.annomeme = false;
                newfeedback.klantnummer = feedback.klantnummer;
                newfeedback.ID = feedback.ID;
                newfeedback.reservering_ID = feedback.reservering_ID;
                newfeedback.datum = feedback.datum;
            }
            else if (input.Item1 == "0")
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
                Console.WriteLine("\n U moet wel een juiste keuze maken");
                Console.WriteLine("Druk op een knop om verder te gaan...");
                Console.ReadKey();
                EditFeedback(feedbackstr, feedback);
                return 8;
            }

            Console.Clear();
            Console.WriteLine(GFLogo);
            Console.WriteLine("Hier kunt u uw feedback bewerken:");
            Console.WriteLine(feedbackstr + "\n");
            Console.WriteLine("Hier schrijft u de inhoud van uw feedback.");
            Console.WriteLine("Met Enter gaat u verder op een nieuwe regel.");
            Console.WriteLine("Als u klaar bent met het typen, type dan  klaar  op een nieuwe regel om uw bericht op te slaan.");
            Console.WriteLine("De inhoud van uw feedback:\n");
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
            if (message.Length == 0)
            {
                Console.WriteLine("\n u moet wel een bericht achterlaten");
                Console.WriteLine("Druk op een knop om verder te gaan...");
                Console.ReadKey();
                EditFeedback(feedbackstr, feedback);
                return 8;
            }
            newfeedback.message = message;

        b:
            Console.Clear();
            Console.WriteLine(GetGFLogo(true));
            Console.WriteLine("Bewerkte Feedback:");
            if (!newfeedback.annomeme)
            {
                Console.WriteLine("Voornaam: " + ingelogd.klantgegevens.voornaam);
                Console.WriteLine("Achternaam: " + ingelogd.klantgegevens.achternaam);
            }
            Console.WriteLine("Bericht: " + newfeedback.message);

            Console.WriteLine("\nWilt u deze bewerking en opslaan?");
            Console.WriteLine("[1] Ja");
            Console.WriteLine("[2] Nee");
            input = AskForInput(8);
            if (input.Item2 != -1)
            {
                return input.Item2;
            }
            else if (input.Item1 == "1")
            {
                if (!newfeedback.annomeme)
                {
                    code_gebruiker.OverwriteFeedback(newfeedback.ID, ingelogd.klantgegevens, newfeedback.message);
                }
                else
                {
                    code_gebruiker.OverwriteFeedback(newfeedback.ID, newfeedback.message);
                }
                Console.WriteLine("\n Feedback is bijgewerkt");
                Console.WriteLine("Druk op een knop om verder te gaan...");
                Console.ReadKey();
                return 5;
            }
            else if (input.Item1 == "2")
            {
                Console.WriteLine("\nDruk op een toets om terug te gaan naar het beginscherm.");
                Console.ReadKey();
                return 5;
            }
            else if (input.Item1 == "0")
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
                Console.WriteLine("\n U moet wel een jusite keuze maken");
                Console.WriteLine("Druk op een knop om verder te gaan...");
                Console.ReadKey();
                goto b;
            }
        }

        private string MakeFeedbackBox(Feedback feedback)
        {
            List<Werknemer> werknemers = new List<Werknemer>(io.GetEmployee());
            string output = "";
            output += new string('#', 56) + "\n";
            output += "#  " + new string(' ', 50) + "  #\n";
            output += "#  " + new string(' ', 50) + "  #\n";
            output += "#  " + "Voornaam: " + ingelogd.klantgegevens.voornaam + new string(' ', 50 - ("Voornaam: " + ingelogd.klantgegevens.voornaam).Length) + "  #\n";
            output += "#  " + "Achternaam: " + ingelogd.klantgegevens.achternaam + new string(' ', 50 - ("Achternaam: " + ingelogd.klantgegevens.achternaam).Length) + "  #\n";

            List<string> msgparts1 = new List<string>();
            string message = feedback.message;

            if (message.Length > 50 - "Feedback: ".Length)
            {
                if (message.LastIndexOf(' ') > 50 || message.LastIndexOf(' ') == -1)
                {
                    msgparts1.Add(message.Substring(0, 50 - "Feedback: ".Length));
                }
                else
                {
                    msgparts1.Add(message.Substring(0, message.Substring(0, 50 - ("Feedback: ").Length).LastIndexOf(' ')));
                }

                message = message.Remove(0, msgparts1[0].Length + 1);
                int count = 1;
                while (message.Length > 50)
                {
                    if (message.LastIndexOf(' ') > 50 || message.LastIndexOf(' ') == -1)
                    {
                        msgparts1.Add(message.Substring(0, 50));
                    }
                    else
                    {
                        msgparts1.Add(message.Substring(0, message.Substring(0, 50).LastIndexOf(' ')));
                    }

                    message = message.Remove(0, msgparts1[count].Length + 1);
                    count++;
                }
                msgparts1.Add(message);

                output += "#  " + "Feedback: " + msgparts1[0] + new string(' ', 50 - ("Feedback: " + msgparts1[0]).Length) + "  #\n";
                for (int a = 1; a < msgparts1.Count; a++)
                {
                    output += "#  " + msgparts1[a] + new string(' ', 50 - msgparts1[a].Length) + "  #\n";
                }
            }
            else
            {
                output += "#  " + "Feedback: " + message + new string(' ', 50 - ("Feedback: " + message).Length) + "  #\n";
            }
            for (int i = 0; i < werknemers.Count; i++)
            {
                if (werknemers[i].ID == feedback.recipient)
                {
                    output += "#  " + "Ontvanger: " + werknemers[i].login_gegevens.klantgegevens.voornaam + " " + werknemers[i].login_gegevens.klantgegevens.achternaam + new string(' ', 50 - ("Ontvanger: " + werknemers[i].login_gegevens.klantgegevens.voornaam + " " + werknemers[i].login_gegevens.klantgegevens.achternaam).Length) + "  #\n";
                }
            }

            output += "#  " + "Datum: " + feedback.datum + new string(' ', 50 - ("Datum: " + feedback.datum).Length) + "  #\n";
            output += "#  " + new string(' ', 50) + "  #\n";
            output += "#  " + new string(' ', 50) + "  #\n";
            output += new string('#', 56);

            return output;
        }

        private List<List<string>> FeedbackToString(List<Feedback> feedback)
        {
            List<Werknemer> werknemers = new List<Werknemer>(io.GetEmployee());
            List<List<string>> output = new List<List<string>>();
            for (int a = 0; a < feedback.Count; a++)
            {
                List<string> block = new List<string>();
                //block += new string('#', 56);
                block.Add(new string(' ', 50));
                block.Add(new string(' ', 50));
                if (!feedback[a].annomeme)
                {
                    block.Add("Voornaam: " + ingelogd.klantgegevens.voornaam + new string(' ', 50 - ("Voornaam: " + ingelogd.klantgegevens.voornaam).Length));
                    block.Add("Achternaam: " + ingelogd.klantgegevens.achternaam + new string(' ', 50 - ("Achternaam: " + ingelogd.klantgegevens.achternaam).Length));
                }
                else
                {
                    block.Add("Anoniem" + new string(' ', 50 - "Anoniem".Length));
                    block.Add(new string(' ', 50));
                }

                List<string> msgparts1 = new List<string>();
                string message = feedback[a].message;

                if (message.Length > 50 - "Feedback: ".Length)
                {
                    if (message.IndexOf(' ') > 50 || message.IndexOf(' ') == -1)
                    {
                        msgparts1.Add(message.Substring(0, 50 - "Feedback: ".Length));
                    }
                    else
                    {
                        msgparts1.Add(message.Substring(0, message.Substring(0, 50 - ("Feedback: ").Length).LastIndexOf(' ')));
                    }

                    message = message.Remove(0, msgparts1[0].Length + 1);
                    int count = 1;
                    while (message.Length > 50)
                    {
                        if (message.IndexOf(' ') > 50 || message.IndexOf(' ') == -1)
                        {
                            msgparts1.Add(message.Substring(0, 50));
                        }
                        else
                        {
                            msgparts1.Add(message.Substring(0, message.Substring(0, 50).LastIndexOf(' ')));
                        }

                        message = message.Remove(0, msgparts1[count].Length + 1);
                        count++;
                    }
                    msgparts1.Add(message);

                    block.Add("Feedback: " + msgparts1[0] + new string(' ', 50 - ("Feedback: " + msgparts1[0]).Length));
                    for (int b = 1; b < 4; b++)
                    {
                        if (b < msgparts1.Count)
                        {
                            block.Add(msgparts1[b] + new string(' ', 50 - msgparts1[b].Length));
                        }
                        else
                        {
                            block.Add(new string(' ', 50));
                        }
                    }
                }
                else
                {
                    block.Add("Feedback: " + message + new string(' ', 50 - ("Feedback: " + message).Length));
                    block.Add(new string(' ', 50));
                    block.Add(new string(' ', 50));
                    block.Add(new string(' ', 50));
                }

                for (int i = 0; i < werknemers.Count; i++)
                {
                    if (werknemers[i].ID == feedback[a].recipient)
                    {
                        block.Add("Ontvanger: " + werknemers[i].login_gegevens.klantgegevens.voornaam + " " + werknemers[i].login_gegevens.klantgegevens.achternaam + new string(' ', 50 - ("Ontvanger: " + werknemers[i].login_gegevens.klantgegevens.voornaam + " " + werknemers[i].login_gegevens.klantgegevens.achternaam).Length));
                    }
                }
                
                if (!feedback[a].annomeme)
                {
                    block.Add("Datum: " + feedback[a].datum + new string(' ', 50 - ("Datum: " + feedback[a].datum).Length));
                }
                else
                {
                    block.Add(new string(' ', 50));
                }

                block.Add(new string(' ', 50));
                block.Add(new string(' ', 50));
                //block += new string('#', 56);

                output.Add(block);
            }


            return output;
        }

        public override List<Screen> Update(List<Screen> screens)
        {
            DoLogoutOnEveryScreen(screens);
            return screens;
        }
    }
    #endregion
}