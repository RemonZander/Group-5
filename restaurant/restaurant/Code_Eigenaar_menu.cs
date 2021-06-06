using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;

namespace restaurant
{
    public partial class Code_Eigenaar_menu
    {
        private Database database = new Database();

        IO io = new IO();
        Testing_class instance = new Testing_class();

        public Code_Eigenaar_menu()
        {
        }

        public void Debug()
        {

        }
        #region Meal

        public void DeleteMeal(int ID)
        {
            database = io.GetDatabase();

            List<Gerechten> gerechten = new List<Gerechten>(GetMeals());
            for (int i = 0; i < gerechten.Count; i++)
            {
                if (gerechten[i].ID == ID)
                {
                    gerechten[i] = null;
                    break;
                }
            }
            gerechten.RemoveAll(x => x == null);
            database.menukaart.gerechten = gerechten;
            io.Savedatabase(database);
        }

        public void ArchiveMeal(int ID)
        {
            database = io.GetDatabase();
            List<Gerechten> gerechten = new List<Gerechten>(GetMeals());
            for (int i = 0; i < gerechten.Count; i++)
            {
                if (gerechten[i].ID == ID)
                {
                    gerechten[i].is_gearchiveerd = true;
                    break;
                }
            }
            database.menukaart.gerechten = gerechten;
            io.Savedatabase(database);
        }

        public void EditMeal(int ID, string name, bool isPopular, double price, bool isSpecial, bool isArchived, List<string> ingredients, List<string> allergens, bool isDiner, bool isLunch, bool isOntbijt)
        {
            database = io.GetDatabase();
            List<Gerechten> gerechten = new List<Gerechten>(GetMeals());
            for (int i = 0; i < gerechten.Count; i++)
            {
                if (gerechten[i].ID == ID)
                {
                    gerechten[i].naam = name;
                    gerechten[i].is_populair = isPopular;
                    gerechten[i].prijs = price;
                    gerechten[i].special = isSpecial;
                    gerechten[i].is_gearchiveerd = isArchived;
                    gerechten[i].Ingredienten = ingredients;
                    gerechten[i].allergenen = allergens;
                    gerechten[i].diner = isDiner;
                    gerechten[i].lunch = isLunch;
                    gerechten[i].ontbijt = isOntbijt;
                    break;
                }
            }
            database.menukaart.gerechten = gerechten;
            io.Savedatabase(database);
        }

        public void CreateMeal(string name, bool isPopular, double price, bool isSpecial, bool isArchived, List<string> ingredients, List<string> allergens, bool isDiner, bool isLunch, bool isOntbijt)
        {
            database = io.GetDatabase();
            List<Gerechten> gerechten = new List<Gerechten>(GetMeals());

            Gerechten gerecht = new Gerechten();
            gerecht.ID = gerechten.Count;
            gerecht.naam = name;
            gerecht.is_populair = isPopular;
            gerecht.prijs = price;
            gerecht.special = isSpecial;
            gerecht.is_gearchiveerd = isArchived;
            gerecht.Ingredienten = ingredients;
            gerecht.allergenen = allergens;
            gerecht.diner = isDiner;
            gerecht.lunch = isLunch;
            gerecht.ontbijt = isOntbijt;
            gerechten.Add(gerecht);
            database.menukaart.gerechten = gerechten;
            io.Savedatabase(database);
        }

        public List<Gerechten> GetMeals()
        {
            database = io.GetDatabase();
            return database.menukaart.gerechten;
        }
        #endregion

        #region Ingredients
        public List<Ingredient> GetIngredients()
        {
            database = io.GetDatabase();
            return database.ingredienten;
        }

        public List<Ingredient> GetAlmostExpiredIngredients(int dag)
        {
            List<Ingredient> ingredients = new List<Ingredient>(GetIngredients());
            DateTime compareDate = DateTime.Now;
            compareDate = compareDate.AddDays(dag);
            for (int i = 0; i < ingredients.Count; i++)
            {
                if (DateTime.Now.AddDays(ingredients[i].dagenHoudbaar) > compareDate || DateTime.Now.AddDays(ingredients[i].dagenHoudbaar) < DateTime.Now)
                {
                    ingredients[i] = new Ingredient();
                }
            }
            ingredients.RemoveAll(x => x.Equals(new Ingredient()));
            return ingredients;
        }

        public List<Ingredient> GetExpiredIngredients()
        {
            List<Ingredient> ingredients = new List<Ingredient>(GetIngredients());
            for (int i = 0; i < ingredients.Count; i++)
            {
                if (DateTime.Now.AddDays(ingredients[i].dagenHoudbaar) > DateTime.Now)
                {
                    ingredients[i] = new Ingredient();
                }
            }
            ingredients.RemoveAll(x => x.Equals(new Ingredient()));
            return ingredients;
        }

        public void DeleteExpiredIngredients()
        {
            database = io.GetDatabase();
            for (int i = 0; i < database.ingredienten.Count; i++)
            {
                if (DateTime.Now.AddDays(database.ingredienten[i].dagenHoudbaar) <= DateTime.Now)
                {
                    database.ingredienten[i] = new Ingredient();
                }
            }
            database.ingredienten.RemoveAll(x => x.Equals(new Ingredient()));
            io.Savedatabase(database);
        }

        public void DeleteIngredientById(int ID)
        {
            database = io.GetDatabase();
            for (int i = 0; i < database.ingredienten.Count; i++)
            {
                if (database.ingredienten[i].ID == ID)
                {
                    database.ingredienten.RemoveAt(i);
                    break;
                }
            }
            io.Savedatabase(database);
        }

        #endregion

        #region Feedback
        public List<Feedback> GetFeedback()
        {
            database = io.GetDatabase();
            return database.feedback;
        }

        public List<Feedback> GetFeedback(int werknemerID)
        {
            List<Feedback> feedback = new List<Feedback>(GetFeedback());
            for (int i = 0; i < feedback.Count; i++)
            {
                if (feedback[i].recipient != werknemerID)
                {
                    feedback[i] = null;
                }
            }
            feedback.RemoveAll(x => x == null);
            return feedback;
        }

        public void DeleteFeedback(int feedbackID)
        {
            database = io.GetDatabase();
            List<Feedback> feedback = new List<Feedback>(GetFeedback());
            for (int i = 0; i < feedback.Count; i++)
            {
                if (feedback[i].ID == feedbackID)
                {
                    feedback[i] = null;
                    break;
                }
            }
            feedback.RemoveAll(x => x == null);
            database.feedback = feedback;
            io.Savedatabase(database);
        }
        #endregion

        #region Review
        public List<Review> GetReviews()
        {
            database = io.GetDatabase();
            return database.reviews;
        }

        public List<Review> GetReviews(int rating)
        {
            List<Review> reviews = new List<Review>(GetReviews());
            for (int i = 0; i < reviews.Count; i++)
            {
                if (reviews[i].Rating != rating)
                {
                    reviews[i] = null;
                }
            }
            reviews.RemoveAll(x => x == null);
            return reviews;
        }

        public List<Review> GetReviews(List<int> rating)
        {
            List<Review> reviews = new List<Review>(GetReviews());
            for (int i = 0; i < reviews.Count; i++)
            {
                for (int j = 0; j < rating.Count; j++)
                {
                    if (reviews[i].Rating != rating[j])
                    {
                        reviews[i] = null;
                        break;
                    }
                }
            }
            reviews.RemoveAll(x => x == null);
            return reviews;
        }

        public void DeleteReview(int reviewID)
        {
            database = io.GetDatabase();
            List<Review> reviews = new List<Review>(GetReviews());
            for (int i = 0; i < reviews.Count; i++)
            {
                if (reviews[i].ID == reviewID)
                {
                    reviews[i] = null;
                    break;
                }
            }
            reviews.RemoveAll(x => x == null);
            database.reviews = reviews;
            io.Savedatabase(database);
        }

        #endregion

        private bool IfDishExists(int id)
        {
            database = io.GetDatabase();
            for (int i = 0; i < database.menukaart.gerechten.Count; i++)
            {
                if (database.menukaart.gerechten[i].ID == id)
                {
                    return true;
                }
            }
            return false;
        }

        public void MakeDishPopular(int id)
        {
            database = io.GetDatabase();
            if (IfDishExists(id))
            {
                int dishIndex = database.menukaart.gerechten.FindIndex(x => x.ID == id);
                database.menukaart.gerechten[dishIndex] = new Gerechten
                {
                    naam = database.menukaart.gerechten[dishIndex].naam,
                    ID = database.menukaart.gerechten[dishIndex].ID,
                    Ingredienten = database.menukaart.gerechten[dishIndex].Ingredienten,
                    is_populair = true,
                    prijs = database.menukaart.gerechten[dishIndex].prijs,
                    special = database.menukaart.gerechten[dishIndex].special,
                    is_gearchiveerd = database.menukaart.gerechten[dishIndex].is_gearchiveerd,
                    allergenen = database.menukaart.gerechten[dishIndex].allergenen,
                    diner = database.menukaart.gerechten[dishIndex].diner,
                    lunch = database.menukaart.gerechten[dishIndex].lunch,
                    ontbijt = database.menukaart.gerechten[dishIndex].ontbijt,
                };
                io.Savedatabase(database);
            }
        }

        public void OverwriteMeal(Gerechten meal)
        {
            database = io.GetDatabase();
            if (IfDishExists(meal.ID))
            {
                int dishIndex = database.menukaart.gerechten.FindIndex(x => x.ID == meal.ID);
                database.menukaart.gerechten[dishIndex] = new Gerechten
                {
                    naam = meal.naam,
                    ID = meal.ID,
                    Ingredienten = meal.Ingredienten,
                    is_populair = meal.is_populair,
                    prijs = meal.prijs,
                    special = meal.special,
                    is_gearchiveerd = meal.is_gearchiveerd,
                    allergenen = meal.allergenen,
                    diner = meal.diner,
                    lunch = meal.lunch,
                    ontbijt = meal.ontbijt,
                };
                io.Savedatabase(database);
            }
        }

        public List<Tuple<Gerechten, int>> GetUserOrderInfo(DateTime beginDate, DateTime endDate)
        {
            database = io.GetDatabase();
            List<Tuple<Gerechten, int>> populaireGerechten = new List<Tuple<Gerechten, int>>();
            //hier komt een loop waarbij populaire gerechten gevuld wordt met alle bestaande gerechten.
            //als de reservering plaats heeft gevonden binnen de gestelde tijd.
            for (int i = 0; i < database.menukaart.gerechten.Count; i++)
            {
                populaireGerechten.Add(Tuple.Create(database.menukaart.gerechten[i], 0));
            }
            foreach (var reservering in database.reserveringen)
            {
                if (reservering.datum.Date >= beginDate.Date && reservering.datum.Date <= endDate.Date)
                {
                    foreach (var gerechtID in reservering.gerechten_ID)
                    {
                        for (int i = 0; i < populaireGerechten.Count; i++)
                        {
                            if (populaireGerechten[i].Item1.ID == gerechtID)
                            {
                                populaireGerechten[i] = Tuple.Create(populaireGerechten[i].Item1, populaireGerechten[i].Item2 + 1);
                                break;
                            }
                        }
                    }
                }
            }
            return populaireGerechten;
        }

        public List<List<string>> ReserveringenToString(List<Reserveringen> reserveringen)
        {
            reserveringen = reserveringen.OrderBy(o => o.datum).ToList();
            List<Klantgegevens> klantgegevens = io.GetCustomer(reserveringen.Select(i => i.klantnummer).ToList());
            List<List<string>> output = new List<List<string>>();
            for (int a = 0; a < reserveringen.Count; a++)
            {
                List<string> block = new List<string>();
                block.Add(new string(' ', 50));
                block.Add(new string(' ', 50));
                block.Add("Voornaam: " + klantgegevens[a].voornaam + new string(' ', 50 - ("Voornaam: " + klantgegevens[a].voornaam).Length));
                block.Add("Achternaam: " + klantgegevens[a].achternaam + new string(' ', 50 - ("Achternaam: " + klantgegevens[a].achternaam).Length));
                block.Add("Tijdstip: " + reserveringen[a].datum.ToShortTimeString() + new string(' ', 50 - ("Tijdstip: " + reserveringen[a].datum.ToShortTimeString()).Length));
                int[] tafels = new int[reserveringen[a].tafels.Count];
                for (int i = 0; i < tafels.Length; i++)
                {
                    tafels[i] = reserveringen[a].tafels[i].ID;
                }
                block.Add("Aantal gereserveerde tafels:" + new string(' ', 50 - ("Aantal gereserveerde tafels:").Length));
                if (tafels.Length < 1)
                {
                    block.Add("Er zijn nog geen tafels gekoppeld" + new string(' ', 50 - ("Er zijn nog geen tafels gekoppeld").Length));
                }
                else
                {
                    block.Add(string.Join(", ", tafels) + new string(' ', 50 - (string.Join(", ", tafels)).Length));
                }
                block.Add(new string(' ', 50));
                output.Add(block);
            }
            return output;
        }

        public List<List<string>> WorkersToString(List<Werknemer> werknemers)
        {
            List<List<string>> output = new List<List<string>>();
            for (int a = 0; a < werknemers.Count; a++)
            {
                List<string> block = new List<string>();
                block.Add(new string(' ', 60));
                block.Add(new string(' ', 60));
                block.Add("Voornaam: " + werknemers[a].login_gegevens.klantgegevens.voornaam + new string(' ', 60 - ("Voornaam: " + werknemers[a].login_gegevens.klantgegevens.voornaam).Length));
                block.Add("Achternaam: " + werknemers[a].login_gegevens.klantgegevens.achternaam + new string(' ', 60 - ("Achternaam: " + werknemers[a].login_gegevens.klantgegevens.achternaam).Length));
                block.Add("E-mailadres: " + werknemers[a].login_gegevens.email + new string(' ', 60 - ("E-mailadres: " + werknemers[a].login_gegevens.email).Length));
                block.Add(new string(' ', 60));
                output.Add(block);
            }
            return output;
        }


        public void DeleteIngredients(List<Ingredient> ingredients)
        {
            database = io.GetDatabase();
            database.ingredienten = database.ingredienten.Except(ingredients).ToList();
            io.Savedatabase(database);
        }

        public bool DeleteWorker(Werknemer werknemer)
        {
            Database database = io.GetDatabase();
            List<Werknemer> workers = database.werknemers;
            int index = workers.FindIndex(u => u.ID == werknemer.ID);
            if (index != -1)
            {
                workers.RemoveAt(index);
                io.Savedatabase(database);
                return true;
            }
            return false;
        }

        public void SaveIngredientName(IngredientType ingredientType)
        {
            database = io.GetDatabase();
            if (database.ingredientenNamen.Count == 0)
            {
                database.ingredientenNamen = new List<IngredientType> { ingredientType };
            }
            else
            {
                database.ingredientenNamen.Add(ingredientType);
            }
            io.Savedatabase(database);
        }
    }

    public class GetReservationsScreen : Screen
    {

        public override int DoWork()
        {
            int maxLength = 104;
            double pos = 0;
            List<string> pages = new List<string>();

            string date = DateTime.Now.ToShortDateString();
            Dictionary<string, List<Reserveringen>> reserveringen = new Dictionary<string, List<Reserveringen>>();
            Dictionary<string, List<Reserveringen>> reserveringenWithoutTables = new Dictionary<string, List<Reserveringen>>();
            //Add todays reservations to the reservations dictionary
            List<Reserveringen> reserveringenList = io.GetDatabase().reserveringen;
            List<string> dates = reserveringenList.Select(d => d.datum.ToShortDateString()).Distinct().ToList();

            for (int i = 0; i < dates.Count; i++)
            {
                reserveringen.Add(dates[i], reserveringenList.Where(d => d.datum.ToShortDateString() == dates[i]).OrderBy(x => x.datum).ToList());
                reserveringenWithoutTables.Add(dates[i], reserveringenList.Where(t => t.tafels.Count == 0 && t.datum.ToShortDateString() == dates[i]).OrderBy(x => x.datum).ToList());

            }
            //var pages = MakePages(boxText, 3);
            int pageNum = 0;
            bool onlyWithoutTables = false;
            do
            {
            a:
                (int, int, double) result = (0, 0, 0.0);
                pages = new List<string>();
                List<List<string>> reservationString = new List<List<string>>();
                if (onlyWithoutTables)
                {
                    if (reserveringen.ContainsKey(date))
                    {
                        reservationString = Makedubbelboxes(code_eigenaar.ReserveringenToString(reserveringenWithoutTables[date]));
                    }
                }
                else
                {
                    if (reserveringen.ContainsKey(date))
                    {
                        reservationString = Makedubbelboxes(code_eigenaar.ReserveringenToString(reserveringen[date]));
                    }
                }
                if (reservationString.Count == 0)
                {
                    Console.Clear();
                    Console.WriteLine(GetGFLogo(true));
                    Console.WriteLine("Geen reserveringen gevonden op " + date);
                    result = Nextpage(pageNum, pos, pageNum, 16,
                        new List<Tuple<(int, int, double), string>> {
                                Tuple.Create((- 2, -2, pos), "D3"),
                                Tuple.Create((- 3, -3, pos), "D4"),
                                Tuple.Create((- 4, -4, pos), "D5"),
                        },
                        new List<string> { "[3] Vorige dag             [4] Volgende dag             [5] Naar vandaag" });
                    if (result.Item2 == 0)
                    {
                        LogoutWithMessage();
                        return 0;
                    }
                    if (result.Item2 > -1)
                    {
                        return result.Item2;
                    }
                    switch (result.Item1)
                    {
                        case -2:
                            date = DateTime.Parse(date).AddDays(-1).ToShortDateString();
                            break;
                        case -3:
                            date = DateTime.Parse(date).AddDays(1).ToShortDateString();
                            break;
                        case -4:
                            //go to today
                            date = DateTime.Now.ToShortDateString();
                            break;
                    }
                    goto a;
                }
                List<string> boxes = new List<string>();
                for (int i = 0; i < reservationString.Count; i++)
                {
                    if (i == reservationString.Count - 1 && reservationString[i][1].Length < 70)
                    {
                        if (i == Convert.ToInt32(Math.Floor(pos / 2)))
                        {
                            if (i != 0 && i % 6 != 0)
                            {
                                boxes.Add(BoxAroundText(reservationString[i], "#", 2, 0, 104, true, new List<string>{
                                    "[7] Tafels koppelen" + new string(' ', 50 - "[7] Tafels koppelen".Length),
                                    new string(' ', 50)}));
                            }
                            else
                            {
                                boxes.Add(BoxAroundText(reservationString[i], "#", 2, 0, 50, true, new List<string>{
                                    "[7] Tafels koppelen" + new string(' ', 50 - "[7] Tafels koppelen".Length),
                                    new string(' ', 50)}));
                            }
                        }
                        else
                        {
                            if (i != 0 && i % 6 != 0)
                            {
                                boxes.Add(BoxAroundText(reservationString[i], "#", 2, 0, 104, true));
                            }
                            else
                            {
                                boxes.Add(BoxAroundText(reservationString[i], "#", 2, 0, 50, true));
                            }
                        }
                    }
                    else
                    {
                        if (i == Convert.ToInt32(Math.Floor(pos / 2)))
                        {
                            if (pos % 2 == 0 || pos == 0)
                            {
                                boxes.Add(BoxAroundText(reservationString[i], "#", 2, 0, 104, true, new List<string>{
                                    "[7] Tafels koppelen" + new string(' ', 50 - "[7] Tafels koppelen".Length) + "##  " + new string(' ', 50),
                                    new string(' ', 50) + "##  " + new string(' ', 50) }));
                            }
                            else
                            {
                                boxes.Add(BoxAroundText(reservationString[i], "#", 2, 0, 104, true, new List<string> {
                                    new string(' ', 50) + "##  " + "[7] Tafels koppelen" + new string(' ', 50 - "[7] Tafels koppelen".Length),
                                    new string(' ', 50) + "##  " + new string(' ', 50)}));
                            }
                        }
                        else
                        {
                            boxes.Add(BoxAroundText(reservationString[i], "#", 2, 0, 104, true));
                        }
                    }
                }
                int oneven = 0;
                pages = MakePages(boxes, 3);
                Console.Clear();
                Console.WriteLine(GetGFLogo(true));

                Console.WriteLine();
                Console.WriteLine("Reserveringen van: " + date + (date == DateTime.Now.ToShortDateString() ? " (vandaag)" : ""));
                Console.WriteLine($"Pagina {pageNum + 1} van de {pages.Count}:");
                if (onlyWithoutTables)
                {
                    if (reserveringenWithoutTables[date].Count > 1)
                    {
                        Console.WriteLine("Gebruik de pijltjestoetsen om te navigeren door de reserveringen.\nDe reservering met de tekst 'Tafels koppelen' is de huidig geselecteerde reservering.");
                    }
                }
                else
                {
                    if (reserveringen[date].Count > 1)
                    {
                        Console.WriteLine("Gebruik de pijltjestoetsen om te navigeren door de reserveringen.\nDe reservering met de tekst 'Tafels koppelen' is de huidig geselecteerde reservering.");
                    }
                }
                if (reservationString[reservationString.Count - 1][1].Length < 70 && pageNum == pages.Count - 1)
                {
                    Console.WriteLine(pages[pageNum] + new string('#', 56));
                    oneven = 1;
                }
                else
                {
                    Console.WriteLine(pages[pageNum] + new string('#', maxLength + 6));
                }
                List<Tuple<(int, int, double), string>> tuples = new List<Tuple<(int, int, double), string>>();
                //if there is a next page and a previous page
                List<string> txt = new List<string>();
                if (pages.Count > 0 && pageNum > 0 && pageNum < pages.Count - 1)
                {
                    txt.Add("[1] Volgende pagina                                     [2] Vorige pagina");
                    tuples.Add(Tuple.Create((pageNum + 1, -1, (pageNum + 1) * 6.0), "D1"));
                    tuples.Add(Tuple.Create((pageNum - 1, -1, (pageNum - 1) * 6.0), "D2"));
                }
                //if there is only a next page
                else if (pages.Count - 1 > 0 && pageNum < pages.Count - 1)
                {
                    txt.Add("[1] Volgende pagina");
                    tuples.Add(Tuple.Create((pageNum + 1, -1, (pageNum + 1) * 6.0), "D1"));
                }
                //if there is only a previous page
                else if (pages.Count - 1 > 0 && pageNum >= pages.Count - 1)
                {
                    txt.Add("                                                        [2] Vorige pagina");
                    tuples.Add(Tuple.Create((pageNum - 1, -1, (pageNum - 1) * 6.0), "D2"));
                }
                //if date is smaller than now
                tuples.Add(Tuple.Create((-2, -2, pos), "D3"));
                tuples.Add(Tuple.Create((-3, -3, pos), "D4"));
                tuples.Add(Tuple.Create((-4, -4, pos), "D5"));
                tuples.Add(Tuple.Create((-7, -7, pos), "D7"));
                txt.Add("[3] Vorige dag             [4] Volgende dag             [5] Naar vandaag");
                if (DateTime.Parse(date) >= DateTime.Parse(DateTime.Now.ToShortDateString()))
                {
                    txt.Add(onlyWithoutTables ? "[6] Toon alle reserveringen van deze dag      " : "[6] Toon alleen de reserveringen zonder tafel");
                    tuples.Add(Tuple.Create((-5, -5, pos), "D6"));
                }
                result = Nextpage(pageNum, pos, boxes.Count * 2 - (1 + oneven), 16, tuples, txt);
                if (result.Item2 > -1)
                {
                    return result.Item2;
                }
                string newDate = DateTime.Now.ToShortDateString();

                switch (result.Item1)
                {
                    case -2:
                        date = DateTime.Parse(date).AddDays(-1).ToShortDateString();
                        break;
                    case -3:
                        date = DateTime.Parse(date).AddDays(1).ToShortDateString();
                        break;
                    case -4:
                        //go to today
                        date = DateTime.Now.ToShortDateString();
                        break;
                    case -5:
                        onlyWithoutTables = !onlyWithoutTables;
                        break;
                    case -7:
                        AddTableToReservationScreen addTable = new AddTableToReservationScreen();
                        if (onlyWithoutTables)
                        {
                            addTable = new AddTableToReservationScreen(reserveringenWithoutTables[date][Convert.ToInt32(pos)]);
                        }
                        else
                        {
                            addTable = new AddTableToReservationScreen(reserveringen[date][Convert.ToInt32(pos)]);
                        }
                        addTable.DoWork();
                        return 17;
                }
                pageNum = 0;
                pos = 0;
                if (result.Item1 >= 0)
                {
                    pos = result.Item3;
                    pageNum = result.Item1;
                }

            } while (true);
        }
        public override List<Screen> Update(List<Screen> screens)
        {
            DoLogoutOnEveryScreen(screens);
            return screens;
        }
    }

    public class GetWorkersScreen : Screen
    {
        int currentScreenID = 24;
        public override int DoWork()
        {
            int maxLength = 124;
            double pos = 0;
            List<string> pages = new List<string>();
            List<Werknemer> workers = io.GetEmployee().OrderBy(x => x.login_gegevens.klantgegevens.voornaam).ToList();
            int pageNum = 0;
            do
            {
                (int, int, double) result = (0, 0, 0.0);
                pages = new List<string>();
                List<List<string>> workerString = Makedubbelboxes(code_eigenaar.WorkersToString(workers));
                if (workerString.Count == 0)
                {
                    Console.Clear();
                    Console.WriteLine(GetGFLogo(true));
                    Console.WriteLine("Geen medewerkers gevonden.");
                    (string, int) ans = AskForInput(11);
                    switch (ans.Item1)
                    {
                        case "0":
                            LogoutWithMessage();
                            return 0;
                        default:
                            Console.WriteLine(PressButtonToContinueMessage);
                            Console.ReadKey();
                            return 11;
                    }
                }
                List<string> boxes = new List<string>();
                for (int i = 0; i < workerString.Count; i++)
                {
                    if (i == workerString.Count - 1 && workerString[i][1].Length < 70)
                    {
                        if (i == Convert.ToInt32(Math.Floor(pos / 2)))
                        {
                            if (i != 0 && i % 6 != 0)
                            {
                                boxes.Add(BoxAroundText(workerString[i], "#", 2, 0, 124, true, new List<string>{
                                        "[3] Medewerker verwijderen" + new string(' ', 60 - "[3] Medewerker verwijderen".Length),
                                        new string(' ', 60)}));
                            }
                            else
                            {
                                boxes.Add(BoxAroundText(workerString[i], "#", 2, 0, 60, true, new List<string>{
                                        "[3] Medewerker verwijderen" + new string(' ', 60 - "[3] Medewerker verwijderen".Length),
                                        new string(' ', 60)}));
                            }
                        }
                        else
                        {
                            if (i != 0 && i % 6 != 0)
                            {
                                boxes.Add(BoxAroundText(workerString[i], "#", 2, 0, 124, true));
                            }
                            else
                            {
                                boxes.Add(BoxAroundText(workerString[i], "#", 2, 0, 60, true));
                            }
                        }
                    }
                    else
                    {
                        if (i == Convert.ToInt32(Math.Floor(pos / 2)))
                        {
                            if (pos % 2 == 0 || pos == 0)
                            {
                                boxes.Add(BoxAroundText(workerString[i], "#", 2, 0, 124, true, new List<string>{
                                        "[3] Medewerker verwijderen" + new string(' ', 60 - "[3] Medewerker verwijderen".Length) + "##  " + new string(' ', 60),
                                        new string(' ', 60) + "##  " + new string(' ', 60) }));
                            }
                            else
                            {
                                boxes.Add(BoxAroundText(workerString[i], "#", 2, 0, 124, true, new List<string> {
                                        new string(' ', 60) + "##  " + "[3] Medewerker verwijderen" + new string(' ', 60 - "[3] Medewerker verwijderen".Length),
                                        new string(' ', 60) + "##  " + new string(' ', 60)}));
                            }
                        }
                        else
                        {
                            boxes.Add(BoxAroundText(workerString[i], "#", 2, 0, 124, true));
                        }
                    }
                }
                pages = MakePages(boxes, 3);

                Console.Clear();
                Console.WriteLine(GetGFLogo(true));


                Console.WriteLine();
                Console.WriteLine("Werknemerslijst");
                Console.WriteLine($"Pagina {pageNum + 1} van de {pages.Count}:");
                int uneven = 0;
                if (workerString[workerString.Count - 1][1].Length < 70 && pageNum == pages.Count - 1)
                {
                    Console.WriteLine(pages[pageNum] + new string('#', 66));
                    uneven = 1;
                }
                else
                {
                    Console.WriteLine(pages[pageNum] + new string('#', maxLength + 6));
                }
                List<Tuple<(int, int, double), string>> tuples = new List<Tuple<(int, int, double), string>>();
                //if there is a next page and a previous page
                List<string> txt = new List<string>();
                if (pages.Count > 0 && pageNum > 0 && pageNum < pages.Count - 1)
                {
                    txt.Add("[1] Volgende pagina                                               [2] Vorige pagina");
                    tuples.Add(Tuple.Create((pageNum + 1, -1, (pageNum + 1) * 6.0), "D1"));
                    tuples.Add(Tuple.Create((pageNum - 1, -1, (pageNum - 1) * 6.0), "D2"));
                }
                //if there is only a next page
                else if (pages.Count - 1 > 0 && pageNum < pages.Count - 1)
                {
                    txt.Add("[1] Volgende pagina");
                    tuples.Add(Tuple.Create((pageNum + 1, -1, (pageNum + 1) * 6.0), "D1"));
                }
                //if there is only a previous page
                else if (pages.Count - 1 > 0 && pageNum >= pages.Count - 1)
                {
                    txt.Add("[2] Vorige pagina");
                    tuples.Add(Tuple.Create((pageNum - 1, -1, (pageNum - 1) * 6.0), "D2"));
                }
                tuples.Add(Tuple.Create((-3, -3, pos), "D3"));
                result = Nextpage(pageNum, pos, boxes.Count * 2 - (1 + uneven), 11, tuples, txt);
                if (result.Item2 > -1)
                {
                    return result.Item2;
                }
                string newDate = DateTime.Now.ToShortDateString();

                switch (result.Item1)
                {
                    case -3:
                        //Verwijderen
                        Console.Clear();
                        Code_Eigenaar_menu eigenaar_Menu = new Code_Eigenaar_menu();

                        Werknemer worker = workers[Convert.ToInt32(pos)];
                        Console.WriteLine(GetGFLogo(true));
                        Console.WriteLine(new string('#', 50));
                        Console.WriteLine("#" + new string(' ', 48) + "#");
                        Console.WriteLine("#" + new string(' ', 48) + "#");
                        Console.WriteLine("#  Voornaam: " + worker.login_gegevens.klantgegevens.voornaam + new string(' ', 36 - worker.login_gegevens.klantgegevens.voornaam.Length) + "#");
                        Console.WriteLine("#  Achternaam: " + worker.login_gegevens.klantgegevens.achternaam + new string(' ', 34 - worker.login_gegevens.klantgegevens.achternaam.ToString().Length) + "#");
                        Console.WriteLine("#  E-mailadres: " + worker.login_gegevens.email + new string(' ', 33 - worker.login_gegevens.email.Length) + "#");
                        Console.WriteLine("#" + new string(' ', 48) + "#");
                        Console.WriteLine("#" + new string(' ', 48) + "#");
                        Console.WriteLine(new string('#', 50) + "\n");
                        Console.WriteLine("Weet u zeker dat u deze medewerker wilt verwijderen? Type in ja of nee");
                        return Confirmation(
                            currentScreenID,
                            () =>
                            {
                                bool deleted = eigenaar_Menu.DeleteWorker(workers[Convert.ToInt32(pos)]);
                                if (deleted)
                                {
                                    Console.WriteLine("\nDe medewerker is succesvol verwijderd!");
                                }
                                else
                                {
                                    Console.WriteLine("\nEr is iets fout gegaan probeer het later opnieuw.");
                                }
                                Console.WriteLine(PressButtonToContinueMessage);
                                Console.ReadKey();
                                return currentScreenID;
                            },
                            () =>
                            {
                                Console.WriteLine("\nDe medewerker is NIET verwijderd.");
                                Console.WriteLine(PressButtonToContinueMessage);
                                Console.ReadKey();
                                return currentScreenID;
                            }
                        );
                }
                pageNum = 0;
                pos = 0;
                if (result.Item1 >= 0)
                {
                    pos = result.Item3;
                    pageNum = result.Item1;
                }
            } while (true);
        }

        public override List<Screen> Update(List<Screen> screens)
        {
            DoLogoutOnEveryScreen(screens);
            return screens;
        }
    }
    public class AddWorkerScreen : StepScreen
    {
        private Login_gegevens lg;

        public AddWorkerScreen(Werknemer worker = null)
        {
            if (worker != null)
            {
                lg.email = worker.login_gegevens.email;
                lg.klantgegevens = worker.login_gegevens.klantgegevens;
                lg.password = worker.login_gegevens.password;
                lg.type = worker.login_gegevens.type;
            }
            steps.Add("De voornaam: ");
            steps.Add("De achternaam: ");
            steps.Add("De geboortedatum genoteerd als dag-maand-jaar: ");
            steps.Add("Hieronder vult u het adres in.\nDe woonplaats: ");
            steps.Add("De postcode: ");
            steps.Add("De straatnaam: ");
            steps.Add("Het huisnummer: ");
            steps.Add("Hieronder vult u de logingegevens in: \nHet e-mailadres: ");
            steps.Add("Het wachtwoord voor het account: ");
            steps.Add("Kloppen de bovenstaande gegevens?\n[1] Deze kloppen niet, breng me terug.\n[2] Ja, deze kloppen.");

            output.Add(GetGFLogo(true));
            output.Add("Hier kunt u een account aanmaken voor een medewerker!");

            List<Werknemer> werknemers = io.GetDatabase().werknemers;

            if (werknemers == null)
            {
                werknemers = new List<Werknemer>();
                io.GetDatabase().werknemers = werknemers;
            }

            lg = new Login_gegevens()
            {
                type = "Medewerker",
                klantgegevens = new Klantgegevens()
                {
                    klantnummer = werknemers.Count == 0 ? 0 : werknemers[werknemers.Count - 1].login_gegevens.klantgegevens.klantnummer + 1,
                    adres = new adres()
                    {
                        land = "NL",
                    }
                }
            };
        }

        private void ResetOutput()
        {
            Reset();
            output.Add(GetGFLogo(true));
            output.Add("Hier kunt u een account aanmaken voor een medewerker!");
        }

        public override int DoWork()
        {
            int currentScreen = 23;
            int previousScreen = 11;
            (string, int, string) result;

            Console.WriteLine(string.Join("\n", output));

            switch (currentStep)
            {
                case 0:
                    Console.WriteLine(steps[currentStep]);
                    result = AskForInput(previousScreen, c => char.IsLetter(c), null, (LettersOnlyMessage, null));

                    if (result.Item1 == null)
                    {
                        ResetOutput();
                        return result.Item2;
                    }

                    if (result.Item3 != null) return ShowInvalidInput(currentScreen, result.Item3);

                    output.Add($"{steps[currentStep]}\n{result.Item1}");

                    lg.klantgegevens.voornaam = result.Item1;

                    currentStep++;
                    return currentScreen;
                case 1:
                    Console.WriteLine(steps[currentStep]);
                    result = AskForInput(previousScreen, c => char.IsLetter(c), null, (LettersOnlyMessage, null));

                    if (result.Item1 == null)
                    {
                        ResetOutput();
                        return result.Item2;
                    }

                    if (result.Item3 != null) return ShowInvalidInput(currentScreen, result.Item3);

                    output.Add($"{steps[currentStep]}\n{result.Item1}");

                    lg.klantgegevens.achternaam = result.Item1;

                    currentStep++;
                    return currentScreen;
                case 2:
                    Console.WriteLine(steps[currentStep]);
                    DateTime resultDateTime = new DateTime();

                    result = AskForInput(
                        previousScreen,
                        c => char.IsDigit(c) || c == '/' || c == '-',
                        input => DateTime.TryParseExact(input, new string[2] { "dd/MM/yyyy", "d/M/yyyy" }, new CultureInfo("nl-NL"), DateTimeStyles.None, out resultDateTime) && resultDateTime < DateTime.Now,
                        ("Het formaat van de datum die u heeft ingevoerd klopt niet. Probeer het opnieuw.", "De datum die u hebt ingevoerd klopt niet. Probeer het opnieuw.")
                    );

                    if (result.Item1 == null)
                    {
                        ResetOutput();
                        return result.Item2;
                    }

                    if (result.Item3 != null) return ShowInvalidInput(currentScreen, result.Item3);

                    output.Add($"{steps[currentStep]}\n{result.Item1}");

                    lg.klantgegevens.geb_datum = resultDateTime;

                    currentStep++;
                    return currentScreen;
                case 3:
                    Console.WriteLine(steps[currentStep]);
                    result = AskForInput(previousScreen, c => char.IsLetter(c), null, (LettersOnlyMessage, null));

                    if (result.Item1 == null)
                    {
                        ResetOutput();
                        return result.Item2;
                    }

                    if (result.Item3 != null) return ShowInvalidInput(currentScreen, result.Item3);

                    output.Add($"{steps[currentStep]}\n{result.Item1}");

                    lg.klantgegevens.adres.woonplaats = result.Item1;

                    currentStep++;
                    return currentScreen;
                case 4:
                    Console.WriteLine(steps[currentStep]);
                    result = AskForInput(previousScreen, c => char.IsLetterOrDigit(c), null, (DigitsAndLettersOnlyMessage, null));

                    if (result.Item1 == null)
                    {
                        ResetOutput();
                        return result.Item2;
                    }

                    if (result.Item3 != null) return ShowInvalidInput(currentScreen, result.Item3);

                    output.Add($"{steps[currentStep]}\n{result.Item1}");

                    lg.klantgegevens.adres.postcode = result.Item1;

                    currentStep++;
                    return currentScreen;
                case 5:
                    Console.WriteLine(steps[currentStep]);
                    result = AskForInput(previousScreen, c => char.IsLetter(c), null, (LettersOnlyMessage, null));

                    if (result.Item1 == null)
                    {
                        ResetOutput();
                        return result.Item2;
                    }

                    if (result.Item3 != null) return ShowInvalidInput(currentScreen, result.Item3);

                    output.Add($"{steps[currentStep]}\n{result.Item1}");

                    lg.klantgegevens.adres.straatnaam = result.Item1;

                    currentStep++;
                    return currentScreen;
                case 6:
                    Console.WriteLine(steps[currentStep]);
                    int possibleValue = -1;
                    result = AskForInput(previousScreen, c => char.IsDigit(c), input => int.TryParse(input, out possibleValue) && possibleValue > 0, (DigitsOnlyMessage, "Het nummer dat u heeft ingevoerd is te lang voor een gemiddeld huisnummer of het is 0"));

                    if (result.Item1 == null)
                    {
                        ResetOutput();
                        return result.Item2;
                    }

                    if (result.Item3 != null) return ShowInvalidInput(currentScreen, result.Item3);

                    output.Add($"{steps[currentStep]}\n{result.Item1}");

                    lg.klantgegevens.adres.huisnummer = possibleValue;

                    currentStep++;
                    return currentScreen;
                case 7:
                    Console.WriteLine(steps[currentStep]);
                    Regex regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

                    result = AskForInput(previousScreen, null, input => regex.IsMatch(input), (null, "Het e-mailadres is niet juist er mist een @ of een ."));

                    if (result.Item1 == null)
                    {
                        ResetOutput();
                        return result.Item2;
                    }

                    if (result.Item3 != null) return ShowInvalidInput(currentScreen, result.Item3);

                    output.Add($"{steps[currentStep]}\n{result.Item1}");

                    lg.email = result.Item1;

                    if (RetryStep)
                    {
                        currentStep = 9;
                        RetryStep = false;
                        return currentScreen;
                    }

                    currentStep++;
                    return currentScreen;
                case 8:
                    Console.WriteLine(steps[currentStep]);
                    (string, int) otherResult = AskForInput(previousScreen);

                    if (otherResult.Item1 == null)
                    {
                        ResetOutput();
                        return otherResult.Item2;
                    }

                    output.Add($"{steps[currentStep]}\n{otherResult.Item1}");

                    lg.password = otherResult.Item1;

                    if (RetryStep)
                    {
                        currentStep = 9;
                        RetryStep = false;
                        return currentScreen;
                    }

                    currentStep++;
                    return currentScreen;
                case 9:
                    Console.WriteLine(steps[currentStep]);
                    result = AskForInput(previousScreen, c => char.IsDigit(c), input => Convert.ToInt32(input) == 1 || Convert.ToInt32(input) == 2, (DigitsOnlyMessage, InvalidInputMessage));

                    if (result.Item1 == null)
                    {
                        ResetOutput();
                        return result.Item2;
                    }

                    if (result.Item3 != null) return ShowInvalidInput(currentScreen, result.Item3);

                    if (result.Item1 == "1")
                    {
                        ResetOutput();
                        return currentScreen;
                    }

                    if (code_login.Register(lg) == "Succes!")
                    {
                        Console.WriteLine("\nDe medewerker is succesvol aangemeld!");
                        Console.WriteLine("Druk op een knop om terug naar het eigenaarsmenu te gaan.");
                        Console.ReadKey();
                        ResetOutput();
                        return previousScreen;
                    }
                    else if (code_login.Register(lg) == "This email and account type is already in use")
                    {
                        Console.WriteLine("\nDit account bestaat al, druk op een knop om een ander e-mailadres in te voeren.");
                        Console.ReadKey();
                        currentStep = 7;
                        RetryStep = true;
                        return currentScreen;
                    }
                    else if (code_login.Register(lg) == "Password must contain at least 8 characters, 1 punctuation mark and 1 number.")
                    {
                        Console.WriteLine("\nHet wachtwoord moet minimaal 8 tekens, 1 leesteken en 1 nummer bevatten. Druk op een knop om een ander wachtwoord in te voeren.");
                        Console.ReadKey();
                        currentStep = 8;
                        RetryStep = true;
                        return currentScreen;
                    }
                    break;
            }

            return previousScreen;
        }

        public override List<Screen> Update(List<Screen> screens)
        {
            DoLogoutOnEveryScreen(screens);
            return screens;
        }
    }
}