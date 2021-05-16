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

                //zet het ID van de reservering naar +1 van het aantal dat al is gemaakt
                //ID = database.reserveringen.Count + 1,

                //aantal mensen
                aantal = aantalMensen,

                //voeg alle klantnummers toe
                klantnummer = klantnummer,

                datum = date
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
        /// Maakt non-anoniem feedback aan
        /// </summary>
        /// <param name="werknemer">De werknemer waarvoor de feedback is</param>
        /// <param name="klant">De klant die de feedback geeft</param>
        /// <param name="message">Het bericht die de klant achterlaat</param>
        /// <param name="reservering">De reservering van de klant</param>
        public void MakeFeedback(Werknemer werknemer,Klantgegevens klant, string message)
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
            //slaat de database op
            io.Savedatabase(database);
        }

        /// <summary>
        /// Maakt anoniem feedback aan
        /// </summary>
        /// <param name="werknemer">De werknemer voor wie de feedback is bedoeld</param>
        /// <param name="message">De feedback</param>
        public void MakeFeedback(Werknemer werknemer,string message)
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
    public class MakeReviewScreen : Screen
    {
        public override int DoWork()
        {
            //checkt of je een review mag maken of niet
            #region Check1
            Database database = io.GetDatabase();
            //pak alle oude reserveringen
            List<Reserveringen> reserveringen = new List<Reserveringen>(code_gebruiker.GetCustomerReservation(ingelogd.klantgegevens, false));
            //lijst met alle reserveringen die al een review hebben
            List<Reserveringen> reviewdReservervations = new List<Reserveringen>();
            //lijst met alle reviews van een klant
            List<Review> klantReviews = new List<Review>(io.GetReviews(ingelogd.klantgegevens));
            
            
            //slaat alle reserveringen die al een review hebben op en except deze uit de lijst zodat je alleen niet reviewde reserveringen overhoudt
            for (int i = 0; i < reserveringen.Count; i++)
            {
                for (int j = 0; j < klantReviews.Count; j++)
                {
                    if (reserveringen[i].ID == klantReviews[j].reservering_ID)
                    {
                        reviewdReservervations.Add(reserveringen[i]);
                        break;
                    }
                }
            }
            reserveringen = reserveringen.Except(reviewdReservervations).ToList();
            
            //main logo
            Console.WriteLine(GFLogo);
            
            
            //als er geen open reserveringen meer zijn voor review
            if (reserveringen.Count == 0)
            {
                Console.WriteLine("Er zijn momenteel geen oude reserveringen waarover u een review kunt schrijven.");
                Console.WriteLine("Druk op een toets om terug te gaan.");
                Console.ReadKey();
                //de return key
                return 5;
            }
            #endregion
            //checkt of de review die je hebt gekozen bestaat
            #region Check2
            else
            {
                Console.WriteLine("Hier kunt u een review maken.");
                Console.WriteLine("U kunt een review schrijven per reservering.");
                Console.WriteLine("U kunt kiezen uit een van de volgende reserveringen:");
                
                //list met alle IDs van reserveringen die nog geen review hebben
                for (int i = 0; i < reserveringen.Count; i++)
                {
                    Console.WriteLine(reserveringen[i].ID+ " | "+ reserveringen[i].aantal+ " | "+ reserveringen[i].datum);
                }
                
                Console.WriteLine("\"ID | aantal mensen | datum\"");
                Console.WriteLine("Is het formaat van deze weergave.\n");
                Console.WriteLine("Met het ID kunt u selecteren over welk bezoek u een review wilt schrijven.");
                Console.WriteLine("Het ID van u reservering:");
                
                string choice = Console.ReadLine();
                //als de input niet een van de getallen is in de lijst met IDs, invalid input
                if (!reserveringen.Select(i => i.ID).ToList().Contains(Convert.ToInt32(choice)))
                {
                    //invalid input message here
                }
                #endregion
                // checkt input: 1 voor anoniem, 2 voor normaal, 3 voor terug
                #region Check3
                else
                {
                    //de reservering ophalen waarover een review word geschreven, word later gebruikt
                    Reserveringen chosenReservation = new Reserveringen();
                    for (int i = 0; i < reserveringen.Count; i++)
                    {
                        if (reserveringen[i].ID == Convert.ToInt32(choice))
                        {
                            chosenReservation = reserveringen[i];
                            break;
                        }
                    }
                    
                    
                    //begin met een een clear screen hier
                    Console.Clear();
                    Console.WriteLine(GFLogo);
                    Console.WriteLine("Er is een mogelijkheid om anoniem een review te geven, anoniem houdt in:");
                    Console.WriteLine("-> U naam word niet opgeslagen met de review.");
                    Console.WriteLine("-> De review word niet gekoppeld aan deze reservering.");
                    Console.WriteLine("-> U kunt deze review niet aanpassen of verwijderen.");
                    //Console.WriteLine("Kies [1] voor het maken voor een anonieme review, kies [2] voor het maken van een normale review.");
                    Console.WriteLine("[1] Anoniem");
                    Console.WriteLine("[2] Normaal");
                    Console.WriteLine("[3] Terug");
                    choice = Console.ReadLine();

                    //list met mogelijke inputs
                    List<string> possibleInput = new List<string> { "1", "2", "3" };

                    if (!possibleInput.Contains(choice))
                    {
                        Console.WriteLine("U moet wel een juiste keuze maken...");
                        Console.WriteLine("Druk op en knop om terug te gaan.");
                        Console.ReadKey();
                        //naar welk scherm gereturned moet worden als de input incorrect is
                        return 7;
                    }
                    #endregion
                    else
                    {
                        //clear screen
                        Console.Clear();
                        Console.WriteLine(GFLogo);


                        if (choice == "1")
                        {
                            //anoniem Review, alleen rating en message
                            Console.WriteLine("De beoordeling die u dit restaurant wilt geven van 1 tot 5 waarin,");
                            Console.WriteLine("1 is het slechtste en 5 is het beste");

                            choice = Console.ReadLine();

                            possibleInput = new List<string> { "1", "2", "3", "4", "5" };

                            //check voor de rating
                            if (!possibleInput.Contains(choice))
                            {
                                Console.WriteLine("U heeft een incorrecte beoordeling ingevoerd.");
                                Console.WriteLine("Druk op een knop om terug te gaan.");
                                Console.ReadKey();

                                //naar welk scherm gereturned moet worden als de input incorrect is
                                return 7;
                            }
                            else
                            {
                                //zet rating naar een int, word later gebruikt
                                int rating = Convert.ToInt32(choice);

                                Console.WriteLine("Als laatste, de inhoud van u review.");
                                Console.WriteLine("Met Enter gaat u verder op een nieuwe regel.");
                                Console.WriteLine("Als u klaar bent met het typen van u review, type dan \"klaar\" op een nieuwe regel.");
                                Console.WriteLine("De inhoud van de review:");

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
                                code_gebruiker.MakeReview(rating, message);
                                Console.WriteLine("Succesvol een review gemaakt.");
                                Console.WriteLine("Druk op een toets om terug te gaan.");
                                Console.ReadKey();

                                //return naar het vorige scherm pls
                                return 5;
                            }
                        }
                        if (choice == "2")
                        {
                            //rating, message en klantgegevens/naam klant
                            Console.WriteLine("De beoordeling die u dit restaurant wilt geven van 1 tot 5 waarin:");
                            Console.WriteLine("1 het slechtste is en 5 het beste.");

                            choice = Console.ReadLine();

                            possibleInput = new List<string> { "1", "2", "3", "4", "5" };

                            //check voor de rating
                            if (!possibleInput.Contains(choice))
                            {
                                Console.WriteLine("U heeft een incorrecte beoordeling ingevoerd.");
                                Console.WriteLine("Druk op een knop om terug te gaan.");
                                Console.ReadKey();

                                //naar welk scherm gereturned moet worden als de input incorrect is
                                return 7;
                            }
                            else
                            {
                                //zet rating naar een int, word later gebruikt
                                int rating = Convert.ToInt32(choice);

                                //nog een clear screen?


                                Console.WriteLine("Als laatste, de inhoud van u review.");
                                Console.WriteLine("Met Enter gaat u verder op een nieuwe regel.");
                                Console.WriteLine("Als u klaar bent met het typen van u review, type dan \"klaar\" op een nieuwe regel.");
                                Console.WriteLine("De inhoud van de review:");

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
                                code_gebruiker.MakeReview(rating, ingelogd.klantgegevens, message, chosenReservation);
                                Console.WriteLine("Succesvol een review gemaakt.");
                                Console.WriteLine("Druk op een toets om terug te gaan.");
                                Console.ReadKey();
                                //return naar het vorige scherm pls
                                return 5;
                            }
                        }
                        if (choice == "3")
                        {
                            //return naar vorig scherm
                            return 5;
                        }
                    }
                }
                return 5;
            }
        }
        public override List<Screen> Update(List<Screen> screens)
        {
            return screens;
        }
    }

    #endregion
}