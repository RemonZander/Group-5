using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Text.RegularExpressions;

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
            screens.Add(new ClientMenuScreen());// Wordt ViewFeedbackScreen
            screens.Add(new MakeFeedbackScreen()); // Wordt MakeFeedBackScreen
            screens.Add(new ViewReviewScreen());
            #endregion
            #region Eigenaar
            screens.Add(new OwnerMenuScreen());
            screens.Add(new OwnerMenuScreen()); // Gerechten
            screens.Add(new OwnerMenuScreen()); // Reservering
            screens.Add(new IngredientsScreen()); // Ingredienten
            screens.Add(new OwnerMenuScreen()); // Inkomsten
            #endregion
            #region Medewerker
            screens.Add(new EmployeeMenuScreen());
            screens.Add(new GetReservationsScreen()); // Reserveringen
            screens.Add(new AddTableToReservationScreen()); // Tafels koppelen
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


    public abstract partial class Screen
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
|  __ \                   | | |  ___|       (_)                      Druk op de esc knop om een scherm terug te gaan.
| |  \/_ __ __ _ _ __   __| | | |_ _   _ ___ _  ___  _ __  
| | __| '__/ _` | '_ \ / _` | |  _| | | / __| |/ _ \| '_ \ 
| |_\ \ | | (_| | | | | (_| | | | | |_| \__ \ | (_) | | | |
 \____/_|  \__,_|_| |_|\__,_| \_|  \__,_|___/_|\___/|_| |_|";

        protected const string GFLogoWithLogin = @" _____                     _  ______         _         
|  __ \                   | | |  ___|       (_)                       U bent nu ingelogd als {0} {1}
| |  \/_ __ __ _ _ __   __| | | |_ _   _ ___ _  ___  _ __             [0] Log uit
| | __| '__/ _` | '_ \ / _` | |  _| | | / __| |/ _ \| '_ \ 
| |_\ \ | | (_| | | | | (_| | | | | |_| \__ \ | (_) | | | |           Druk op de esc knop om een scherm terug te gaan.
 \____/_|  \__,_|_| |_|\__,_| \_|  \__,_|___/_|\___/|_| |_|";

        protected const string DigitsOnlyMessage = "Alleen maar cijfers mogen ingevoerd worden!";
        protected const string LettersOnlyMessage = "Alleen maar letters mogen ingevoerd worden!";
        protected const string DigitsAndLettersOnlyMessage = "Alleen maar letters of cijfers mogen ingevoerd worden!";
        protected const string InputEmptyMessage = "Vul wat in alsjeblieft.";
        protected const string InvalidInputMessage = "U moet wel een juiste keuze maken...";
        protected const string ESCAPE_KEY = "ESCAPE";
        protected const string ENTER_KEY = "ENTER";
        protected const string BACKSPACE_KEY = "BACKSPACE";
        protected const string UP_ARROW = "UPARROW";
        protected const string DOWN_ARROW = "DOWNARROW";
        protected const string LEFT_ARROW = "LEFTARROW";
        protected const string RIGHT_ARROW = "RIGHTARROW";

        [Obsolete()]
        /// <summary>
        /// Returns a variant of the GFLogo string based on wether the user is logged in or not.
        /// </summary>
        /// <param name="highestNumber">This is the number to display the logout choice with.</param>
        /// <returns>GFLogo string</returns>
        protected string GetGFLogo(int highestNumber = -1) => !IsLoggedIn() || highestNumber < 0 ? GFLogo + "\n" : string.Format(GFLogoWithLogin, ingelogd.klantgegevens.voornaam, ingelogd.klantgegevens.achternaam);

        /// <summary>
        /// Returns a variant of the GFLogo string based on wether the user is logged in or not.
        /// </summary>
        /// <param name="highestNumber">This is the number to display the logout choice with.</param>
        /// <returns>GFLogo string</returns>
        protected string GetGFLogo() => !IsLoggedIn() ? GFLogo + "\n" : string.Format(GFLogoWithLogin, ingelogd.klantgegevens.voornaam, ingelogd.klantgegevens.achternaam);

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
            Console.WriteLine("\nU bent nu uitgelogd.");
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
        /// Returns true if the key with the specified keycode is pressed.
        /// </summary>
        /// <param name="key">This is the name of the key, use one of the key constants described in the BaseScreen.</param>
        /// <returns>True if the right key is pressed, false is not</returns>
        protected bool IsKeyPressed(ConsoleKeyInfo cki, string key) => cki.Key.ToString().ToUpper() == key;

        #region Deprecated
        [Obsolete()]
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

        [Obsolete()]
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

        [Obsolete()]
        protected string AskForInput()
        {
            bool AskRepeat = true;
            string output = "";

            while (AskRepeat)
            {
                ConsoleKeyInfo CKInfo = Console.ReadKey(true);

                if (IsKeyPressed(CKInfo, ENTER_KEY)) break;

                if (IsKeyPressed(CKInfo, BACKSPACE_KEY))
                {
                    (int, int) curserPos = Console.GetCursorPosition();
                    if (curserPos.Item1 > 0)
                    {
                        Console.SetCursorPosition(curserPos.Item1 - 1, curserPos.Item2);
                        Console.Write(" ");
                    }
                }

                output += CKInfo.KeyChar;
                Console.Write(CKInfo.KeyChar);
            }

            return output;
        }

        [Obsolete()]
        protected string AskForInput(Func<char, bool> conditionPerChar, Func<string, bool> conditionInput, (string, string) onFalseMessage, bool required = true)
        {
            string input = AskForInput();

            if (required)
            {
                if (IsInputEmpty(input))
                {
                    Console.WriteLine(InputEmptyMessage);
                    return AskForInput(conditionPerChar, conditionInput, onFalseMessage, required);
                }
            }

            if (conditionPerChar != null && !ValidateInput(input, conditionPerChar))
            {
                Console.WriteLine(onFalseMessage.Item1);
                return AskForInput(conditionPerChar, conditionInput, onFalseMessage, required);
            }

            if (conditionInput != null && !ValidateInput(input, conditionInput))
            {
                Console.WriteLine(onFalseMessage.Item2);
                return AskForInput(conditionPerChar, conditionInput, onFalseMessage, required);
            }

            return input;
        }
        #endregion

        protected (string, int) AskForInput(int screenIndex)
        {
            bool AskRepeat = true;
            List<char> output = new();

            while (AskRepeat)
            {
                ConsoleKeyInfo CKInfo = Console.ReadKey(true);

                if (IsKeyPressed(CKInfo, ENTER_KEY)) break;

                if (IsKeyPressed(CKInfo, ESCAPE_KEY)) return (null, screenIndex);

                if (IsKeyPressed(CKInfo, BACKSPACE_KEY))
                {
                    (int, int) curserPos = Console.GetCursorPosition();
                    if (curserPos.Item1 > 0)
                    {
                        Console.SetCursorPosition(curserPos.Item1 - 1, curserPos.Item2);
                        Console.Write(" ");
                        output.RemoveAt(output.Count - 1);
                    }
                }
                else
                {
                    output.Add(CKInfo.KeyChar);
                }

                Console.Write(CKInfo.KeyChar);
            }

            // -1 means no interruptions has been found while asking for input
            return (string.Join(null, output), -1);
        }

        /// <summary>
        /// With this method you can ask the user for input and add a condition based on what type of characters are allowed in the input.
        /// If you only need to ask the user for input without any checks on the input please use AskForInput() without parameters.
        /// </summary>
        /// <param name="screenIndex">The index of the screen you want to go to.</param>
        /// <param name="conditionPerChar">The lambda that gets called to check if every character in the input string matches a certain condition. Use null for no check.</param>
        /// <param name="conditionInput">The lambda that gets called to check if the input itself matches a certain condition. Use null for no check.</param>
        /// <param name="onFalseMessage">The messages to display when the condition failes.</param>
        /// <returns>The input that has been asked</returns>
        protected (string, int, string) AskForInput(int screenIndex, Func<char, bool> conditionPerChar, Func<string, bool> conditionInput, (string, string) onFalseMessage, bool required = true)
        {
            (string, int) result = AskForInput(screenIndex);

            if (result.Item2 == -1 && required)
            {
                if (IsInputEmpty(result.Item1.Trim()))
                {
                    return (result.Item1, screenIndex, InputEmptyMessage);
                }
            }

            if (result.Item2 == -1 && conditionPerChar != null && !ValidateInput(result.Item1, conditionPerChar))
            {
                return (result.Item1, screenIndex, onFalseMessage.Item1);
            }

            if (result.Item2 == -1 && conditionInput != null && !ValidateInput(result.Item1, conditionInput))
            {
                return (result.Item1, screenIndex, onFalseMessage.Item2);
            }

            return (result.Item1, result.Item2, null);
        }
    }

    public abstract class StepScreen : Screen
    {
        protected List<string> steps = new();
        protected List<string> output = new();
        protected int currentStep = 0;
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

            (string, int, string) result = AskForInput(
                0,
                c => char.IsDigit(c),
                input => (new string[11] { "1", "2", "3", "4", "100", "101", "102", "103", "104", "105", "1000" }).Contains(input),
                (DigitsOnlyMessage, InvalidInputMessage),
                false
            );

            if (result.Item3 != null)
            {
                Console.WriteLine("\n" + result.Item3);
                Console.WriteLine("Druk op een knop om verder te gaan.");
                Console.ReadKey();
                return result.Item2;
            }

            switch (Convert.ToInt32(result.Item1))
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
                        return 16;
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
                Console.WriteLine(InvalidInputMessage);
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

            Console.WriteLine(GetGFLogo());
            Console.WriteLine("Dit zijn alle reviews die zijn achtergelaten door onze klanten: \n");

            int maxLength = 40;

            if (reviews.Count > 0)
            {
                List<List<string>> reviewsString = ReviewsToString(reviews);
                List<string> boxes = new List<string>();

                foreach (List<string> review in reviewsString)
                {
                    boxes.Add(BoxAroundText(review, "#", 2, 2, maxLength, true));
                }

                var pages = MakePages(boxes, 3);
            }
            else
            {
                Console.WriteLine("Er zijn nog geen reviews achtergelaten.");
            }

            string choice = AskForInput();

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
                Console.WriteLine(InvalidInputMessage);
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

    public class RegisterScreen : StepScreen
    {
        private Login_gegevens lg;

        public RegisterScreen()
        {
            steps.Add("Uw voornaam: ");
            steps.Add("Uw achternaam: ");
            steps.Add("Uw geboortedatum genoteerd als dag-maand-jaar: ");
            steps.Add("Hieronder vult u uw adres in. Dit is in verband met het bestellen van eten.\nUw woonplaats: ");
            steps.Add("Uw postcode: ");
            steps.Add("Uw straatnaam: ");
            steps.Add("Uw huisnummer: ");
            steps.Add("Hieronder vult u uw login gegevens: \nUw email adres: ");
            steps.Add("Het wachtwoord voor uw account: ");
            steps.Add("Kloppen de bovenstaande gegevens?\n[1] Deze kloppen niet, breng me terug.\n[2] ja deze kloppen.");

            output.Add(GetGFLogo());
            output.Add("Hier kunt u een account aanmaken om o.a. reververingen te plaatsen voor GrandFusion!");

            
            List<Login_gegevens> login_Gegevens = io.GetDatabase().login_gegevens;

            if (login_Gegevens == null)
            {
                login_Gegevens = new List<Login_gegevens>();
                io.GetDatabase().login_gegevens = login_Gegevens;
            }

            lg = new Login_gegevens()
            {
                type = "Gebruiker",
                klantgegevens = new Klantgegevens()
                {
                    klantnummer = login_Gegevens.Count == 0 ? 0 : login_Gegevens[login_Gegevens.Count - 1].klantgegevens.klantnummer + 1,
                    adres = new adres()
                    {
                        land = "NL",
                    }
                }
            };
        }

        public override int DoWork()
        {
            (string, int, string) result;

            Console.WriteLine(string.Join("\n", output));

            int ShowInvalidInput(string msg)
            {
                Console.WriteLine("\n" + result.Item3);
                Console.WriteLine("Druk op een knop om verder te gaan.");
                Console.ReadKey();
                return 3;
            }

            void Reset()
            {
                output.Clear();
                output.Add(GetGFLogo());
                output.Add("Hier kunt u een account aanmaken om o.a. reververingen te plaatsen voor GrandFusion!");
                currentStep = 0;
            }

            switch (currentStep)
            {
                case 0:
                    Console.WriteLine(steps[currentStep]);
                    result = AskForInput(0, c => char.IsLetter(c), null, (LettersOnlyMessage, null));

                    if (result.Item1 == null)
                    {
                        Reset();
                        return result.Item2;
                    }

                    if (result.Item3 != null) return ShowInvalidInput(result.Item3);

                    output.Add($"{steps[currentStep]}\n{result.Item1}");

                    lg.klantgegevens.voornaam = result.Item1;

                    currentStep++;
                    return 3;
                case 1:
                    Console.WriteLine(steps[currentStep]);
                    result = AskForInput(0, c => char.IsLetter(c), null, (LettersOnlyMessage, null));

                    if (result.Item1 == null)
                    {
                        Reset();
                        return result.Item2;
                    }

                    if (result.Item3 != null) return ShowInvalidInput(result.Item3);

                    output.Add($"{steps[currentStep]}\n{result.Item1}");

                    lg.klantgegevens.achternaam = result.Item1;

                    currentStep++;
                    return 3;
                case 2:
                    Console.WriteLine(steps[currentStep]);
                    DateTime resultDateTime = new DateTime();

                    result = AskForInput(
                        0,
                        c => char.IsDigit(c) || c == '/' || c == '-', 
                        input => DateTime.TryParse(input, out resultDateTime), 
                        ("Het formaat van de datum die u heeft ingevoerd klopt niet. Probeer het opnieuw.", "De datum die u hebt ingevoerd klopt niet, probeer het opnieuw.")
                    );

                    if (result.Item1 == null)
                    {
                        Reset();
                        return result.Item2;
                    }

                    if (result.Item3 != null) return ShowInvalidInput(result.Item3);

                    output.Add($"{steps[currentStep]}\n{result.Item1}");

                    lg.klantgegevens.geb_datum = resultDateTime;

                    currentStep++;
                    return 3;
                case 3:
                    Console.WriteLine(steps[currentStep]);
                    result = AskForInput(0, c => char.IsLetter(c), null, (LettersOnlyMessage, null));

                    if (result.Item1 == null)
                    {
                        Reset();
                        return result.Item2;
                    }

                    if (result.Item3 != null) return ShowInvalidInput(result.Item3);

                    output.Add($"{steps[currentStep]}\n{result.Item1}");

                    lg.klantgegevens.adres.woonplaats = result.Item1;

                    currentStep++;
                    return 3;
                case 4:
                    Console.WriteLine(steps[currentStep]);
                    result = AskForInput(0, c => char.IsLetterOrDigit(c), null, (DigitsAndLettersOnlyMessage, null));

                    if (result.Item1 == null)
                    {
                        Reset();
                        return result.Item2;
                    }

                    if (result.Item3 != null) return ShowInvalidInput(result.Item3);

                    output.Add($"{steps[currentStep]}\n{result.Item1}");

                    lg.klantgegevens.adres.postcode = result.Item1;

                    currentStep++;
                    return 3;
                case 5:
                    Console.WriteLine(steps[currentStep]);
                    result = AskForInput(0, c => char.IsLetter(c), null, (LettersOnlyMessage, null));

                    if (result.Item1 == null)
                    {
                        Reset();
                        return result.Item2;
                    }

                    if (result.Item3 != null) return ShowInvalidInput(result.Item3);

                    output.Add($"{steps[currentStep]}\n{result.Item1}");

                    lg.klantgegevens.adres.straatnaam = result.Item1;

                    currentStep++;
                    return 3;
                case 6:
                    Console.WriteLine(steps[currentStep]);
                    int possibleValue = -1;
                    result = AskForInput(0, c => char.IsDigit(c), input => int.TryParse(input, out possibleValue), (DigitsOnlyMessage, "De nummer die u heeft ingevoerd is te lang voor een gemiddeld huisnummer"));

                    if (result.Item1 == null)
                    {
                        Reset();
                        return result.Item2;
                    }

                    if (result.Item3 != null) return ShowInvalidInput(result.Item3);

                    output.Add($"{steps[currentStep]}\n{result.Item1}");

                    lg.klantgegevens.adres.huisnummer = possibleValue;

                    currentStep++;
                    return 3;
                case 7:
                    Console.WriteLine(steps[currentStep]);
                    Regex regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

                    result = AskForInput(0, null, input => regex.IsMatch(input), (null, "De email is niet juist er mist een @ of een ."));

                    if (result.Item1 == null)
                    {
                        Reset();
                        return result.Item2;
                    }

                    if (result.Item3 != null) return ShowInvalidInput(result.Item3);

                    output.Add($"{steps[currentStep]}\n{result.Item1}");

                    lg.email = result.Item1;

                    currentStep++;
                    return 3;
                case 8:
                    Console.WriteLine(steps[currentStep]);
                    (string, int) otherResult = AskForInput(0);

                    if (otherResult.Item1 == null)
                    {
                        Reset();
                        return otherResult.Item2;
                    }

                    output.Add($"{steps[currentStep]}\n{otherResult.Item1}");

                    lg.password = otherResult.Item1;

                    currentStep++;
                    return 3;
                case 9:
                    Console.WriteLine(steps[currentStep]);
                    result = AskForInput(0, c => char.IsDigit(c), input => Convert.ToInt32(input) == 1 || Convert.ToInt32(input) == 2, (DigitsOnlyMessage, InvalidInputMessage));

                    if (result.Item1 == null)
                    {
                        Reset();
                        return result.Item2;
                    }

                    if (result.Item3 != null) return ShowInvalidInput(result.Item3);

                    if (result.Item1 == "1")
                    {
                        Reset();
                        currentStep = 0;
                        return 3;
                    }

                    a:
                        if (code_login.Register(lg) == "Succes!")
                        {
                            Console.WriteLine("U bent succesfull geregistreerd!");
                            Console.WriteLine("Druk op een knop om naar het hoofdmenu te gaan");
                            Console.ReadKey();
                            return 0;
                        }
                        else if (code_login.Register(lg) == "This email and account type is already in use")
                        {
                            Console.WriteLine("Dit account bestaat al, probeer een ander email adres:");

                            result = AskForInput(0, null, input => (new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")).IsMatch(input), (null, "De email is niet juist er mist een @ of een ."));

                            if (result.Item3 != null) return ShowInvalidInput(result.Item3);

                            lg.email = result.Item1;
                            goto a;
                        }
                        else if (code_login.Register(lg) == "Password must contain at least 8 characters, 1 punctuation mark and 1 number.")
                        {
                            Console.WriteLine("Het wachtwoord moet minimaal 8 tekens, 1 leesteken en 1 nummer bevatten.");
                            Console.WriteLine("Het wachtwoord voor uw account: ");
                            lg.password = AskForInput(0).Item1;
                            goto a;
                        }
                    break;
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
                return 5;
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
            Console.WriteLine(GetGFLogo());
            Console.WriteLine("Welkom in het klanten menu.");
            Console.WriteLine("[1] Laat alle gerechten zien");
            Console.WriteLine("[2] Laat alle reviews zien");
            Console.WriteLine("[3] Maak een reservering aan");
            Console.WriteLine("[4] Maak een review aan");
            Console.WriteLine("[5] Bekijk en bewerk uw eigen reviews");
            Console.WriteLine("[6] Maak feedback aan");
            Console.WriteLine("[7] Bekijk en bewerk uw eigen feedback");

            int possibleValue = -1;
            int[] choices = new int[8] { 0, 1, 2, 3, 4, 5, 6, 7 };

            (string, int, string) result = AskForInput(
                0,
                null,
                input => int.TryParse(input, out possibleValue),
                (null, InvalidInputMessage)
            );

            if (result.Item3 != null)
            {
                Console.WriteLine(InvalidInputMessage);
                Console.ReadKey();
                return 5;
            }

            switch (result.Item1)
            {
                case "0":
                    LogoutWithMessage();
                    return 0;
                case "1":
                    return 1;
                case "2":
                    return 2;
                case "3":
                    return 6;
                case "4":
                    return 7;
                case "5":
                    return 10;
                case "6":
                    return 9;
                case "7":
                    return 8;
            }


            return 5;
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
            Console.WriteLine(GetGFLogo());
            Console.WriteLine("Welkom bij het eigenaars menu.");
            Console.WriteLine("[1] Gerechten");
            Console.WriteLine("[2] Reservering");
            Console.WriteLine("[3] Ingredienten");
            Console.WriteLine("[4] Inkomsten");
            Console.WriteLine("[5] Ga terug");

            string choice = Console.ReadLine();

            if (!(new string[6] { "1", "2", "3", "4", "5", "6" }).Contains(choice))
            {
                Console.WriteLine(InvalidInputMessage);
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

    public class AddMealScreen : StepScreen
    {
        private Gerechten meal;

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
            speciaal = AskForInput(8, null, input => input.ToLower() == "ja" || input.ToLower() == "nee", (null, "Type in ja of nee alstublieft.")).Item1 == "ja";
            Console.WriteLine("Geef nu aan de allergenen van het gerecht, als u geen allergenen wilt aangeven of als u klaar bent laat dab de invoerveld leeg en klik op enter");
            (string, int, string) result;
            do
            {
                result = AskForInput(8, c => char.IsLetter(c), null, (LettersOnlyMessage, null));
                if (result.Item1.Trim() != "") allergenen.Add(result.Item1);
            } while (result.Item1.Trim() != "");

/*            code_eigenaar.CreateMeal(naam, false, prijs, speciaal, false, ingredienten, allergenen);*/

            return 8;
        }

        public override List<Screen> Update(List<Screen> screens)
        {
            DoLogoutOnEveryScreen(screens);
            return screens;
        }
    }
}
