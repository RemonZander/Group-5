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

        private readonly List<Dictionary<string, dynamic>> screens = new List<Dictionary<string, dynamic>>();

        private readonly List<string> screenNames = new List<string>();

        private Dictionary<string, dynamic> currentScreen;

        private const int DisplayType = 0, ActionType = 1;

        // Generate a random int with it to make it more faultproof?
        private const string ActionCancelled = "ACTION_CANCELLED", ActionException = "ACTION_EXCEPTION";

        private const string ScreenNext = "SCREEN_NEXT", ScreenBack = "SCREEN_BACK";

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
            Login_gegevens data = new Login_gegevens();
            data.email = "test@gmail.com";
            data.password = "rU3#)J2A8$E";
            data.type = "Eigenaar";

            var result = Code_login.Register(data);

            screens.Add(LoginScreen());
            screens.Add(StartScreenCustomer());
            screens.Add(StartScreenEmployee());
            screens.Add(StartScreen());
            screens.Add(InvalidInputScreen());

            currentScreen = screens[screenNames.IndexOf("StartScreen")];
        }

        #region Screen Functions

        // Throw exception on -1
        private int GetScreenIdByName(string name)
        {
            return screenNames.IndexOf(name);
        }

        private void AddActionWithMessage(List<dynamic[]> actions, Func<string, dynamic> action, string message)
        {
            actions.Add(new dynamic[] { action, message });
        }

        private dynamic[] CreateChoice(int screenId, string text, string ScreenFlowDirection = ScreenNext)
        {
            return new dynamic[] { screenId, text , ScreenFlowDirection };
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
                { "name", name},
                { "output", "" },
                { "previousScreen", 0},
                { "type", type},
            };

            if (type == DisplayType) {
                dict.Add("choices", new List<dynamic[]>());
            } else if (type == ActionType) {
                dict.Add("actionStep", 0);
                dict.Add("variables", new Dictionary<string, dynamic>());
                dict.Add("actions", new List<dynamic[]>());
            }

            return dict;
        }

        private Dictionary<string, dynamic> StartScreen()
        {
            var dict = CreateScreen("StartScreen");
            dict["output"] = $"{GFLogo}\nKies een optie:";
            dict["choices"].Add(CreateChoice(GetScreenIdByName("StartScreenCustomer"), "Customer"));
            dict["choices"].Add(CreateChoice(GetScreenIdByName("LoginScreen"), "Employee"));
            return dict;
        }

        private Dictionary<string, dynamic> StartScreenCustomer()
        {
            var dict = CreateScreen("StartScreenCustomer");
            dict["output"] = $"{GFLogo}\nKlanten Scherm";
            dict["choices"].Add(CreateChoice(0, "Go back", ScreenBack));
            return dict;
        }

        private Dictionary<string, dynamic> StartScreenEmployee()
        {
            var dict = CreateScreen("StartScreenEmployee");
            dict["output"] = $"{GFLogo}\nMedewerkers Scherm";
            dict["choices"].Add(CreateChoice(0, "Go back", ScreenBack));
            return dict;
        }

        private Dictionary<string, dynamic> LoginScreen()
        {
            var dict = CreateScreen("LoginScreen", ActionType);
            dict["output"] = $"{GFLogo}\nPlease Login with your email and password.";

            // typecast the lambda to avoid error, maybe find another way to implement this more cleanly?
            AddActionWithMessage(dict["actions"], (Func<string, dynamic>)(input => {
                dict["variables"].Add("email", input);
                return null;
            }), "Your email");
            AddActionWithMessage(dict["actions"], (Func<string, dynamic>)(input => {
                dict["variables"].Add("psw", input);
                return null;
            }), "Your password");
            AddActionWithMessage(dict["actions"], (Func<string, dynamic>)(input =>
                {
                    if (input == "y")
                    {
                        return Code_login.Login_Check(dict["variables"]["email"], dict["variables"]["psw"]);
                    }
                    else if (input == "n")
                    {
                        return ActionCancelled;
                    }
                    else
                    {
                        return ActionException;
                    }
               }), "Are you sure? Type in y or n"
            );

            return dict;
        }

        private Dictionary<string, dynamic> InvalidInputScreen()
        {
            var dict = CreateScreen("InvalidInputScreen");
            dict["output"] = "Please type a valid choice.\nValid choices are the ones marked with -> [] with a number inside it.\nDon't type your choice with the brackets (These things -> [])\nExample: When you see this option -> [1] you press the number: 1 and then click on enter";
            dict["choices"].Add(CreateChoice(0, "Go back", ScreenBack));
            return dict;
        }

        private Dictionary<string, dynamic> ActionResultScreen(string outputMessage, params dynamic[][] choices)
        {
            var dict = CreateScreen("ActionResultScreen");
            dict["output"] = outputMessage;

            foreach (dynamic[] choice in choices)
            {
                if (choice[2] == ScreenBack)
                {
                    dict["previousScreen"] = choice[0];
                }

                dict["choices"].Add(choice);
            }

            return dict;
        }

        #endregion

        private void ScreenManager(string input)
        {
            List<dynamic[]> choices = null;
            List<dynamic[]> actions = null;

            // Check if the current screen is a DisplayScreen or ActionScreen
            if (currentScreen.ContainsKey("choices")) {
                choices = currentScreen["choices"];
            } else if (currentScreen.ContainsKey("actions")) {
                actions = currentScreen["actions"];
            }

            // for every screen match the id with choice then get that screen and set it as current screen.
            if (choices != null)
            {
                foreach (var screen in screens)
                {
                    int inputAsInteger = int.Parse(input);
                    if (choices[inputAsInteger - 1][0] == screen["id"])
                    {
                        if (currentScreen["name"] != "InvalidInputScreen" && currentScreen["name"] != "ActionResultScreen") 
                        {
                            screen["previousScreen"] = currentScreen["id"];
                        }

                        if (currentScreen["name"] == "ActionResultScreen")
                        {
                            screen["previousScreen"] = currentScreen["previousScreen"];
                        }

                        currentScreen = screen;
                        break;
                    }
                }
            }

            if (actions != null)
            {
                var funcResult = actions[currentScreen["actionStep"]][0](input);
                bool isLastStep = currentScreen["actionStep"] == actions.Count - 1;
                int actionPreviousScreenId = currentScreen["previousScreen"];

                // if the program has gone to every action in the screen reset the action
                if (isLastStep)
                {
                    currentScreen["actionStep"] = 0;
                    currentScreen["variables"].Clear();
                }

                // If result eq to null it means the action has no return value
                if (funcResult != null)
                {
                    if (funcResult.GetType() == typeof(string) && funcResult == ActionCancelled)
                    {
                        currentScreen = ActionResultScreen(
                            "The current action has been cancelled.",
                            CreateChoice(actionPreviousScreenId, "Go back", ScreenBack)
                        );
                    }

                    if (funcResult.GetType() == typeof(string) && funcResult == ActionException)
                    {
                        invalidInput = true;
                    }

                    if (funcResult.GetType() == typeof(Login_gegevens))
                    {
                        if (funcResult.type == "No account found")
                        {
                            currentScreen = ActionResultScreen(
                                "The account with the specified mail does not exist in our current Database.",
                                CreateChoice(actionPreviousScreenId, "Go back", ScreenBack)
                            );
                        }
                        else
                        {
                            currentScreen = ActionResultScreen("Succesfully logged in.", CreateChoice(GetScreenIdByName("StartScreenEmployee"), "Continue"));
                            currentScreen["previousScreen"] = actionPreviousScreenId;
                        }
                    }
                }

                if (!isLastStep) currentScreen["actionStep"] += 1;
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
                var temp = currentScreen["id"];
                currentScreen = screens[GetScreenIdByName("InvalidInputScreen")];
                currentScreen["previousScreen"] = temp;
                invalidInput = false;
            }

            string output = currentScreen["output"];

            if (currentScreen.ContainsKey("choices"))
            {
                int counter = 0;
                foreach (dynamic[] choice in currentScreen["choices"])
                {
                    if (choice[2] == ScreenBack)
                    {
                        choice[0] = currentScreen["previousScreen"];
                    }

                    output += $"\n[{++counter}] {choice[1]}";
                }
            }

            if (currentScreen.ContainsKey("actions") && currentScreen["actions"].Count > 0)
            {
                output += $"\n{currentScreen["actions"][currentScreen["actionStep"]][1]}";
            }

            Console.WriteLine(output);

            input = Console.ReadLine();

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
                Code_login.Debug();
            }
            else if (input == "103")
            {
                Code_Medewerker.Debug();
            }
            else if (input == "104")
            {
                TestingClass.Debug();
            } 
            else if (InputCheck(input))
            {
                ScreenManager(input);
            }
        }

        public void Refresh()
        {
            Console.Clear();
        }
    }
}