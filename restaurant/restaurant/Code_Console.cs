using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;

namespace restaurant
{
    class Code_Console
    {
        private int currentScreen;
        private int lastscreen;
        private List<Screen> screens = new List<Screen>();

        public Code_Console()
        {
            screens.Add(new StartScreen());
            screens.Add(new GerechtenScreen());
            screens.Add(new ReviewScreen());
            screens.Add(new RegisterScreen());
            screens.Add(new LoginScreen());
            #region Klant
            screens.Add(new ClientMenuScreen());
            screens.Add(new MakeReservationScreen());
            screens.Add(new MakeReviewScreen());
            screens.Add(new EditReviewScreen());
            screens.Add(new DeleteReview());
            screens.Add(new ViewReviewScreen());
            #endregion
            #region Eigenaar
            screens.Add(new OwnerMenuScreen());
            screens.Add(new OwnerMenuScreen());
            screens.Add(new OwnerMenuScreen());
            screens.Add(new OwnerMenuScreen());
            screens.Add(new OwnerMenuScreen());
            #endregion
            currentScreen = 0;
        }

        public void Display()
        {
            lastscreen = currentScreen;
            currentScreen = screens[currentScreen].DoWork();
            screens = screens[lastscreen].Update(screens);
        }

        public void Refresh()
        {
            Console.Clear();
        }
    }


    public abstract class Screen
    {
        protected readonly Code_Medewerker_menu code_medewerker = new Code_Medewerker_menu();
        protected readonly Code_Gebruiker_menu code_gebruiker = new Code_Gebruiker_menu();
        protected readonly Code_Eigenaar_menu code_eigenaar = new Code_Eigenaar_menu();
        protected readonly Code_Login_menu code_login = new Code_Login_menu();
        protected readonly IO io = new IO();
        protected readonly Testing_class testing_class = new Testing_class();
        public Login_gegevens ingelogd = new Login_gegevens();
        protected bool logoutUpdate = false;

        protected const string GFLogo = @" _____                     _  ______         _         
|  __ \                   | | |  ___|       (_)            
| |  \/_ __ __ _ _ __   __| | | |_ _   _ ___ _  ___  _ __  
| | __| '__/ _` | '_ \ / _` | |  _| | | / __| |/ _ \| '_ \ 
| |_\ \ | | (_| | | | | (_| | | | | |_| \__ \ | (_) | | | |
 \____/_|  \__,_|_| |_|\__,_| \_|  \__,_|___/_|\___/|_| |_|";

        protected const string GFLogoWithLogin = @" _____                     _  ______         _         
|  __ \                   | | |  ___|       (_)                         U bent nu ingelogd als {0} {1}
| |  \/_ __ __ _ _ __   __| | | |_ _   _ ___ _  ___  _ __               [{2}] Log uit
| | __| '__/ _` | '_ \ / _` | |  _| | | / __| |/ _ \| '_ \ 
| |_\ \ | | (_| | | | | (_| | | | | |_| \__ \ | (_) | | | |
 \____/_|  \__,_|_| |_|\__,_| \_|  \__,_|___/_|\___/|_| |_|";

        protected const string DigitsOnlyMessage = "Alleen maar cijfers mogen ingevoerd worden!";
        protected const string LettersOnlyMessage = "Alleen maar letters mogen ingevoerd worden!";
        protected const string DigitsAndLettersOnlyMessage = "Alleen maar letters of cijfers mogen ingevoerd worden!";
        protected const string InputEmptyMessage = "Vul wat in alsjeblieft.";

        protected string BoxAroundText(List<string> input, string sym, int spacingside, int spacingtop, int maxlength, bool openbottom)
        {
            string output = new string(Convert.ToChar(sym), maxlength + 2 + spacingside * 2) + "\n";
            for (int a = 0; a < spacingtop; a++)
            {
                output += sym + new string(' ', maxlength + spacingside * 2) + sym + "\n";
            }

            foreach (var line in input)
            {
                output += sym + new string(' ', spacingside) + line + new string(' ', spacingside) + sym + "\n";
            }

            for (int a = 0; a < spacingtop; a++)
            {
                output += sym + new string(' ', maxlength + spacingside * 2) + sym + "\n";
            }

            if (openbottom)
            {
                return output;
            }
            return output += new string(Convert.ToChar(sym), maxlength + 2 + spacingside * 2) + "\n";
        }
        protected List<string> ReviewsToString(List<Review> reviews)
        {
            List<Klantgegevens> klantgegevens = io.GetCustomer(reviews.Select(i => i.Klantnummer).ToList());
            List<string> block = new List<string>();
            List<string> output = new List<string>();

            for (int a = 0; a < reviews.Count - 1; a += 2)
            {
                block.Add(new string(' ', 50) + "##" + new string(' ', 50));
                block.Add(new string(' ', 50) + "##" + new string(' ', 50));
                if (reviews[a].annomeme && reviews[a + 1].annomeme)
                {
                    block.Add("Anoniem: " + new string(' ', 50 - ("Anoniem: ").Length) + "##  " +
                            "Anoniem: " + new string(' ', 48 - ("Anoniem: ").Length));
                    block.Add(new string(' ', 50) + "##" + new string(' ', 50));
                }
                else if (reviews[a].annomeme && !reviews[a + 1].annomeme)
                {
                    block.Add("Anoniem: " + new string(' ', 50 - ("Anoniem: ").Length) + "##  " +
                            "Voornaam: " + klantgegevens[a + 1].voornaam + new string(' ', 48 - ("Voornaam: " + klantgegevens[a + 1].voornaam).Length));
                    block.Add(new string(' ', 50) + "##  " +
                        "Achternaam: " + klantgegevens[a + 1].achternaam + new string(' ', 48 - ("Achternaam: " + klantgegevens[a + 1].achternaam).Length));
                }
                else if (!reviews[a].annomeme && reviews[a + 1].annomeme)
                {
                    block.Add("Voornaam: " + klantgegevens[a].voornaam + new string(' ', 50 - ("Voornaam: " + klantgegevens[a].voornaam).Length) + "##  " +
                        "Anoniem: " + new string(' ', 48 - ("Anoniem: ").Length));
                    block.Add("Achternaam: " + klantgegevens[a].achternaam + new string(' ', 50 - ("Achternaam: " + klantgegevens[a].achternaam).Length) + "##  " +
                        new string(' ', 48));
                }
                else
                {
                    block.Add("Voornaam: " + klantgegevens[a].voornaam + new string(' ', 50 - ("Voornaam: " + klantgegevens[a].voornaam).Length) + "##  " +
                                        "Voornaam: " + klantgegevens[a + 1].voornaam + new string(' ', 48 - ("Voornaam: " + klantgegevens[a + 1].voornaam).Length));
                    block.Add("Achternaam: " + klantgegevens[a].achternaam + new string(' ', 50 - ("Achternaam: " + klantgegevens[a].achternaam).Length) + "##  " +
                        "Achternaam: " + klantgegevens[a + 1].achternaam + new string(' ', 48 - ("Achternaam: " + klantgegevens[a + 1].achternaam).Length));
                }

                if (reviews[a].message.Length < 50 - ("Review: ").Length && reviews[a + 1].message.Length < 48 - ("Review: ").Length)
                {
                    block.Add("Review: " + reviews[a].message + new string(' ', 50 - ("Review: " + reviews[a].message).Length) + "##  " +
                        "Review: " + reviews[a].message + new string(' ', 48 - ("Review: " + reviews[a].message).Length));
                }
                else
                {
                    List<string> msgparts1 = new List<string>();
                    List<string> msgparts2 = new List<string>();
                    if (reviews[a].message.Length > 50 - ("Review: ").Length)
                    {
                        string message = reviews[a].message;

                        msgparts1.Add(message.Substring(0, message.Substring(0, 50 - ("Review: ").Length).LastIndexOf(' ')));
                        message = message.Remove(0, msgparts1[0].Length + 1);

                        int count = 1;
                        while (message.Length > 50)
                        {
                            msgparts1.Add(message.Substring(0, message.Substring(0, 50).LastIndexOf(' ')));
                            message = message.Remove(0, msgparts1[count].Length + 1);
                            count++;
                        }
                        msgparts1.Add(message);
                    }
                    else
                    {
                        msgparts1.Add(reviews[a].message);
                    }

                    if (reviews[a + 1].message.Length > 48 - ("Review: ").Length)
                    {
                        string message = reviews[a + 1].message;

                        msgparts2.Add(message.Substring(0, message.Substring(0, 48 - ("Review: ").Length).LastIndexOf(' ')));
                        message = message.Remove(0, msgparts2[0].Length + 1);

                        int count = 1;
                        while (message.Length > 50)
                        {
                            msgparts2.Add(message.Substring(0, message.Substring(0, 48).LastIndexOf(' ')));
                            message = message.Remove(0, msgparts2[count].Length + 1);
                            count++;
                        }
                        msgparts2.Add(message);
                    }
                    else
                    {
                        msgparts2.Add(reviews[a + 1].message);
                    }

                    block.Add("Review: " + msgparts1[0] + new string(' ', 50 - ("Review: " + msgparts1[0]).Length) + "##  " +
                        "Review: " + msgparts2[0] + new string(' ', 48 - ("Review: " + msgparts2[0]).Length));
                    for (int b = 1; b < 4; b++)
                    {
                        if (msgparts1.Count - 1 >= b && msgparts2.Count - 1 >= b)
                        {
                            block.Add(msgparts1[b] + new string(' ', 50 - msgparts1[b].Length) + "##  " +
                                msgparts2[b] + new string(' ', 48 - msgparts2[b].Length));
                        }
                        else if (msgparts1.Count - 1 >= b && msgparts2.Count - 1 < b)
                        {
                            block.Add(msgparts1[b] + new string(' ', 50 - msgparts1[b].Length) + "##  " +
                                new string(' ', 48));
                        }
                        else if (msgparts1.Count - 1 < b && msgparts2.Count - 1 >= b)
                        {
                            block.Add(new string(' ', 50) + "##  " +
                                msgparts2[b] + new string(' ', 48 - msgparts2[b].Length));
                        }
                    }
                }


                block.Add("Rating: " + reviews[a].Rating + new string(' ', 50 - ("Rating: " + reviews[a].Rating).Length) + "##  " +
                    "Rating: " + reviews[a + 1].Rating + new string(' ', 48 - ("Rating: " + reviews[a + 1].Rating).Length));

                if (reviews[a].annomeme && !reviews[a + 1].annomeme)
                {
                    block.Add(new string(' ', 50) + "##  " +
                        "Datum: " + reviews[a + 1].datum + new string(' ', 48 - ("Datum: " + reviews[a + 1].datum).Length));
                }
                else if (!reviews[a].annomeme && reviews[a + 1].annomeme)
                {
                    block.Add("Datum: " + reviews[a].datum + new string(' ', 50 - ("Datum: " + reviews[a].datum).Length) + "##  " +
                        new string(' ', 48));
                }
                else
                {
                    block.Add("Datum: " + reviews[a].datum + new string(' ', 50 - ("Datum: " + reviews[a].datum).Length) + "##  " +
                        "Datum: " + reviews[a + 1].datum + new string(' ', 48 - ("Datum: " + reviews[a + 1].datum).Length));
                }
                block.Add(new string(' ', 50) + "##" + new string(' ', 50));
                block.Add(new string(' ', 50) + "##" + new string(' ', 50));

                output.Add(BoxAroundText(block, "#", 2, 0, 102, true));
                block = new List<string>();
            }

            return output;
        }
        protected List<string> MakePages(List<string> alldata, int maxblocks)
        {
            string[] output = new string[alldata.Count / maxblocks + 1];

            for (int a = 0, b = 1; a < alldata.Count; a++)
            {
                if (a < maxblocks * b)
                {
                    output[b - 1] += alldata[a];
                }
                else
                {
                    b++;
                    output[b - 1] += alldata[a];
                }
            }

            List<string> done = output.ToList();
            done.RemoveAll(x => x == null);
            return done;
        }

        /// <summary>
        /// This is the main function of the current screen. Here is all the logic of that current screen
        /// </summary>
        /// <returns>This function returns the ID of the next screen to display</returns>
        public abstract int DoWork();

        /// <summary>
        /// This function updates all screens with data from one screen to an other
        /// </summary>
        /// <param name="screens">This is the list of screens to update</param>
        /// <returns>This returns the same list you just gave as param but now it has been updated with information</returns>
        public abstract List<Screen> Update(List<Screen> screens);

        /// <summary>
        /// Checks if a user is currently logged in or not
        /// </summary>
        /// <returns>True if the user is logged in false if not</returns>
        protected bool IsLoggedIn() => !(ingelogd.type == null || ingelogd.type == "No account found");

        protected void Logout() => ingelogd = new Login_gegevens();

        protected void LogoutWithMessage()
        {
            logoutUpdate = true;
            Console.WriteLine("U bent nu uitgelogd.");
            Console.WriteLine("Druk op een knop om verder te gaan.");
            Console.ReadKey();
        }

        /// <summary>
        /// Only used in the Update method where needed. Makes sure that on every screen the login data is updated.
        /// </summary>
        /// <param name="list">List with all the screens.</param>
        protected void DoLogoutOnEveryScreen(List<Screen> list) {
            if (logoutUpdate) 
            {
                list.ForEach(s => s.Logout());
                logoutUpdate = false;
            }
        }

        protected static void InvalidInputMessage() => Console.WriteLine("\nU moet wel een juiste keuze maken...\nDruk op en knop om verder te gaan.");

        private static bool IsInputEmpty(string input) => input == "";

        private bool ValidateInput(string input, Func<char, bool> conditionPerChar)
        {
            char[] charsOfInput = input.ToCharArray();

            for (int i = 0; i < charsOfInput.Length; i++)
            {
                if (charsOfInput[i] == ' ') continue;
                if (!conditionPerChar(charsOfInput[i])) return false;
            }

            return true;
        }

        private bool ValidateInput(string input, Func<string, bool> conditionInput) => conditionInput(input);

        /// <summary>
        /// With this method you can ask the user for input and add a condition based on what type of characters are allowed in the input.
        /// If you only need to ask the user for input without any checks on the input please use Console.Readline() instead.
        /// </summary>
        /// <param name="conditionPerChar">The lambda that gets called to check if every character in the input string matches a certain condition.</param>
        /// <param name="onFalseMessage">The message to display when the condition failes.</param>
        /// <returns>The input that has been asked</returns>
        protected string AskForInput(Func<char, bool> conditionPerChar, string onFalseMessage = "", bool required = true)
        {
            string input = Console.ReadLine();

            if (required)
            {
                if (IsInputEmpty(input))
                {
                    Console.WriteLine(InputEmptyMessage);
                    return AskForInput(conditionPerChar, onFalseMessage, required);
                }
            }

            if (!ValidateInput(input, conditionPerChar))
            {
                Console.WriteLine(onFalseMessage);
                return AskForInput(conditionPerChar, onFalseMessage, required);
            }

            return input;
        }

        /// <summary>
        /// With this method you can ask the user for input and add a condition based on what type of characters are allowed in the input.
        /// If you only need to ask the user for input without any checks on the input please use Console.Readline() instead.
        /// </summary>
        /// <param name="conditionInput">The lambda that gets called to check if the input itself matches a certain condition</param>
        /// <param name="onFalseMessage">The message to display when the condition failes.</param>
        /// <returns>The input that has been asked</returns>
        protected string AskForInput(Func<string, bool> conditionInput, string onFalseMessage = "", bool required = true)
        {
            string input = Console.ReadLine();

            if (required)
            {
                if (IsInputEmpty(input))
                {
                    Console.WriteLine(InputEmptyMessage);
                    return AskForInput(conditionInput, onFalseMessage, required);
                }
            }

            if (!ValidateInput(input, conditionInput))
            {
                Console.WriteLine(onFalseMessage);
                return AskForInput(conditionInput, onFalseMessage, required);
            }

            return input;
        }

        /// <summary>
        /// With this method you can ask the user for input and add a condition based on what type of characters are allowed in the input.
        /// If you only need to ask the user for input without any checks on the input please use Console.Readline() instead.
        /// </summary>
        /// <param name="conditionPerChar">The lambda that gets called to check if every character in the input string matches a certain condition.</param>
        /// <param name="conditionInput">The lambda that gets called to check if the input itself matches a certain condition</param>
        /// <param name="onFalseMessage">The message to display when the condition failes.</param>
        /// <returns>The input that has been asked</returns>
        protected string AskForInput(Func<char, bool> conditionPerChar, Func<string, bool> conditionInput, string onFalseMessage = "", bool required = true)
        {
            string input = Console.ReadLine();

            if (required)
            {
                if (IsInputEmpty(input))
                {
                    Console.WriteLine(InputEmptyMessage);
                    return AskForInput(conditionPerChar, conditionInput, onFalseMessage, required);
                }
            }

            if (!ValidateInput(input, conditionPerChar))
            {
                Console.WriteLine(onFalseMessage);
                return AskForInput(conditionPerChar, onFalseMessage, required);
            }

            return ValidateInput(input, conditionInput) ? input : AskForInput(conditionInput, onFalseMessage);
        }

        /// <summary>
        /// Returns a variant of the GFLogo string based on wether the user is logged in or not.
        /// </summary>
        /// <param name="highestNumber">This is the number to display the logout choice with.</param>
        /// <returns>GFLogo string</returns>
        public string GetGFLogo(int highestNumber = -1) => !IsLoggedIn() || highestNumber < 0 ? GFLogo + "\n" : string.Format(GFLogoWithLogin, ingelogd.klantgegevens.voornaam, ingelogd.klantgegevens.achternaam, highestNumber);
    }

    public class StartScreen : Screen
    {
        public override int DoWork()
        {
            Console.WriteLine(GetGFLogo(4));
            Console.WriteLine("Kies een optie:");
            Console.WriteLine("[1] Laat alle gerechten zien");
            Console.WriteLine("[2] Laat alle reviews zien");
            if (IsLoggedIn() && ingelogd.type == "Gebruiker")
            {
                Console.WriteLine("[3] Klant menu");
            }
            else if (IsLoggedIn() && ingelogd.type == "Medewerker")
            {
                Console.WriteLine("[3] Medewerker menu");
            }
            else if (IsLoggedIn() && ingelogd.type == "Eigenaar")
            {
                Console.WriteLine("[3] Eigenaar menu");
            }
            else
            {
                Console.WriteLine("[3] Registreer");
                Console.WriteLine("[4] Login");
            }

            string choice = Console.ReadLine();

            if (choice != "0" && choice != "1" && choice != "2" && choice != "3" && choice != "4" && choice != "100" && choice != "101" && choice != "102" && choice != "103" && choice != "104" && choice != "105" && choice != "1000")
            {
                InvalidInputMessage();
                Console.ReadKey();
                return 0;
            }
            else
            {
                switch (Convert.ToInt32(choice))
                {
                    case 1:
                        return 1;
                    case 2:
                        return 2;
                    case 3:
                        if (!IsLoggedIn())
                        {
                            return 3;
                        }
                        else if (ingelogd.type == "Gebruiker")
                        {
                            return 5;
                        }
                        else if (ingelogd.type == "Medewerker")
                        {
                            return 5;
                        }
                        else
                        {
                            return 11;
                        }
                    case 4:
                        if (!IsLoggedIn())
                        {
                            return 4;
                        }
                        else
                        {
                            LogoutWithMessage();
                            return 0;
                        }
                    case 100:
                        code_gebruiker.Debug();
                        return 0;
                    case 101:
                        code_eigenaar.Debug();
                        return 0;
                    case 102:
                        code_login.Debug();
                        return 0;
                    case 103:
                        code_medewerker.Debug();
                        return 0;
                    case 104:
                        testing_class.Debug();
                        return 0;
                    case 105:
                        List<Login_gegevens> login_Gegevens = io.GetDatabase().login_gegevens;
                        Login_gegevens dataEigenaar = new();
                        dataEigenaar.email = "eigenaar@gmail.com";
                        dataEigenaar.password = "0000";
                        dataEigenaar.type = "Eigenaar";
                        dataEigenaar.klantgegevens = new Klantgegevens();
                        dataEigenaar.klantgegevens.klantnummer = login_Gegevens[login_Gegevens.Count - 1].klantgegevens.klantnummer + 1;
                        dataEigenaar.klantgegevens.voornaam = "Bob";
                        dataEigenaar.klantgegevens.achternaam = "De Boer";

                        Login_gegevens dataMedewerker = new();
                        dataMedewerker.email = "medewerker@gmail.com";
                        dataMedewerker.password = "0000";
                        dataMedewerker.type = "Medewerker";
                        dataMedewerker.klantgegevens = new Klantgegevens();
                        dataMedewerker.klantgegevens.klantnummer = login_Gegevens[login_Gegevens.Count - 1].klantgegevens.klantnummer + 1;
                        dataMedewerker.klantgegevens.voornaam = "Bob";
                        dataMedewerker.klantgegevens.achternaam = "De Wasser";

                        Login_gegevens dataGebruiker = new();
                        dataGebruiker.email = "gebruiker@gmail.com";
                        dataGebruiker.password = "0000";
                        dataGebruiker.type = "Gebruiker";
                        dataGebruiker.klantgegevens = new Klantgegevens();
                        dataGebruiker.klantgegevens.klantnummer = login_Gegevens[login_Gegevens.Count - 1].klantgegevens.klantnummer + 1;
                        dataGebruiker.klantgegevens.voornaam = "Bob";
                        dataGebruiker.klantgegevens.achternaam = "De Bouwer";

                        DateTime resDate = new(2050, 3, 1, 7, 0, 0);

                        var list = new List<int>();
                        list.Add(dataGebruiker.klantgegevens.klantnummer);

                        code_login.Register(dataEigenaar);
                        code_login.Register(dataMedewerker);
                        code_login.Register(dataGebruiker);
                        code_gebruiker.MakeCustomerReservation(resDate, dataGebruiker.klantgegevens.klantnummer, 1, false);
                        return 0;
                    case 1000:
                        io.ResetFilesystem();
                        return 0;
                }
            }

            return 0;
        }

        public override List<Screen> Update(List<Screen> screens)
        {
            DoLogoutOnEveryScreen(screens);
            return screens;
        }
    }

    public class GerechtenScreen : Screen
    {
        //max lengte van een gerecht naam is 25 tekens
        private readonly List<Gerechten> gerechten = new List<Gerechten>();
        private readonly List<string> gerechtenstring = new List<string>();
        private readonly string gerechtenbox;

        public GerechtenScreen()
        {
            gerechten = code_gebruiker.GetMenukaart();
            if (gerechten.Count > 0)
            {
                gerechtenstring = DishesToString();
                gerechtenbox = BoxAroundText(gerechtenstring, "#", 2, 2, 35, false);
            }
        }

        private List<string> DishesToString()
        {
            List<string> output = new List<string>();
            output.Add("Naam:" + new string(' ', 25 - 5) + "Prijs:" + new string(' ', 4));
            foreach (var gerecht in gerechten)
            {
                output.Add(gerecht.naam + new string(' ', 25 - gerecht.naam.Length) + gerecht.prijs + " euro" + new string(' ', 10 - (gerecht.prijs + " euro").Length));
            }

            return output;
        }

        public override int DoWork()
        {
            Console.WriteLine(GetGFLogo(2));
            Console.WriteLine(gerechtenbox);
            Console.WriteLine("[1] Ga terug");

            string choice = Console.ReadLine();
            if (choice == "1")
            {
                return 0;
            }
            else if (choice == "2")
            {
                LogoutWithMessage();
                return 0;
            }
            else
            {
                InvalidInputMessage();
                Console.ReadKey();
                return 1;
            }
        }

        public override List<Screen> Update(List<Screen> screens)
        {
            DoLogoutOnEveryScreen(screens);
            return screens;
        }
    }

    public class ReviewScreen : Screen
    {
        public ReviewScreen()
        {

        }
        public override int DoWork()
        {
            List<Review> reviews = io.GetReviews();

            Console.WriteLine(GetGFLogo(2));
            Console.WriteLine("Dit zijn alle reviews die zijn achtergelaten door onze klanten: \n");

            if (reviews.Count > 0)
            {
                Console.WriteLine(string.Join(null, ReviewsToString(io.GetReviews())) + new string('#', 108) + "\n" + "[1] Ga terug");
            }

            string choice = Console.ReadLine();

            if (choice == "1")
            {
                return 0;
            }
            else if (choice == "2")
            {
                LogoutWithMessage();
                return 0;
            }
            else
            {
                InvalidInputMessage();
                Console.ReadKey();
                return 2;
            }
        }

        public override List<Screen> Update(List<Screen> screens)
        {
            DoLogoutOnEveryScreen(screens);
            return screens;
        }
    }

    public class RegisterScreen : Screen
    {
        public RegisterScreen()
        {

        }

        public override int DoWork()
        {
            List<Login_gegevens> login_Gegevens = io.GetDatabase().login_gegevens;
            Login_gegevens new_gebruiker = new Login_gegevens();
            Console.WriteLine(GetGFLogo());
            Console.WriteLine("Hier kunt u een account aanmaken om o.a. reververingen te plaatsen voor GrandFusion!" + "\n");
            Console.WriteLine("Uw voornaam: ");
            new_gebruiker.klantgegevens = new Klantgegevens
            {
                voornaam = AskForInput(c => char.IsLetter(c), LettersOnlyMessage),
                klantnummer = login_Gegevens[login_Gegevens.Count - 1].klantgegevens.klantnummer + 1
            };
            Console.WriteLine("Uw achternaam: ");
            new_gebruiker.klantgegevens.achternaam = AskForInput(c => char.IsLetter(c), LettersOnlyMessage);

            Console.WriteLine("Uw geboorte datum met het formaat dd/mm/yyyy: ");
            
            dateInput:
                DateTime resultDateTime;
                if (DateTime.TryParseExact(Console.ReadLine(), "dd/mm/yyyy", new CultureInfo("nl-NL"), DateTimeStyles.None, out resultDateTime))
                {
                    new_gebruiker.klantgegevens.geb_datum = resultDateTime;
                }
                else
                {
                    Console.WriteLine("De datum die u hebt ingevoerd klopt niet, probeer het opnieuw.");
                    goto dateInput;
                }

            Console.WriteLine("Hieronder vult u uw adres in. Dit is in verband met het bestellen van eten. \n");
            Console.WriteLine("Uw woonplaats: ");
            new_gebruiker.klantgegevens.adres = new adres
            {
                woonplaats = AskForInput(c => char.IsLetter(c), LettersOnlyMessage),
                land = "NL"
            };
            Console.WriteLine("Uw postcode: ");
            new_gebruiker.klantgegevens.adres.postcode = AskForInput(c => char.IsLetterOrDigit(c), DigitsAndLettersOnlyMessage);

            Console.WriteLine("Uw straatnaam: ");
            new_gebruiker.klantgegevens.adres.straatnaam = AskForInput(c => char.IsLetter(c), LettersOnlyMessage);

            Console.WriteLine("Uw huisnummer: ");
            new_gebruiker.klantgegevens.adres.huisnummer = Convert.ToInt32(AskForInput(c => char.IsDigit(c), DigitsOnlyMessage));

            Console.WriteLine("\nHieronder vult u uw login gegevens: ");
            Console.WriteLine("Uw email adres: ");
            new_gebruiker.email = AskForInput(input => input.Contains('@') && input.Contains('.'), "De email is niet juist er mist een @ of een .");

            Console.WriteLine("Het wachtwoord voor uw account: ");
            new_gebruiker.password = Console.ReadLine();
            new_gebruiker.type = "Gebruiker";

            Console.WriteLine("\n Kloppen de bovenstaande gegevens?");
            Console.WriteLine("[1] Deze kloppen niet, breng me terug.");
            Console.WriteLine("[2] ja deze kloppen.");

            string choice = Console.ReadLine();

            if (choice != "1" && choice != "2")
            {
                InvalidInputMessage();
                return 3;
            }
            else if (choice == "1")
            {
                return 3;
            }

        a:
            if (code_login.Register(new_gebruiker) == "Succes!")
            {
                Console.WriteLine("U bent succesfull geregistreerd!");
                Console.WriteLine("Druk op een knop om naar het hoofdmenu te gaan");
                Console.ReadKey();
                return 0;
            }
            else if (code_login.Register(new_gebruiker) == "This email and account type is already in use")
            {
                Console.WriteLine("Dit account bestaat al, probeer een ander email adres:");
                new_gebruiker.email = Console.ReadLine();
                goto a;
            }
            else if (code_login.Register(new_gebruiker) == "Password must contain at least 8 characters, 1 punctuation mark and 1 number.")
            {
                Console.WriteLine("Het wachtwoord moet minimaal 8 tekens, 1 leesteken en 1 nummer bevatten.");
                Console.WriteLine("Het wachtwoord voor uw account: ");
                new_gebruiker.password = Console.ReadLine();
                goto a;
            }

            return 3;
        }

        public override List<Screen> Update(List<Screen> screens)
        {
            return screens;
        }
    }

    public class LoginScreen : Screen
    {
        public LoginScreen()
        {

        }

        public override int DoWork()
        {
            Console.WriteLine(GetGFLogo());
            Console.WriteLine("Log hier in met uw email en wachtwoord: \n");
            Console.WriteLine("Uw email: ");
            string email = Console.ReadLine();
            Console.WriteLine("uw wachtwoord: ");
            string password = Console.ReadLine();
            ingelogd = code_login.Login_Check(email, password);
            if (ingelogd.type == "No account found")
            {
                Console.WriteLine("Wachtwoord of email is niet juist, druk op een toets om opniew te proberen.");
                Console.ReadKey();
                return 4;
            }
            else
            {
                Console.WriteLine("U bent ingelogd!");
                Console.WriteLine("Druk op een toets om terug naar het hoofdmenu te gaan.");
                Console.ReadKey();
                return 0;
            }
        }

        public override List<Screen> Update(List<Screen> screens)
        {
            foreach (var screen in screens)
            {
                screen.ingelogd = ingelogd;
            }

            return screens;
        }
    }

    public class ClientMenuScreen : Screen
    {
        public ClientMenuScreen()
        {

        }

        public override int DoWork()
        {
            Console.WriteLine(GetGFLogo(7));
            Console.WriteLine("Welkom in het klanten menu.");
            Console.WriteLine("[1] Maak een reservering aan");
            Console.WriteLine("[2] Maak een review aan");
            Console.WriteLine("[3] Bewerk een review");
            Console.WriteLine("[4] Verwijder een review");
            Console.WriteLine("[5] Bekijk uw eigen reviews");
            Console.WriteLine("[6] Ga terug");

            string choice = Console.ReadLine();

            if (choice != "1" && choice != "2" && choice != "3" && choice != "4" && choice != "5" && choice != "6" && choice != "7")
            {
                InvalidInputMessage();
                Console.ReadKey();
                return 5;
            }
            switch (choice)
            {
                case "1":
                    return 6;
                case "2":
                    return 7;
                case "3":
                    return 8;
                case "4":
                    return 9;
                case "5":
                    return 10;
                case "6":
                    return 0;
                case "7":
                    LogoutWithMessage();
                    return 0;
            }


            return 5;
        }

        public override List<Screen> Update(List<Screen> screens)
        {
            DoLogoutOnEveryScreen(screens);
            return screens;
        }
    }

    public class EditReviewScreen : Screen
    {
        public EditReviewScreen()
        {

        }

        public override int DoWork()
        {
            Console.WriteLine(GetGFLogo(2));

            int maxLength = 55;

            foreach (Review review in io.GetReviews(ingelogd.klantgegevens))
            {
                List<string> outTemplate = new List<string>();
                outTemplate.Add($"Datum: {review.datum}");
                outTemplate.Add("Bericht:");

                string msg = review.message;
                string tempMsg = review.message;

                char[] arr = msg.ToCharArray();

                int remainingPos = 0;
                int lastOccurance = 0;
                for (int i = 0; i < arr.Length; i++)
                {
                    if (i == ' ') lastOccurance = i + 1;

                    if (i != 0 && i % maxLength == 0)
                    {
                        outTemplate.Add(tempMsg.Substring(0, maxLength).Trim());
                        tempMsg = tempMsg.Remove(0, maxLength);
                        remainingPos = i;
                    }
                }

                outTemplate.Add(msg.Substring(remainingPos).Trim());

                outTemplate.Add($"Beoordeling: {review.Rating}");
                outTemplate.Add($"Nummer: {review.ID}");

                for (int i = 0; i < outTemplate.Count; i++)
                {
                    if (outTemplate[i].Length != maxLength)
                    {
                        outTemplate[i] = outTemplate[i] + new string(' ', maxLength - outTemplate[i].Length);
                    }
                }

                Console.Write(BoxAroundText(
                    outTemplate,
                    "#",
                    2,
                    2,
                    maxLength,
                    true
                ));            
            }

            Console.WriteLine("Hier kunt u een review bewerken. Kies een review uit de lijst met reviews. (Type de nummer van de review in)");

            int id = int.Parse(AskForInput(c => char.IsDigit(c), DigitsOnlyMessage));

            Console.WriteLine("Wat is de nieuwe beoordeling die u wilt geven?");
            int rating = int.Parse(AskForInput(c => char.IsDigit(c), input => int.Parse(input) > 0 || int.Parse(input) < 6, DigitsOnlyMessage));

            Console.WriteLine("Wat is de nieuwe bericht die u wilt geven?");
            string message = Console.ReadLine();

            Console.WriteLine("Klopt alles?\n[1] Ja\n[2] Nee");

            if (AskForInput(c => char.IsDigit(c), input => input == "1" || input == "2", DigitsOnlyMessage) == "1")
            {
                code_gebruiker.OverwriteReview(id, rating, ingelogd.klantgegevens, message);
                Console.WriteLine("Review is bewerkt! Klik op een knop om door te gaan.");
                Console.ReadKey();
                return 5;
            }

            return 8;
        }

        public override List<Screen> Update(List<Screen> screens)
        {
            DoLogoutOnEveryScreen(screens);
            return screens;
        }
    }

    public class OwnerMenuScreen : Screen
    {
        public OwnerMenuScreen()
        {

        }

        public override int DoWork()
        {
            Console.WriteLine(GetGFLogo(6));
            Console.WriteLine("Welkom bij het eigenaars menu.");
            Console.WriteLine("[1] Gerechten");
            Console.WriteLine("[2] Reservering");
            Console.WriteLine("[3] Ingredienten");
            Console.WriteLine("[4] Inkomsten");
            Console.WriteLine("[5] Ga terug");

            string choice = Console.ReadLine();

            if (!(new string[6] { "1", "2", "3", "4", "5", "6" }).Contains(choice))
            {
                InvalidInputMessage();
                Console.ReadKey();
                return 11;
            }
            else
            {
                switch (Convert.ToInt32(choice))
                {
                    case 1:
                        return 12;
                    case 2:
                        return 13;
                    case 3:
                        return 14;
                    case 4:
                        return 15;
                    case 5:
                        return 0;
                    case 6:
                        logoutUpdate = true;
                        return 0;
                }
            }
            return 11;
        }

        public override List<Screen> Update(List<Screen> screens)
        {
            DoLogoutOnEveryScreen(screens);
            return screens;
        }
    }

    public class AddMealScreen : Screen
    {
        public AddMealScreen()
        {

        }

        public override int DoWork()
        {
            Console.WriteLine(GetGFLogo());
            Console.WriteLine("Hier kunt u een gerecht toevoegen.");

            string naam;
            double prijs;
            bool speciaal;
            List<int> ingredienten = new();
            List<string> allergenen = new();

            Console.WriteLine("Wat is de naam van het gerecht?");
            naam = AskForInput(c => char.IsLetterOrDigit(c), DigitsAndLettersOnlyMessage);
            Console.WriteLine("Wat is de prijs van het gerecht?");
            prijs = double.Parse(AskForInput(c => char.IsDigit(c), DigitsOnlyMessage));
            Console.WriteLine("Is het gerecht een special?");
            speciaal = AskForInput(input => input.ToLower() == "ja" || input.ToLower() == "nee", "Type in ja of nee alstublieft.") == "ja";
            Console.WriteLine("Geef nu aan de allergenen van het gerecht, als u geen allergenen wilt aangeven of als u klaar bent laat dab de invoerveld leeg en klik op enter");
            string input;
            do
            {
                input = AskForInput(c => char.IsLetter(c), LettersOnlyMessage);
                if (input.Trim() != "") allergenen.Add(input);
            } while (input.Trim() != "");

            code_eigenaar.CreateMeal(naam, false, prijs, speciaal, false, ingredienten, allergenen);

            return 8;
        }

        public override List<Screen> Update(List<Screen> screens)
        {
            DoLogoutOnEveryScreen(screens);
            return screens;
        }
    }
}
