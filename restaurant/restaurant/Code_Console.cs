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
            startScreenCustomer.Choices.Add(new Choice("ScreenMeals", "Gerechten Menu"));
            startScreenCustomer.Choices.Add(new Choice("StartMenu", "Ga terug", Choice.SCREEN_BACK));

            DisplayScreen startScreenEmployee = new DisplayScreen("StartScreenEmployee", $"{GFLogo}\nMedewerkers Scherm");
            startScreenEmployee.Choices.Add(new Choice("StartMenu", "Ga terug", Choice.SCREEN_BACK));

            DisplayScreen startScreenMeals = new DisplayScreen("ScreenMeals", $"{GFLogo}\nWelkom bij de gerechten menu van GRAND FUSION. Eet smakelijk!");
            startScreenMeals.Choices.Add(new Choice("AllMeals", "Laat alle gerechten zien"));
            startScreenMeals.Choices.Add(new Choice("StartScreenCustomer", "Ga terug", Choice.SCREEN_BACK));

            // Kan in een aparte functie
            List<Gerechten> MealsMenu = TestingClass.Get_standard_dishes();
            int SpacesBetweenColumn = 20;
            string MealsNameColumn = "Naam:";
            string MealsPriceColumn = "Price:";
            string temp = "";

            for (int i = 0; i < SpacesBetweenColumn - MealsNameColumn.Length; i++)
            {
                temp += " ";
            }

            string MealsOutput = $"{GFLogo}\n{MealsNameColumn}{temp}{MealsPriceColumn}\n";

            foreach (Gerechten item in MealsMenu)
            {
                string SpacesBetweenItems = "";

                for (int i = 0; i < SpacesBetweenColumn - item.naam.Length; i++)
                {
                    SpacesBetweenItems += " ";
                }

                MealsOutput += $"{item.naam}{SpacesBetweenItems}{item.prijs.ToString("F")} euro\n";
            }
            //

            DisplayScreen allMeals = new DisplayScreen("AllMeals", MealsOutput);
            allMeals.Choices.Add(new Choice("", "Ga terug", Choice.SCREEN_BACK));

            DisplayScreen invalidInputScreen = new DisplayScreen("InvalidInputScreen", "Type alstublieft de correcte keuze in.\nCorrecte keuzes zijn gemarkeerd met -> [] met een nummer erin.\nType alleen de nummer van de keuze in.\nVoorbeeld: Met keuze [1] type je in 1");
            invalidInputScreen.Choices.Add(new Choice("", "Ga terug", Choice.SCREEN_BACK));

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
            screens.AllScreens.Add(startScreenMeals);
            screens.AllScreens.Add(allMeals);
            screens.AllScreens.Add(invalidInputScreen);

            currentScreen = screens.CurrentScreen = startScreen;
        }

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
                    Choice choice = choices[inputAsInteger - 1];
                    if (choice.ScreenName == screen.Name)
                    {
                        if (currentScreen.Name != "InvalidInputScreen") 
                        {
                            screen.PreviousScreen = currentScreen;
                        }

                        currentScreen = screen;
                        break;
                    } 
                    else if (choice.ScreenName == "" && choice.ScreenFlowDirection == Choice.SCREEN_BACK)
                    {
                        currentScreen = displayScreen.PreviousScreen;
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
                BaseScreen previousScreen;
                if (currentScreen.GetType() == typeof(FunctionScreen))
                {
                    previousScreen = currentScreen.PreviousScreen;
                }
                else
                {
                    previousScreen = currentScreen;
                }

                currentScreen = screens.GetScreenByName("InvalidInputScreen");
                currentScreen.PreviousScreen = previousScreen;
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