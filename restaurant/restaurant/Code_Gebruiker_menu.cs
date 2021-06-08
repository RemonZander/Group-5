using System;
using System.Collections.Generic;
using System.Linq;

namespace restaurant
{
    public partial class Code_Gebruiker_menu
    {
        Database database = new Database();
        IO io = new IO();
        Testing_class testClass = new Testing_class();

        public void Debug()
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
            if (database.reserveringen == null)
            {
                return new List<Reserveringen>();
            }

            //voor elke reservering in de database
            List<Reserveringen> reservering = new List<Reserveringen>();
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
            if (database.reserveringen == null)
            {
                return new List<Reserveringen>();
            }

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
            database = io.GetDatabase();
            if (database.reserveringen != null)
            {
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
            database = io.GetDatabase();
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

        /**
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
        **/
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
            if (database.reviews != null)
            {
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
            if (database.reviews != null)
            {
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
            if (database.reviews != null)
            {
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
            if (database.feedback != null)
            {
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
            if (database.feedback != null)
            {
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
        }
        #endregion

        #region Deprecated

        #endregion
    }
    
    #region Screens
    class MakeFeedbackScreen : Screen
    {
        //het ID van dit scherm
        private readonly int huidigScherm = 9;
        //het ID van klanten menu
        private readonly int vorigScherm = 5;
        
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
                Console.Clear();
                Console.WriteLine(GetGFLogo(false));
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
            Console.WriteLine("Het is mogelijk om anoniem feedback te schrijven, anoniem houdt in:");
            Console.WriteLine("-> Uw naam wordt niet opgeslagen bij de feedback.");
            Console.WriteLine("-> De feedback wordt niet gekoppeld aan deze reservering.");
            Console.WriteLine("-> U kunt deze feedback niet aanpassen en/of verwijderen.");
            Console.WriteLine("[1] Anoniem");
            Console.WriteLine("[2] Normaal");
            Console.WriteLine("[3] Terug");
            (string, int)key = AskForInput(vorigScherm);

            if (key.Item2 != -1)
            {
                return vorigScherm;
            }

            //reservering ID heb ik later nodig buiten de scope
            int ReserveringID = -1;

            //feedback anoniem
            if (key.Item1 == "1")
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
                        Console.WriteLine("Kies hier een medewerker aan wie u uw feedback wilt richten.");
                        Console.WriteLine("\nDe lijst met medewerkers:");
                        Console.WriteLine(new string('–', 46) + "\n| ID     | Naam                              |\n" + new string('–', 46));
                        for (int j = 0; j < Medewerkers.Count; j++)
                        {
                            //Console.WriteLine("Naam:  " + Medewerkers[j].login_gegevens.klantgegevens.voornaam + " " + Medewerkers[j].login_gegevens.klantgegevens.achternaam);
                            Console.WriteLine("|" + Medewerkers[j].ID + new string(' ', 8 - Medewerkers[j].ID.ToString().Length) + "|" + Medewerkers[j].login_gegevens.klantgegevens.voornaam + " " + Medewerkers[j].login_gegevens.klantgegevens.achternaam +  new string(' ', 35 - (Medewerkers[j].login_gegevens.klantgegevens.voornaam + " " + Medewerkers[j].login_gegevens.klantgegevens.achternaam).Length) +  "|\n" + new string('–', 46));
                        }
                        Console.WriteLine("\nVoer hieronder de voor- en achternaam in van de medewerker aan wie u uw feedback wilt richten.");
                        Console.WriteLine("Druk vervolgens op enter om uw keuze te bevestigen.");
                        Console.WriteLine("Wilt u uw feedback richten aan de eigenaar van het restaurant, voer dan niks in en druk gelijk op Enter.");
                        Console.WriteLine("LET OP! Het invoeren van de naam is hoofdlettergevoelig!");

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
                            return LogoutSequence();
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
                Console.WriteLine(GetGFLogo(false));

                if (feedbackVoorEigenaar)
                {
                    Eigenaar eigenaar = io.GetEigenaar();
                    Console.WriteLine($"De feedback die u gaat schrijven is voor {eigenaar.login_gegevens.klantgegevens.voornaam} {eigenaar.login_gegevens.klantgegevens.achternaam} (Eigenaar).");
                }
                else
                {
                    Console.WriteLine($"De feedback die u gaat schrijven is voor { feedbackMedewerker.login_gegevens.klantgegevens.voornaam} {feedbackMedewerker.login_gegevens.klantgegevens.achternaam} (Medewerker).");
                }
                Console.WriteLine("\nHieronder kunt u de inhoud van uw feedback schrijven (max. 160 tekens).");
                bool succes = false;
                string message = "";
                do
                {
                    message = Console.ReadLine();

                    if (message.Length > 160)
                    {
                        Console.WriteLine("Uw feedback mag niet langer zijn dan 160 tekens.\nHieronder kunt u opnieuw feedback schrijven.");
                    }
                    else
                    {
                        succes = true;
                    }
                } while (!succes);
                if (feedbackVoorEigenaar)
                {
                    code_gebruiker.MakeFeedback(io.GetEigenaar(), message);
                    Console.WriteLine("\nSuccesvol feedback aangemaakt.");
                    Console.WriteLine("Druk op een toets om terug te gaan naar het klantenmenu.");
                    Console.ReadKey();
                    return vorigScherm;
                }
                else
                {
                    code_gebruiker.MakeFeedback(feedbackMedewerker, message);
                    Console.WriteLine("\nSuccesvol feedback aangemaakt.");
                    Console.WriteLine("Druk op een toets om terug te gaan naar het klantenmenu.");
                    Console.ReadKey();
                    return vorigScherm;
                }
            }
            //feedback normaal
            else if (key.Item1 == "2")
            {
                //bool voor de do-while loop
                bool repeat = false;

                //voor het kiezen van een reservering en krijgen van ReserveringID
                do
                {
                    Console.Clear();
                    Console.WriteLine(GetGFLogo(true));
                    Console.WriteLine("Hier kunt u een oude reservering kiezen, waarover u feedback wilt schrijven.");
                    Console.WriteLine("U kunt één keer feedback schrijven per reservering.");
                    Console.WriteLine("\nU kunt kiezen uit één van de onderstaande reserveringen:");
                    Console.WriteLine(new string('–', 44) + "\n|ID     |Aantal mensen | Datum             |\n" + new string('–', 44));
                    for (int i = 0; i < reserveringen.Count; i++)
                    {
                        Console.WriteLine("|" + reserveringen[i].ID + new string(' ', 7 - reserveringen[i].ID.ToString().Length) + "| " + reserveringen[i].aantal + new string(' ', 13 - reserveringen[i].aantal.ToString().Length) + "| " + reserveringen[i].datum.ToShortDateString() + " " + reserveringen[i].datum.ToShortTimeString() + new string(' ', 18-(reserveringen[i].datum.ToShortDateString() + " " + reserveringen[i].datum.ToShortTimeString()).Length) + "|\n" + new string('–', 44));
                    }
                    Console.WriteLine("\nMet het ID kunt u selecteren over welk bezoek u feedback wilt schrijven.");
                    Console.WriteLine("Voer een ID in en druk op enter.");

                    key = AskForInput(huidigScherm);
                    if (key.Item2 != -1)
                    {
                        return huidigScherm;
                    }

                    //als input is 0, logout
                    if (key.Item1 == "0")
                    {
                        return LogoutSequence();
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

                //voor wie de feedback is
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

                    Console.WriteLine("Kies hier een medewerker aan wie u uw feedback wilt richten.");
                    Console.WriteLine("\nDe lijst met medewerkers:");
                    Console.WriteLine(new string('–', 46) + "\n| ID     | Naam                              |\n" + new string('–', 46));
                    for (int j = 0; j < Medewerkers.Count; j++)
                    {
                        //Console.WriteLine("Naam:  " + Medewerkers[j].login_gegevens.klantgegevens.voornaam + " " + Medewerkers[j].login_gegevens.klantgegevens.achternaam);
                        Console.WriteLine("|" + Medewerkers[j].ID + new string(' ', 8 - Medewerkers[j].ID.ToString().Length) + "|" + Medewerkers[j].login_gegevens.klantgegevens.voornaam + " " + Medewerkers[j].login_gegevens.klantgegevens.achternaam + new string(' ', 35 - (Medewerkers[j].login_gegevens.klantgegevens.voornaam + " " + Medewerkers[j].login_gegevens.klantgegevens.achternaam).Length) + "|\n" + new string('–', 46));
                    }
                    Console.WriteLine("\nVoer hieronder de voor- en achternaam in van de medewerker aan wie u uw feedback wilt richten.");
                    Console.WriteLine("Druk vervolgens op enter om uw keuze te bevestigen.");
                    Console.WriteLine("Wilt u uw feedback richten aan de eigenaar van het restaurant, voer dan niks in en druk gelijk op Enter.");
                    Console.WriteLine("LET OP! Het invoeren van de naam is hoofdlettergevoelig!");

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
                        return LogoutSequence();
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
                    if (feedbackVoorEigenaar == false && feedbackMedewerker.login_gegevens == null)
                    {
                        Console.WriteLine("\nEr is geen medewerker met deze naam.");
                        Console.WriteLine("druk op een toets om opnieuw te proberen.");
                        Console.ReadKey();
                    }
                } while (!((feedbackVoorEigenaar != false && feedbackMedewerker.login_gegevens == null) || (feedbackVoorEigenaar == false && feedbackMedewerker.login_gegevens != null)));

                Console.Clear();
                Console.WriteLine(GetGFLogo(false));

                if (feedbackVoorEigenaar)
                {
                    Eigenaar eigenaar = io.GetEigenaar();
                    Console.WriteLine($"De feedback die u gaat schrijven is voor {eigenaar.login_gegevens.klantgegevens.voornaam} {eigenaar.login_gegevens.klantgegevens.achternaam} (Eigenaar).");
                }
                else
                {
                    Console.WriteLine($"De feedback die u gaat schrijven is voor { feedbackMedewerker.login_gegevens.klantgegevens.voornaam} {feedbackMedewerker.login_gegevens.klantgegevens.achternaam} (Medewerker).");
                }
                Console.WriteLine("\nHieronder kunt u de inhoud van uw feedback schrijven (max. 160 tekens).");

                bool succes = false;
                string message = "";
                do
                {
                    message = Console.ReadLine();
                    if (message.Length > 160)
                    {
                        Console.WriteLine("Uw feedback mag niet langer zijn dan 160 tekens.\nHieronder kunt u opnieuw feedback schrijven.");
                    }
                    else
                    {
                        succes = true;
                    }
                } while (!succes);
                if (feedbackVoorEigenaar)
                {
                    code_gebruiker.MakeFeedback(io.GetEigenaar(), ingelogd.klantgegevens, message, ReserveringID);
                    Console.WriteLine("\nSuccesvol feedback aangemaakt.");
                    Console.WriteLine("Druk op een toets om terug te keren naar het klantenmenu.");
                    Console.ReadKey();
                    return vorigScherm;
                }
                else
                {
                    code_gebruiker.MakeFeedback(feedbackMedewerker, ingelogd.klantgegevens, message, ReserveringID);
                    Console.WriteLine("\nSuccesvol feedback aangemaakt.");
                    Console.WriteLine("Druk op een toets om terug te keren naar het klantenmenu.");
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
            //logout
            else if (key.Item1 == "0")
            {
                return LogoutSequence();
            }
            //wrong input
            else
            {
                //error message
                Console.WriteLine("\nU moet wel een juiste keuze maken.");
                Console.WriteLine("Druk op een toets om opniew te kiezen.");
                Console.ReadKey();
                return huidigScherm;
            }
        }

        private int LogoutSequence()
        {
            Console.WriteLine("\nSuccesvol uitgelogd");
            Console.WriteLine("Druk op een toets om terug te gaan.");
            Console.ReadKey();
            //logout sequence
            logoutUpdate = true;
            Logout();
            return 0;
        }

        public override List<Screen> Update(List<Screen> screens)
        {
            DoLogoutOnEveryScreen(screens);
            return screens;
        }
    }

    class ViewFeedbackScreen : Screen
    {
        private readonly int huidigscherm = 8;
        private readonly int vorigscherm = 5;
        
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
                Console.WriteLine("Druk op een toets om terug te gaan");
                Console.ReadKey();
                return vorigscherm;
            }

            Console.WriteLine(GetGFLogo(true));
            Console.WriteLine("Hier kunt u uw eigen feedback zien en bewerken:");
            Console.WriteLine("[1] Alle feedback");
            Console.WriteLine("[2] Feedback vanaf een bepaalde datum (genoteerd als dag-maand-jaar)");
            Console.WriteLine("[3] Ga terug naar het klantenmenu");

            //als escape, ga terug naar scherm 5
            (string, int) input = AskForInput(vorigscherm);
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
                                if (a != 0 && a % 3 != 0)
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
                                if (a != 0 && a % 3 != 0)
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
                    int oneven = 0;
                    Console.Clear();
                    Console.WriteLine(GetGFLogo(true));
                    Console.WriteLine($"Dit is uw feedback op pagina {page + 1} van de {pages.Count}:");
                    Console.WriteLine("Gebruik de pijltjestoetsen om te navigeren door uw gegeven feedback.\nDe feedback met de tekst \"Bewerken\" en \"Verwijderen\" is de huidig geselecteerde feedback.");

                    //schrijft de pagina samen met de bottom symbols
                    if (feedbackstring[feedbackstring.Count - 1][1].Length < 70 && page == pages.Count - 1)
                    {
                        Console.WriteLine(pages[page] + new string('#', 56));
                        oneven = 1;
                    }
                    else
                    {
                        Console.WriteLine(pages[page] + new string('#', 110));
                    }

                    //var result = Nextpage(page, pages.Count - 1, pos, boxes.Count * 2 - 1, 8);
                    (int, int, double) result = (0, 0, 0);
                    if (page < pages.Count - 1)
                    {
                        result = Nextpage(page, pos, boxes.Count * 2 -(1 + oneven), 8,
                            new List<Tuple<(int, int, double), string>> { Tuple.Create((page + 1, -1, (page + 1) * 6.0), "D1"), Tuple.Create((page, 8, pos), "D2"), Tuple.Create((-1, -1, pos), "D3"), Tuple.Create((-2, -2, pos), "D4") },
                            new List<string> { "[1] Volgende pagina", "[2] Terug" });
                    }
                    else
                    {
                        result = Nextpage(page, pos, boxes.Count * 2 - (1 + oneven), 8,
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
                            return LogoutSequence();
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
                        Console.WriteLine("Druk op en toets om terug te gaan.");
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
                        int oneven = 0;
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
                        Console.WriteLine("Gebruik de pijltjestoetsen om te navigeren door uw gegeven feedback.\nDe feedback met de tekst \"Bewerken\" en \"Verwijderen\" is de huidig geselecteerde feedback.");
                        if (feedbackstring[feedbackstring.Count - 1][1].Length < 70 && page == pages.Count - 1)
                        {
                            Console.WriteLine(pages[page] + new string('#', 56));
                            oneven = 1;
                        }
                        else
                        {
                            Console.WriteLine(pages[page] + new string('#', 110));
                        }
                        //var result = Nextpage(page, pages.Count - 1, pos, boxes.Count *2 - 1, 8);
                        (int, int, double) result = (0, 0, 0);
                        if (page < pages.Count - 1)
                        {
                            result = Nextpage(page, pos, boxes.Count * 2 - (1 + oneven), 8,
                                new List<Tuple<(int, int, double), string>> { Tuple.Create((page + 1, -1, (page + 1) * 6.0), "D1"), Tuple.Create((page, 8, pos), "D2"), Tuple.Create((-1, -1, pos), "D4"), Tuple.Create((-2, -2, pos), "D5") },
                                new List<string> { "[1] Volgende pagina", "[2] Terug" });
                        }
                        else
                        {
                            result = Nextpage(page, pos, boxes.Count * 2 - (1 + oneven), 8,
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
                                return LogoutSequence();
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
                return LogoutSequence();
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
            #region Anoniem
            Feedback newfeedback = new Feedback();
            Console.Clear();
            Console.WriteLine(GetGFLogo(true));
            Console.WriteLine("Hier kunt u uw feedback bewerken.");
            Console.WriteLine(feedbackstr + "\n");
            Console.WriteLine("Het is mogelijk om anoniem een feedback te schrijven, anoniem houdt in:");
            Console.WriteLine("-> Uw naam wordt niet opgeslagen bij de feedback.");
            Console.WriteLine("-> De feedback wordt niet gekoppeld aan deze reservering.");
            Console.WriteLine("-> U kunt deze feedback niet bewerken en/of verwijderen.");
            Console.WriteLine("\nWilt u anoniem feedback geven?");
            Console.WriteLine("Type ja of nee.");
            
            //input, als escape - terug, als 0 - logout sequence
            (string, int) input = AskForInput(huidigscherm);
            if (input.Item2 != -1)
            {
                return input.Item2;
            }
            else if (input.Item1 == "ja")
            {
                newfeedback.annomeme = true;
                newfeedback.klantnummer = -1;
                newfeedback.ID = feedback.ID;
                newfeedback.reservering_ID = -1;
                newfeedback.datum = new DateTime();
            }
            else if (input.Item1 == "nee")
            {
                newfeedback.annomeme = false;
                newfeedback.klantnummer = feedback.klantnummer;
                newfeedback.ID = feedback.ID;
                newfeedback.reservering_ID = feedback.reservering_ID;
                newfeedback.datum = feedback.datum;
            }
            //logout sequence
            else if (input.Item1 == "0")
            {
                return LogoutSequence();
            }
            //wrong input message
            else
            {
                Console.WriteLine("\n U moet wel een juiste keuze maken");
                Console.WriteLine("Druk op een knop om verder te gaan...");
                Console.ReadKey();
                EditFeedback(feedbackstr, feedback);
                return 8;
            }
            #endregion

            #region Bericht
            Console.WriteLine("\n Uw feedback mag niet langer zijn dan 160 tekens.\nHieronder kunt u opnieuw feedback schrijven.");
            input = AskForInput(huidigscherm);
            if (input.Item2 != -1)
            {
                return input.Item2;
            }
            else if (input.Item1.Length > 160)
            {
                Console.WriteLine("\n Uw bericht mag niet langer zijn dan 160 tekens");
                Console.WriteLine("Druk op een toets om opnieuw te proberen.");
                Console.ReadKey();
                EditFeedback(feedbackstr, feedback);
            }
            else if (input.Item1.Length == 0)
            {
                Console.WriteLine("\n Graag iets invullen.");
                Console.WriteLine("Druk op een toets om opnieuw te proberen.");
                Console.ReadKey();
                EditFeedback(feedbackstr, feedback);
            }
            else if (input.Item1 == "0")
            {
                return LogoutSequence();
            }
            feedback.message = input.Item1;
        #endregion

            #region Confirm
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

            Console.WriteLine("\nWilt u deze feedback opslaan? ja | nee.");
            input = AskForInput(huidigscherm);
            if (input.Item2 != -1)
            {
                return input.Item2;
            }
            else if (input.Item1 == "ja")
            {
                if (!newfeedback.annomeme)
                {
                    code_gebruiker.OverwriteFeedback(newfeedback.ID, ingelogd.klantgegevens, newfeedback.message);
                }
                else
                {
                    code_gebruiker.OverwriteFeedback(newfeedback.ID, newfeedback.message);
                }
                Console.WriteLine("\nUw feedback is succesvol bijgewerkt.");
                Console.WriteLine("Druk op een toets om terug te keren naar het klantenmenu.");
                Console.ReadKey();
                return 5;
            }
            else if (input.Item1 == "nee")
            {
                Console.WriteLine("\nUw feedback is NIET bijgewerkt.\nDruk op een toets om terug te keren naar het klantenmenu.");
                Console.ReadKey();
                return 5;
            }
            else if (input.Item1 == "0")
            {
                return LogoutSequence();
            }
            else
            {
                Console.WriteLine("\n U moet wel een jusite keuze maken");
                Console.WriteLine("Druk op een toets om opniew te proberen.");
                Console.ReadKey();
                goto b;
            }
            #endregion
        }

        private string MakeFeedbackBox(Feedback feedback)
        {
            //pak alle medewerkers
            List<Werknemer> werknemers = new List<Werknemer>(io.GetEmployee());
            string output = "";
            //top of box
            output += new string('#', 56) + "\n";
            //twee witregels
            output += "#  " + new string(' ', 50) + "  #\n";
            output += "#  " + new string(' ', 50) + "  #\n";
            //voornaam+ achternaam klant
            output += "#  " + "Voornaam: " + ingelogd.klantgegevens.voornaam + new string(' ', 50 - ("Voornaam: " + ingelogd.klantgegevens.voornaam).Length) + "  #\n";
            output += "#  " + "Achternaam: " + ingelogd.klantgegevens.achternaam + new string(' ', 50 - ("Achternaam: " + ingelogd.klantgegevens.achternaam).Length) + "  #\n";


            List<string> msgparts1 = new List<string>();
            string message = feedback.message;

            //cut messages in stukken van 50 char en add aan lijst
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

                //verwijder de eerste want die is net geplaatst
                message = message.Remove(0, msgparts1[0].Length + 1);
                //regel1
                int count = 1;
                //als bericht nog steeds langer is dan 50 char, cut ze tot het hele bericht 50 char is
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

                //voeg line 0 toe
                output += "#  " + "Feedback: " + msgparts1[0] + new string(' ', 50 - ("Feedback: " + msgparts1[0]).Length) + "  #\n";
                
                //voeg de andere lines toe
                for (int a = 1; a < msgparts1.Count; a++)
                {
                    output += "#  " + msgparts1[a] + new string(' ', 50 - msgparts1[a].Length) + "  #\n";
                }
            }
            //voeg een lijn toe want is niet meer dan 50 char
            else
            {
                output += "#  " + "Feedback: " + message + new string(' ', 50 - ("Feedback: " + message).Length) + "  #\n";
            }
            //zoek door de werknemers en als medewerker is gevonden, stop zijn naam erbij
            for (int i = 0; i < werknemers.Count; i++)
            {
                if (werknemers[i].ID == feedback.recipient)
                {
                    output += "#  " + "Ontvanger: " + werknemers[i].login_gegevens.klantgegevens.voornaam + " " + werknemers[i].login_gegevens.klantgegevens.achternaam + new string(' ', 50 - ("Ontvanger: " + werknemers[i].login_gegevens.klantgegevens.voornaam + " " + werknemers[i].login_gegevens.klantgegevens.achternaam).Length) + "  #\n";
                }
            }

            //datum van feedback
            output += "#  " + "Datum: " + feedback.datum + new string(' ', 50 - ("Datum: " + feedback.datum).Length) + "  #\n";
            //bottom enters
            output += "#  " + new string(' ', 50) + "  #\n";
            output += "#  " + new string(' ', 50) + "  #\n";
            //floor box
            output += new string('#', 56);

            return output;
        }

        private int LogoutSequence()
        {
            Console.WriteLine("\nSuccesvol uitgelogd");
            Console.WriteLine("Druk op een toets om terug te gaan.");
            Console.ReadKey();
            //logout sequence
            logoutUpdate = true;
            Logout();
            return 0;
        }

        public override List<Screen> Update(List<Screen> screens)
        {
            DoLogoutOnEveryScreen(screens);
            return screens;
        }
    }

    class PaymentScreen : Screen
    {
        private readonly int huidigscherm = 9;
        private readonly int vorigscherm = 5;
        public override int DoWork()
        {
            //alle oude reserveringen van de klant
            List<Reserveringen> reserveringen = new List<Reserveringen>(code_gebruiker.GetCustomerReservation(ingelogd.klantgegevens, false));
            List<Reserveringen> exemptReserveringen = new List<Reserveringen>();
            //als er geen oude reserveringen zijn
            if (reserveringen.Count == 0)
            {
                Console.Clear();
                Console.WriteLine(GFLogoWithLogin);
                Console.WriteLine("Er staat niks open voor betaling");
                Console.WriteLine("Druk op een toets om terug te gaan");
                return vorigscherm;
            }
            
            //als reservering al betaald is of heeft geen gerechten besteld, haal deze weg
            foreach (var reservering in reserveringen)
            {
                if (reservering.isBetaald == true || reservering.gerechten_ID == null || reservering.gerechten_ID.Count == 0)
                {
                    exemptReserveringen.Add(reservering);
                }
            }
            
            reserveringen = reserveringen.Except(exemptReserveringen).ToList();

            //er staat niks meer open voor betaling
            if (reserveringen.Count == 0)
            {
                Console.Clear();
                Console.WriteLine(GFLogoWithLogin);
                Console.WriteLine("Er staat niks open voor betaling");
                Console.WriteLine("Druk op een toets om terug te gaan");
                return vorigscherm;
            }
            
            bool repeat = true;
            Reserveringen chosenReservering = new Reserveringen();
            (string, int) input;
            //check of de input een van de ID's is
            do
            {
                Console.Clear();
                Console.WriteLine(GFLogoWithLoginAndEscape);
                Console.WriteLine("Hier ziet u alle Reserveringen die nog open staan voor betaling.");
                Console.WriteLine(new string('–', 31) + "\n| ID   | Datum                |\n" + new string('–', 31));

                for (int i = 0; i < reserveringen.Count; i++)
                {
                    Console.WriteLine("| " + reserveringen[i].ID + new string(' ', 5 - reserveringen[i].ID.ToString().Length) + "| " + reserveringen[i].datum + new string(' ', 21 - reserveringen[i].datum.ToString().Length) + "|\n" + new string('–', 31));
                }

                Console.WriteLine("U kunt met het ID kiezen welke reservering u wilt betalen.");

                //met escape, return naar klantmenu
                input = AskForInput(vorigscherm);
                if (input.Item2 != -1)
                {
                    return input.Item2;
                }

                //als input is 0, logout
                if (input.Item1 == "0")
                {
                    return LogoutSequence();
                }

                //is om te kijken of choice een int is
                try
                {
                    //kijkt of de invoer in de lijst is, zo niet dan invalid input
                    if (!reserveringen.Select(i => i.ID).ToList().Contains(Convert.ToInt32(input.Item1)))
                    {
                        Console.WriteLine("\nU moet wel geldige Reservering invoeren.");
                        Console.WriteLine("Druk op een toets om opnieuw te kiezen.");
                        Console.ReadKey();
                    }
                    //als die wel in de lijst zoek die dan en sla deze op
                    for (int i = 0; i < reserveringen.Count; i++)
                    {
                        if (reserveringen[i].ID == Convert.ToInt32(input.Item1))
                        {
                            //breek beide loops en sla de reservering op
                            repeat = false;
                            chosenReservering = reserveringen[i];
                            break;
                        }
                    }
                }
                //geen int ingevoerd
                catch
                {
                    Console.WriteLine("\nU moet wel een reservering invoeren.");
                    Console.WriteLine("Druk op een toets om verder te gaan.");
                    Console.ReadKey();
                }
            } while (repeat);


            while (true)
            {
                Console.Clear();
                Console.WriteLine(GetGFLogo(true));
                Console.WriteLine($"De reservering die u heeft gekozen heeft nummer: {chosenReservering.ID} en is van {chosenReservering.datum}");
                Console.WriteLine("Hier ziet u wat er allemaal was besteld:");
                Console.WriteLine("\n" + BestelBox(chosenReservering));
                Console.WriteLine("\n" + BetaalBox(chosenReservering));
                Console.WriteLine("U kunt betalen door uw pin in te voeren");
                input = AskForInput(vorigscherm);
                if (input.Item2 != -1)
                {
                    return vorigscherm;
                }

                //als input is 0, logout
                if (input.Item1 == "0")
                {
                    return LogoutSequence();
                }
                //dummygetal die ik ff nodig had
                int keyCode;
                //als de lengte van de input 4 is en het is een getal 
                if (input.Item1.Length == 4 && int.TryParse(input.Item1, out keyCode))
                {
                    Console.WriteLine("\nBetaling was succesvol");
                    Console.WriteLine("Druk op een toets om terug te gaan naar het klantenmenu.");
                    io.ReserveringBetalen(chosenReservering);
                    Console.ReadKey();
                    return vorigscherm;
                }
                //anders doe error message en ga er nog een keer doorheen
                else
                {
                    Console.WriteLine("U moet wel vier getallen invoeren");
                    Console.WriteLine("Druk op een toets om opnieuw te proberen");
                    Console.ReadKey();
                }
            }
        }

        private int LogoutSequence()
        {
            Console.WriteLine("\nSuccesvol uitgelogd");
            Console.WriteLine("Druk op een toets om terug te gaan.");
            Console.ReadKey();
            //logout sequence
            logoutUpdate = true;
            Logout();
            return 0;
        }

        public override List<Screen> Update(List<Screen> screens)
        {
            DoLogoutOnEveryScreen(screens);
            return screens;
        }
    }
    #endregion
}