using System;
using System.Collections.Generic;

namespace restaurant
{
    public class Choice
    {
        public const string SCREEN_NEXT = "SCREEN_NEXT", SCREEN_BACK = "SCREEN_BACK";

        public string ScreenName { get; set; }
        public string Text { get; set; }
        public string ScreenFlowDirection { get; set; }

        public Choice(string screenName, string text, string screenDirection = SCREEN_NEXT)
        {
            ScreenName = screenName;
            Text = text;
            ScreenFlowDirection = screenDirection;
        }
    }

    public abstract class BaseScreen
    {
        public string Name { get; protected set; }
        public string Text { get; protected set; }
        public BaseScreen PreviousScreen { get; set; }
    }

    public class DisplayScreen : BaseScreen
    {
        public List<Choice> Choices = new List<Choice>();

        public DisplayScreen(string name, string text)
        {
            Name = name;
            Text = text;
        }
    }

    public class FunctionScreen : BaseScreen
    {
        public const string CANCELLED = "FUNCTION_CANCELLED", EXCEPTION = "FUNCTION_EXCEPTION", FINISHED = "FUNCTION_FINISHED";

        private int FunctionStep = 0;
        private int TotalFunctionSteps = 0;
        public readonly Dictionary<string, dynamic> Variables = new Dictionary<string, dynamic>();
        public readonly List<Tuple<Func<string, dynamic>, string>> Functions = new List<Tuple<Func<string, dynamic>, string>>();

        public FunctionScreen(string name, string text)
        {

            Name = name;
            Text = text;
        }

        public void AddFunctionWithMessage(Func<string, dynamic> func, string message)
        {
            Functions.Add(new Tuple<Func<string, dynamic>, string>( func, message ));
            TotalFunctionSteps++;
        }

        public void AddFunction(Func<string, dynamic> func)
        {
            AddFunctionWithMessage(func, "");
        }

        public Func<string, dynamic> GetCurrentFunction()
        {
            return Functions[FunctionStep].Item1;
        }

        public string GetCurrentMessage()
        {
            return Functions[FunctionStep].Item2;
        }

        public bool IsLastStep()
        {
            return FunctionStep == TotalFunctionSteps;
        }

        public void Reset()
        {
            FunctionStep = 0;
            Variables.Clear();
        }

        public void NextFunction()
        {
            FunctionStep++;
        }
    }

    public class Screens
    {
        public readonly List<BaseScreen> AllScreens = new List<BaseScreen>();
        public BaseScreen CurrentScreen { get; set; }

        public BaseScreen GetScreenByName(string name)
        {
            foreach (var screen in AllScreens)
            {
                if (screen.Name == name)
                {
                    return screen;
                } 
            }

            return null;
        }
    }

    /*
    * This class is in charge of displaying the content of the application to the user.
    * Every input that the user has goes through here.
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

        private readonly restaurant.IO IO = new IO();

        private readonly restaurant.Testing_class TestingClass = new Testing_class();

        private readonly Screens screens = new Screens();

        private BaseScreen currentScreen;

        private bool invalidInput = false;

        private string input = "0";

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

            Code_login.Register(data);

            DisplayScreen startScreen = new DisplayScreen("StartMenu", $"{GFLogo}\nKies een optie:");
            startScreen.Choices.Add(new Choice("StartScreenCustomer", "Klant"));
            startScreen.Choices.Add(new Choice("LoginScreenEmployee", "Medewerker"));

            DisplayScreen startScreenCustomer = new DisplayScreen("StartScreenCustomer", $"{GFLogo}\nKlanten Scherm");
            startScreenCustomer.Choices.Add(new Choice("StartMenu", "Ga terug", Choice.SCREEN_BACK));

            DisplayScreen startScreenEmployee = new DisplayScreen("StartScreenEmployee", $"{GFLogo}\nMedewerkers Scherm");
            startScreenEmployee.Choices.Add(new Choice("StartMenu", "Ga terug", Choice.SCREEN_BACK));

            DisplayScreen invalidInputScreen = new DisplayScreen("InvalidInputScreen", "Type alstublieft de correcte keuze in.\nCorrecte keuzes zijn gemarkeerd met -> [] met een nummer erin.\nType alleen de nummer van de keuze in.\nVoorbeeld: Met keuze [1] type je in 1");
            invalidInputScreen.Choices.Add(new Choice("StartMenu", "Ga terug", Choice.SCREEN_BACK));

            FunctionScreen loginScreen = new FunctionScreen("LoginScreenEmployee", $"{GFLogo}\nLog in met je Email en Wachtwoord");
            loginScreen.AddFunctionWithMessage(input => {
                loginScreen.Variables.Add("email", input);
                return null;
            }, "Je email");
            loginScreen.AddFunctionWithMessage(input => {
                loginScreen.Variables.Add("psw", input);

                Login_gegevens funcResult = Code_login.Login_Check(loginScreen.Variables["email"], loginScreen.Variables["psw"]);
                BaseScreen previousScreen = currentScreen.PreviousScreen;

                if (funcResult.type == "No account found")
                {
                    DisplayScreen noAccountFoundScreen = new DisplayScreen(
                        "",
                        "The account with the specified mail does not exist in our current Database."
                    );
                    noAccountFoundScreen.Choices.Add(new Choice("StartMenu", "Go back", Choice.SCREEN_BACK));

                    currentScreen = noAccountFoundScreen;
                }
                else
                {
                    DisplayScreen loginSuccesfullScreen = new DisplayScreen(
                        "",
                        "Login successfull."
                    );

                    loginSuccesfullScreen.Choices.Add(new Choice("StartScreenEmployee", "Continue"));

                    currentScreen = loginSuccesfullScreen;
                    currentScreen.PreviousScreen = previousScreen;
                }

                return FunctionScreen.FINISHED;
            }, "Je wachtwoord");


            screens.AllScreens.Add(loginScreen);
            screens.AllScreens.Add(startScreenCustomer);
            screens.AllScreens.Add(startScreenEmployee);
            screens.AllScreens.Add(startScreen);
            screens.AllScreens.Add(invalidInputScreen);

            currentScreen = screens.CurrentScreen = startScreen;
        }

        #region Screen Functions

        // Throw exception on -1
/*        private int GetScreenIdByName(string name)
        {
            return screenNames.IndexOf(name);
        }

        private void AddActionWithMessage(List<dynamic[]> actions, Func<string, dynamic> action, string message)
        {
            actions.Add(new dynamic[] { action, message });
        }

        private dynamic[] CreateChoice(int screenId, string text, string ScreenFlowDirection = SCREEN_NEXT)
        {
            return new dynamic[] { screenId, text , ScreenFlowDirection };
        }*/

        #endregion

        #region Screens

/*        private Dictionary<string, dynamic> CreateScreen(string name, int type = DisplayType)
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
        }*/

/*        private Dictionary<string, dynamic> StartScreen()
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
            dict["choices"].Add(CreateChoice(0, "Go back", SCREEN_BACK));
            return dict;
        }

        private Dictionary<string, dynamic> StartScreenEmployee()
        {
            var dict = CreateScreen("StartScreenEmployee");
            dict["output"] = $"{GFLogo}\nMedewerkers Scherm";
            dict["choices"].Add(CreateChoice(0, "Go back", SCREEN_BACK));
            return dict;
        }*/

/*        private Dictionary<string, dynamic> LoginScreen()
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

                return Code_login.Login_Check(dict["variables"]["email"], dict["variables"]["psw"]);
            }), "Your password");

            return dict;
        }*/

/*        private Dictionary<string, dynamic> InvalidInputScreen()
        {
            var dict = CreateScreen("InvalidInputScreen");
            dict["output"] = "Please type a valid choice.\nValid choices are the ones marked with -> [] with a number inside it.\nDon't type your choice with the brackets (These things -> [])\nExample: When you see this option -> [1] you press the number: 1 and then click on enter";
            dict["choices"].Add(CreateChoice(0, "Go back", SCREEN_BACK));
            return dict;
        }*/

/*        private Dictionary<string, dynamic> ActionResultScreen(string outputMessage, params dynamic[][] choices)
        {
            var dict = CreateScreen("ActionResultScreen");
            dict["output"] = outputMessage;

            foreach (dynamic[] choice in choices)
            {
                if (choice[2] == SCREEN_BACK)
                {
                    dict["previousScreen"] = choice[0];
                }

                dict["choices"].Add(choice);
            }

            return dict;
        }*/

        #endregion

        private void ScreenManager(string input)
        {
            // for every screen match the id with choice then get that screen and set it as current screen.
            if (currentScreen.GetType() == typeof(DisplayScreen))
            {
                var displayScreen = (DisplayScreen)currentScreen;
                var choices = displayScreen.Choices;

                foreach (var screen in screens.AllScreens)
                {
                    int inputAsInteger = int.Parse(input);
                    if (choices[inputAsInteger - 1].ScreenName == screen.Name)
                    {
                        if (currentScreen.Name != "InvalidInputScreen") 
                        {
                            screen.PreviousScreen = currentScreen;
                        }

                        currentScreen = screen;
                        break;
                    }
                }
            } 
            else if (currentScreen.GetType() == typeof(FunctionScreen)) 
            {
                FunctionScreen funcScreen = (FunctionScreen)currentScreen;

                var funcResult = funcScreen.GetCurrentFunction()(input);

                if (funcResult != null)
                {
                    if (funcResult.GetType() == typeof(string) && funcResult == FunctionScreen.CANCELLED)
                    {
                        DisplayScreen functionCancelledScreen = new DisplayScreen(
                            "",
                            "The current action has been cancelled."
                        );
                        functionCancelledScreen.Choices.Add(new Choice("", "Go back", Choice.SCREEN_BACK));

                        currentScreen = functionCancelledScreen;
                    }
                    else if (funcResult.GetType() == typeof(string) && funcResult == FunctionScreen.EXCEPTION)
                    {
                        invalidInput = true;
                    }
                    else if (funcResult.GetType() == typeof(string) && funcResult == FunctionScreen.FINISHED)
                    {
                        funcScreen.Reset();
                    }
                } else if (!funcScreen.IsLastStep()) funcScreen.NextFunction();
            }
        }

        private bool InputCheck(string input)
        {
            if (input == null)
            {
                invalidInput = true;
                return false;
            }

            if (currentScreen.GetType() == typeof(DisplayScreen))
            {
                DisplayScreen displayScreen = (DisplayScreen)currentScreen;
                bool isInteger = int.TryParse(input, out int number);

                if (!isInteger || !(number > 0 && number <= displayScreen.Choices.Count))
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
                /*                string temp;
                                if (currentScreen.GetType() == typeof(FunctionScreen))
                                {
                                    temp = currentScreen.Name;
                                }
                                else
                                {
                                    temp = currentScreen["previousScreen"];
                                }*/
                
                BaseScreen temp = currentScreen.PreviousScreen;
                currentScreen = screens.GetScreenByName("InvalidInputScreen");
                currentScreen.PreviousScreen = temp;
                invalidInput = false;
            }

            string output = currentScreen.Text;

            if (currentScreen.GetType() == typeof(DisplayScreen))
            {
                DisplayScreen ds = (DisplayScreen)currentScreen;
                int counter = 0;

                foreach (var choice in ds.Choices)
                {
                    output += $"\n[{++counter}] {choice.Text}";
                }
            } else if (currentScreen.GetType() == typeof(FunctionScreen))
            {
                FunctionScreen fs = (FunctionScreen)currentScreen;

                if (fs.Functions.Count > 0)
                {
                    output += $"\n{fs.GetCurrentMessage()}";
                }
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
            else if (input == "1000")
            {
                IO.Reset_filesystem();
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