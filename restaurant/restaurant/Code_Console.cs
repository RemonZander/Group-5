using System;
using System.Collections.Generic;

namespace restaurant
{
    /*
    * This class is in charge of displaying the content of the application to the user.
    * Every input that the user has goes through here.
    * 
    * NOTE: Screens could be defined in JSON
    * NOTE: Code in FirstInit if condition can be copied into a constructor
    * 
    * TODO: Make distinction between DisplayScreen and ActionScreen
    * To elaborate: DisplayScreens are purely for displaying data, ActionScreens is for doing a certain action (like logging in)
    * Difference in code is in the part where the choices are located. ActionScreens dont have choices for going to another screen.
    * They instead hold a list of actions that must be performed in that screen to then move on to the next screen or display an error message of some kind.
    * 
    * ActionScreen Flow
    * ActionScreen holds a list of actions
    * ActionScreen holds a list called variables that is a dict with key: String | value: Dynamic
    * The purpose of the action is to take the variable and store it in the variables dict
    */
    public class Code_Console
    {
        private readonly Code_Login_menu Code_login = new Code_Login_menu();

        private readonly restaurant.Code_Eigenaar_menu Code_Eigenaar = new Code_Eigenaar_menu();

        private readonly restaurant.Code_Medewerker_menu Code_Medewerker = new Code_Medewerker_menu();

        private readonly restaurant.Code_Gebruiker_menu Code_Gebruiker = new Code_Gebruiker_menu();

        private readonly restaurant.Testing_class TestingClass = new Testing_class();
        
        private Dictionary<string, dynamic> currentScreen, previousScreen;

        private readonly List<Dictionary<string, dynamic>> screens = new List<Dictionary<string, dynamic>>();

        private readonly List<string> screenNames = new List<string>();

        private const int DisplayType = 0, ActionType = 1;

        private bool invalidInput = false;

        private string input = "0";

        private int screenIds = -1;

        private string GFLogo = @" _____                     _  ______         _             
|  __ \                   | | |  ___|       (_)            
| |  \/_ __ __ _ _ __   __| | | |_ _   _ ___ _  ___  _ __  
| | __| '__/ _` | '_ \ / _` | |  _| | | / __| |/ _ \| '_ \ 
| |_\ \ | | (_| | | | | (_| | | | | |_| \__ \ | (_) | | | |
 \____/_|  \__,_|_| |_|\__,_| \_|  \__,_|___/_|\___/|_| |_|";

        public Code_Console()
        {
            // restaurant.Database.login_gegevens.get returned null.

            Login_gegevens data = new Login_gegevens();
            data.email = "test@gmail.com";
            data.password = "123";
            data.type = "Eigenaar";

            Code_login.Register(data);

            // screens.Add(LoginScreen());
            screens.Add(StartScreenCustomer());
            screens.Add(StartScreenEmployee());
            screens.Add(ChoiceScreen());

            currentScreen = screens[screenNames.IndexOf("ChoiceScreen")];
            previousScreen = currentScreen;
        }

        #region Screen Functions

        private int GetScreenIdByName(string name)
        {
            return screenNames.IndexOf(name);
        }

        private void AddActionWithMessage(List<dynamic[]> actions, Func<string, dynamic> action, string message)
        {
            actions.Add(new dynamic[] { action, message });
        }

        private dynamic[] CreateChoice(int screenId, string text)
        {
            return new dynamic[] { screenId, text };
        }

        #endregion

        #region Screens

        /*
         * Creates a dict with string keys and different type of values.
         * id: int
         * name: string
         * output: string
         * choices: List<Dictionary<int, string>> int = the id of the screen | string is the message to display
         */
        private Dictionary<string, dynamic> CreateScreen(string name, int type = DisplayType)
        {
            screenIds++;
            screenNames.Add(name);

            var dict = new Dictionary<string, dynamic>
            {
                { "id", screenIds },
                { "output", "" },
            };

            if (type == DisplayType) {
                dict.Add("choices", new List<dynamic[]>());
            } else if (type == ActionType) {
                dict.Add("variables", new Dictionary<string, dynamic>());
                dict.Add("actions", new List<dynamic[]>());
            }

            return dict;
        }

        private Dictionary<string, dynamic> ChoiceScreen()
        {
            var dict = CreateScreen("ChoiceScreen");
            dict["output"] = $"{GFLogo}\nKies een optie:";
            dict["choices"].Add(CreateChoice(GetScreenIdByName("StartScreenCustomer"), "Customer"));
            dict["choices"].Add(CreateChoice(GetScreenIdByName("StartScreenEmployee"), "Employee"));
            return dict;
        }

        private Dictionary<string, dynamic> StartScreenCustomer()
        {
            var dict = CreateScreen("StartScreenCustomer");
            dict["output"] = $"{GFLogo}\nKlanten Scherm";
            return dict;
        }

        private Dictionary<string, dynamic> StartScreenEmployee()
        {
            var dict = CreateScreen("StartScreenEmployee");
            dict["output"] = $"{GFLogo}\nMedewerkers Scherm";
            return dict;
        }

        private Dictionary<string, dynamic> LoginScreen()
        {
            var dict = CreateScreen("LoginScreen", ActionType);
            dict["output"] = $"{GFLogo}\nPlease Login with your email and password.\n";

            // typecast the lambda to avoid error, maybe find another way to implement this more cleanly?
            AddActionWithMessage(dict["actions"], (Func<string, dynamic>)(input => dict["variables"].Add("email", input)), "Your email");
            AddActionWithMessage(dict["actions"], (Func<string, dynamic>)(input => dict["variables"].Add("psw", input)), "Your password");
            AddActionWithMessage(dict["actions"], (Func<string, dynamic>)(input =>
                {
                    if (input == "y")
                    {
                        return Code_login.Login_Check(dict["variables"]["email"], dict["variables"]["psw"]);
                    }
                    else
                    {
                        return null;
                    }
                }), "Are you sure? (y/n)"
            );

            return dict;
        }

        private Dictionary<string, dynamic> InvalidInputScreen()
        {
            var dict = CreateScreen("InvalidInputScreen");
            dict["output"] = "Please type a valid choice.\nValid choices are the ones marked with -> [] with a number inside it.\nDon't type your choice with the brackets (These things -> [])\nExample: When you see this option -> [1] you press the number: 1 and then click on enter";
            dict["choices"].Add(CreateChoice(previousScreen["id"], "Go back"));
            return dict;
        }

        #endregion

        private void ScreenManager(string input)
        {
            List<dynamic[]> choices = null;
            Dictionary<int, string> actions = null;

            int inputAsInteger = int.Parse(input);

            // Check if the current screen is a DisplayScreen or ActionScreen
            if (currentScreen.ContainsKey("choices")) {
                choices = currentScreen["choices"];

            } else if (currentScreen.ContainsKey("actions")) {
                actions = currentScreen["actions"];
            }

            // for every screen match the id with choice then get that screen and set it as current screen.
            foreach (var screen in screens)
            {
                if (choices != null)
                {
                    if (screen["id"] == choices[inputAsInteger - 1][0])
                    {
                        previousScreen = currentScreen;
                        currentScreen = screen;
                        break;
                    }
                }

                if (actions != null)
                {
                    var funcResult = currentScreen["actions"][0](input);

                    if (funcResult is Login_gegevens)
                    {
                        if (funcResult.type == "No account found")
                        {
                            Console.WriteLine("No Account Found");
                        }
                    }

                    currentScreen["actions"].RemoveAt(0);
                }
            }
        }

        private bool InputCheck(string input)
        {
            if (input == null)
            {
                invalidInput = true;
                return false;
            }

            if (currentScreen.ContainsKey("choices"))
            {
                bool isInteger = int.TryParse(input, out int number);

                if (!isInteger || !(number > 0 && number <= currentScreen["choices"].Count))
                {
                    invalidInput = true;
                    return false;
                }
            }

            return true;
        }

        public void Display()
        {
            if (invalidInput)
            {
                previousScreen = currentScreen;
                currentScreen = InvalidInputScreen();
                invalidInput = false;
            }

            string output = currentScreen["output"];

            if (currentScreen.ContainsKey("choices"))
            {
                int counter = 0;
                foreach (dynamic[] choice in currentScreen["choices"])
                {
                    output += $"\n[{++counter}] {choice[1]}";
                }
            }

            Console.WriteLine(output);

            input = Console.ReadLine();

            if (InputCheck(input))
            {
                if (input == "100")
                {
                    Code_Gebruiker.Debug();
                }
                else if (input == "101")
                {
                    Code_Eigenaar.Debug();
                }
                else if (input == "102")
                {
                    // Code_login.Debug();
                }
                else if (input == "103")
                {
                    // Code_Medewerker.Debug();
                }
                else if (input == "104")
                {
                    TestingClass.Debug();
                }
                else
                {
                    // Proceed
                    ScreenManager(input);
                }
            }
        }

        public void Refresh()
        {
            Console.Clear();
        }
    }
}