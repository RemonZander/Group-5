using System;
using System.Collections.Generic;
using System.Linq;
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

        public void EditMeal(int ID, string name, bool isPopular, double price, bool isSpecial, bool isArchived, List<int> ingredients, List<string> allergens, bool isDiner, bool isLunch, bool isOntbijt)
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
                    gerechten[i].ingredienten = ingredients;
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

        public void CreateMeal(string name, bool isPopular, double price, bool isSpecial, bool isArchived, List<int> ingredients, List<string> allergens, bool isDiner, bool isLunch, bool isOntbijt)
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
            gerecht.ingredienten = ingredients;
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
            compareDate = compareDate.AddDays(-dag);
            for (int i = 0; i < ingredients.Count; i++)
            {
                if (ingredients[i].houdbaarheids_datum < compareDate)
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
                if (ingredients[i].houdbaarheids_datum > DateTime.Now)
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
                if (database.ingredienten[i].houdbaarheids_datum <= DateTime.Now)
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
                    ingredienten = database.menukaart.gerechten[dishIndex].ingredienten,
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

        public List<Tuple<Gerechten, int>> GetUserOrderInfo(DateTime beginDate, DateTime endDate)
        {
            database = io.GetDatabase();
            List<Tuple<Gerechten, int>> populaireGerechten = new List<Tuple<Gerechten, int>>();
            //hier komt een loop waarbij populaire gerechten gevuld wordt met alle bestaande gerechten.
            //als de reservering plaats heeft gevonden binnen de gestelde tijd.
            for (int i = 0; i < 6; i++)
            {
                populaireGerechten.Add(Tuple.Create(instance.Get_standard_dishes()[i], 0));

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
            List<Klantgegevens> klantgegevens = io.GetCustomer(reserveringen.Select(i => i.klantnummer).ToList());
            List<List<string>> output = new List<List<string>>();
            for (int a = 0; a < reserveringen.Count; a++)
            {
                List<string> block = new List<string>();
                block.Add(new string(' ', 50));
                block.Add(new string(' ', 50));

                block.Add("Voornaam: " + klantgegevens[a].voornaam + new string(' ', 50 - ("Voornaam: " + klantgegevens[a].voornaam).Length));
                block.Add("Achternaam: " + klantgegevens[a].achternaam + new string(' ', 50 - ("Achternaam: " + klantgegevens[a].achternaam).Length));

                int[] tafels = new int[reserveringen[a].tafels.Count];
                for (int i = 0; i < tafels.Length; i++)
                {
                    tafels[i] = reserveringen[a].tafels[i].ID;
                }
                block.Add("Gereserveerde Tafels" + new string(' ', 50 - ("Gereserveerde Tafels").Length));
                if (tafels.Length < 1) 
                { 
                    block.Add("Nog geen tafels gekoppeld" + new string(' ', 50 - ("Nog geen tafels gekoppeld").Length)); 
                }
                block.Add(string.Join(", ", tafels) + new string(' ', 50 - (string.Join(", ", tafels)).Length));
                block.Add(new string(' ', 50));
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
                reserveringen.Add(dates[i], reserveringenList.Where(d => d.datum.ToShortDateString() == dates[i]).ToList());
                reserveringenWithoutTables.Add(dates[i], reserveringenList.Where(t => t.tafels.Count == 0 && t.datum.ToShortDateString() == dates[i]).ToList());

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
                        new List<string> { "[3] Volgende dag             [4] Vorige dag             [5] Naar vandaag" });
                    if (result.Item2 > -1)
                    {
                        return result.Item2;
                    }
                    switch (result.Item1)
                    {
                        case -2:
                            date = DateTime.Parse(date).AddDays(1).ToShortDateString();
                            break;
                        case -3:
                            date = DateTime.Parse(date).AddDays(-1).ToShortDateString();
                            break;
                        case -4:
                            //go to today
                            date = DateTime.Now.ToShortDateString();
                            break;
                    }
                    //Console.Clear();
                    goto a;
                }
                List<string> boxes = new List<string>();
                for (int i = 0; i < reservationString.Count; i++)
                {
                    if(i == reservationString.Count - 1 && reservationString[i][1].Length < 70)
                    {
                        if (i == Convert.ToInt32(Math.Floor(pos/2)))
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
                pages = MakePages(boxes, 3);
                Console.Clear();
                Console.WriteLine(GetGFLogo(true));


                Console.WriteLine();
                Console.WriteLine("Reserveringen van: " + date + (date == DateTime.Now.ToShortDateString() ? " (vandaag)" : ""));
                Console.WriteLine($"Pagina {pageNum + 1} van de {pages.Count}:");
                Console.WriteLine(pages[pageNum] + new string('#', maxLength + 6));
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
                else if (pages.Count -1 > 0 && pageNum < pages.Count - 1)
                {
                    txt.Add("[1] Volgende pagina");
                    tuples.Add(Tuple.Create((pageNum + 1, -1, (pageNum + 1) * 6.0), "D1"));
                }
                //if there is only a previous page
                else if (pages.Count -1 > 0 && pageNum >= pages.Count - 1)
                {
                    txt.Add("                                                        [2] Vorige pagina");
                    tuples.Add(Tuple.Create((pageNum - 1, -1, (pageNum - 1) * 6.0), "D2"));
                }
                //if date is smaller than now
                if (DateTime.Parse(date) >= DateTime.Parse(DateTime.Now.ToShortDateString()))
                {
                    txt.Add(onlyWithoutTables ? "[6] Toon alle reserveringen van deze dag      " : "[6] Toon alleen de reserveringen zonder tafel");
                    tuples.Add(Tuple.Create((-5, -5, pos), "D6"));
                }
                tuples.Add(Tuple.Create((-2, -2, pos), "D3"));
                tuples.Add(Tuple.Create((-3, -3, pos), "D4"));
                tuples.Add(Tuple.Create((-4, -4, pos), "D5"));
                tuples.Add(Tuple.Create((-7, -7, pos), "D7"));
                txt.Add("[3] Volgende dag             [4] Vorige dag             [5] Naar vandaag");
                        
                result = Nextpage(pageNum, pos, (boxes.Count - 1) * 2, 16, tuples, txt);
                if (result.Item2 > -1)
                {
                    return result.Item2;
                }
                string newDate = DateTime.Now.ToShortDateString();

                switch (result.Item1)
                {
                    case -2:
                        date = DateTime.Parse(date).AddDays(1).ToShortDateString();
                        break;
                    case -3:
                        date = DateTime.Parse(date).AddDays(-1).ToShortDateString();
                        break;
                    case -4:
                        //go to today
                        date = DateTime.Now.ToShortDateString();
                        break;
                    case -5:
                        onlyWithoutTables = !onlyWithoutTables;
                        break;
                    case -7:
                        return 18;
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
            return screens;
        }
    }
}