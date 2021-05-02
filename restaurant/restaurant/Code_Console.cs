using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace restaurant
{
    #region classes
    public class Choice
    {
        public const string SCREEN_NEXT = "SCREEN_NEXT", SCREEN_BACK = "SCREEN_BACK";

        public string ScreenName { get; set; }
        public string Text { get; set; }
        public string ScreenFlowDirection { get; set; }
        public bool ShowOnlyWhenLoggedIn { get; private set; }
        public bool ShowOnlyWhenLoggedOut { get; private set; }
        public Func<bool> DisplayCondition { get; private set; }

        public Choice(string screenName, string text, string screenDirection = SCREEN_NEXT, Func<bool> displayCondition = null)
        {
            ScreenName = screenName;
            Text = text;
            ScreenFlowDirection = screenDirection;
            DisplayCondition = displayCondition;
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
        public bool Repeat { get; private set; } = false;
        public bool NoOutput { get; private set;  } = false;
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

        public void SetRepeat(bool boolean)
        {
            Repeat = boolean;
        }

        public void SetNoOutput(bool boolean)
        {
            NoOutput = boolean;
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

    #endregion

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

        private readonly IO IO = new IO();

        private readonly restaurant.Testing_class TestingClass = new Testing_class();

        private readonly Screens screens = new Screens();

        private BaseScreen currentScreen;

        private bool invalidInput = false;

        private bool userLoggedIn = false;

        private Login_gegevens currentUser = null;

        private string input = "0";

        private string GFLogo = @" _____                     _  ______         _             
|  __ \                   | | |  ___|       (_)            
| |  \/_ __ __ _ _ __   __| | | |_ _   _ ___ _  ___  _ __  
| | __| '__/ _` | '_ \ / _` | |  _| | | / __| |/ _ \| '_ \ 
| |_\ \ | | (_| | | | | (_| | | | | |_| \__ \ | (_) | | | |
 \____/_|  \__,_|_| |_|\__,_| \_|  \__,_|___/_|\___/|_| |_|";

        private string BoxAroundString(string input, string sym, int spacing = 0)
        {
            int multipliedSpacing = spacing * 2;
            int BaseSpace = 2;

            string[] splitString = input.Split("\n");
            string[] arr = new string[splitString.Length + BaseSpace + multipliedSpacing];
            int longestStringLength = 0;

            for (int i = 0; i < splitString.Length; i++)
            {
                if (longestStringLength < splitString[i].Length)
                {
                    longestStringLength = splitString[i].Trim().Length;
                }
            }

            string row = "";

            for (int i = 0; i < longestStringLength + BaseSpace + multipliedSpacing; i++)
            {
                row += sym;
            }

            Func<int, string> makeLine = (length) =>
            {
                string temp = "";
                for (int j = 0; j < length; j++)
                {
                    temp += " ";
                }
                return temp;
            };

            Func<string, string> surroundLine = (line) =>
            {
                string sideLeft = makeLine(spacing), sideRight = sideLeft;

                return sym + sideLeft + line + sideRight + sym;
            };

            int counter = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                if (i == 0 || i == arr.Length - 1)
                {
                    arr[i] = row;
                }
                else if ((i > 0 && i <= spacing) || (i >= arr.Length - spacing - 1 && i < arr.Length))
                {
                    arr[i] = surroundLine(makeLine(longestStringLength));
                }
                else
                {
                    string line = splitString[counter].Trim();
                    arr[i] = surroundLine(line + makeLine(longestStringLength - line.Length));
                    counter++;
                }
            }

            return string.Join("\n", arr);
        }

        private string GerechtenToOutput(List<Gerechten> gerechten)
        {
            int SpacesBetweenColumn = 20;
            string MealsNameColumn = "Naam:";
            string MealsPriceColumn = "Prijs:";
            string temp = "";

            for (int i = 0; i < SpacesBetweenColumn - MealsNameColumn.Length; i++)
            {
                temp += " ";
            }

            string MealsOutput = $"{MealsNameColumn}{temp}{MealsPriceColumn}\n";

            int counter = 0;
            foreach (Gerechten item in gerechten)
            {
                string SpacesBetweenItems = "";

                for (int i = 0; i < SpacesBetweenColumn - item.naam.Length; i++)
                {
                    SpacesBetweenItems += " ";
                }

                MealsOutput += $"{item.naam}{SpacesBetweenItems}{item.prijs.ToString("F")} euro";
                counter++;

                if (counter != gerechten.Count)
                {
                    MealsOutput += "\n";
                }
            }

            return $"{GFLogo}\n" + BoxAroundString(MealsOutput, "#", 2);
        }

        private string ReviewsToOutput(List<Review> reviews)
        {
            string output = "Hier zijn alle reviews die zijn achtergelaten door onze klanten.\n";

            int counter = 0;
            foreach (Review review in reviews)
            {
                output += $"Klantnummer: {review.Klantnummer}     Review: {review.message}     Rating: {review.Rating}\n";
                counter++;

                if (counter > 20) break;
            }

            return $"{GFLogo}\n" + output;
        }

        public Code_Console()
        {
            Func<bool> isGebruiker = () => userLoggedIn && currentUser != null && currentUser.type == "Gebruiker";
            Func<bool> isMedewerker = () => userLoggedIn && currentUser != null && currentUser.type == "Medewerker";
            Func<bool> isEigenaar = () => userLoggedIn && currentUser != null && currentUser.type == "Eigenaar";

            #region startScreens
            DisplayScreen startScreen = new DisplayScreen("StartMenu", $"{GFLogo}\nKies een optie:");
            startScreen.Choices.Add(new Choice("StartScreenCustomer", "Klant"));
            startScreen.Choices.Add(new Choice("StartScreenEmployee", "Medewerker", Choice.SCREEN_NEXT, isMedewerker));
            startScreen.Choices.Add(new Choice("StartScreenOwner", "Eigenaar", Choice.SCREEN_NEXT, isEigenaar));
            startScreen.Choices.Add(new Choice("LoginScreenEmployee", "Inloggen", Choice.SCREEN_NEXT, () => !userLoggedIn));
            startScreen.Choices.Add(new Choice("LogoutScreen", "Uitloggen", Choice.SCREEN_NEXT, () => userLoggedIn));

            DisplayScreen startScreenCustomer = new DisplayScreen("StartScreenCustomer", $"{GFLogo}\nWelkom bij het klanten scherm.");
            startScreenCustomer.Choices.Add(new Choice("AllMeals", "Laat alle gerechten zien"));
            startScreenCustomer.Choices.Add(new Choice("AllReviews", "Laat alle reviews zien"));
            startScreenCustomer.Choices.Add(new Choice("CreateReview", "Maak een review aan", Choice.SCREEN_NEXT, isGebruiker));
            startScreenCustomer.Choices.Add(new Choice("EditReview", "Bewerk een review", Choice.SCREEN_NEXT, isGebruiker));
            startScreenCustomer.Choices.Add(new Choice("RemoveReview", "Verwijder een review", Choice.SCREEN_NEXT, isGebruiker));
            startScreenCustomer.Choices.Add(new Choice("UserReviews", "Bekijk uw eigen reviews", Choice.SCREEN_NEXT, isGebruiker));
            startScreenCustomer.Choices.Add(new Choice("StartMenu", "Ga terug", Choice.SCREEN_BACK));

            DisplayScreen startScreenEmployee = new DisplayScreen("StartScreenEmployee", $"{GFLogo}\nWelkom bij het medewerkers scherm.");
            startScreenEmployee.Choices.Add(new Choice("StartMenu", "Ga terug", Choice.SCREEN_BACK));

            DisplayScreen startScreenOwner = new DisplayScreen("StartScreenOwner", $"{GFLogo}\nWelkom bij het eigenaars Scherm");
            startScreenOwner.Choices.Add(new Choice("StartMenu", "Ga terug", Choice.SCREEN_BACK));
            #endregion

            #region userScreens
            DisplayScreen allMeals = new DisplayScreen("AllMeals", GerechtenToOutput(TestingClass.Get_standard_dishes()));
            allMeals.Choices.Add(new Choice("", "Ga terug", Choice.SCREEN_BACK));

            DisplayScreen allReviews = new DisplayScreen("AllReviews", ReviewsToOutput(Code_Gebruiker.getReviews()));
            allReviews.Choices.Add(new Choice("", "Ga terug", Choice.SCREEN_BACK));

            FunctionScreen createReview = new FunctionScreen("CreateReview", $"{GFLogo}\nLaat een review achter!");
            createReview.AddFunctionWithMessage(input =>
            {
                createReview.Variables.Add("customerData", currentUser.klantgegevens);
                createReview.Variables.Add("reservationData", Code_Gebruiker.getCustomerReservation(currentUser.klantgegevens));
                createReview.Variables.Add("anonymous", input.ToLower() == "ja" ? true : false);
                return null;
            }, "Wilt u dat de review liever anoniem blijft? Type 'ja' of 'nee'");
            createReview.AddFunctionWithMessage(input =>
            {
                createReview.Variables.Add("rating", int.Parse(input));
                return null;
            }, "Hoe was uw ervaring bij GrandeFusion van 1 tot 5?");
            createReview.AddFunctionWithMessage(input =>
            {
                Code_Gebruiker.makeReview(
                    createReview.Variables["rating"],
                    createReview.Variables["customerData"],
                    input,
                    createReview.Variables["reservationData"],
                    createReview.Variables["anonymous"]
                );

                DisplayScreen loginSuccesfullScreen = new DisplayScreen(
                    "",
                    "Bedankt voor uw review!."
                );

                loginSuccesfullScreen.Choices.Add(new Choice("", "Ga terug", Choice.SCREEN_BACK));

                BaseScreen previousScreen = currentScreen.PreviousScreen;
                currentScreen = loginSuccesfullScreen;
                currentScreen.PreviousScreen = previousScreen;

                return FunctionScreen.FINISHED;
            }, "Hier kunt u nog een bericht achterlaten.");

            FunctionScreen editReview = new FunctionScreen("EditReview", $"{GFLogo}\nBewerk een review");
            editReview.AddFunctionWithMessage(input =>
            {
                createReview.Variables.Add("reviewId", int.Parse(input));
                createReview.Variables.Add("customerData", currentUser.klantgegevens);
                return null;
            }, "Type in de ID van de review die u wilt bewerken");
            editReview.AddFunctionWithMessage(input =>
            {
                createReview.Variables.Add("anonymous", input.ToLower() == "ja" ? true : false);
                return null;
            }, "Wilt u dat de review liever anoniem blijft? Type 'ja' of 'nee'");
            editReview.AddFunctionWithMessage(input =>
            {
                createReview.Variables.Add("rating", int.Parse(input));
                return null;
            }, "Hoe was uw ervaring bij GrandeFusion van 1 tot 5?");
            editReview.AddFunctionWithMessage(input =>
            {
                Code_Gebruiker.overwriteReview(
                    createReview.Variables["reviewId"],
                    createReview.Variables["rating"],
                    createReview.Variables["customerData"],
                    input,
                    createReview.Variables["anonymous"]
                );

                DisplayScreen resultScreen = new DisplayScreen(
                    "",
                    "Uw review is aangepast."
                );

                resultScreen.Choices.Add(new Choice("", "Ga terug", Choice.SCREEN_BACK));

                BaseScreen previousScreen = currentScreen.PreviousScreen;
                currentScreen = resultScreen;
                currentScreen.PreviousScreen = previousScreen;

                return FunctionScreen.FINISHED;
            }, "Hier kunt u nog een bericht achterlaten.");

            FunctionScreen deleteReview = new FunctionScreen("RemoveReview", $"{GFLogo}\nVerwijder een review");
            deleteReview.AddFunctionWithMessage(input =>
            {
                Code_Gebruiker.deleteReview(
                    int.Parse(input),
                    currentUser.klantgegevens
                );

                DisplayScreen resultScreen = new DisplayScreen(
                    "",
                    "Uw review is verwijderd."
                );

                resultScreen.Choices.Add(new Choice("", "Ga terug", Choice.SCREEN_BACK));

                BaseScreen previousScreen = currentScreen.PreviousScreen;
                currentScreen = resultScreen;
                currentScreen.PreviousScreen = previousScreen;

                return FunctionScreen.FINISHED;
            }, "Wat is de ID van de review dat u wil verwijderen?");
            #endregion

            #region miscScreens

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
                        "De account met de gespecificeerde mail of wachtwoord is niet juist."
                    );
                    noAccountFoundScreen.Choices.Add(new Choice("StartMenu", "Ga terug", Choice.SCREEN_BACK));

                    currentScreen = noAccountFoundScreen;
                }
                else
                {
                    DisplayScreen loginSuccesfullScreen = new DisplayScreen(
                        "",
                        "Login succesvol."
                    );

                    string typeScreen = "";

                    if (funcResult.type == "Medewerker")
                    {
                        typeScreen = "StartScreenEmployee";
                    }
                    else if (funcResult.type == "Eigenaar")
                    {
                        typeScreen = "StartScreenOwner";
                    }
                    else
                    {
                        typeScreen = "StartScreenCustomer";
                    }

                    loginSuccesfullScreen.Choices.Add(new Choice(typeScreen, "Ga door"));

                    currentScreen = loginSuccesfullScreen;
                    currentScreen.PreviousScreen = previousScreen;

                    userLoggedIn = true;
                    currentUser = funcResult;
                }

                return FunctionScreen.FINISHED;
            }, "Je wachtwoord");

            FunctionScreen logoutScreen = new FunctionScreen("LogoutScreen", "");
            logoutScreen.AddFunction(input =>
            {
                userLoggedIn = false;
                currentUser = null;
                return FunctionScreen.FINISHED;
            });
            logoutScreen.SetNoOutput(true);
            #endregion

            screens.AllScreens.Add(logoutScreen);
            screens.AllScreens.Add(loginScreen);
            screens.AllScreens.Add(startScreenCustomer);
            screens.AllScreens.Add(createReview);
            screens.AllScreens.Add(editReview);
            screens.AllScreens.Add(deleteReview);
            screens.AllScreens.Add(startScreenEmployee);
            screens.AllScreens.Add(startScreenOwner);
            screens.AllScreens.Add(startScreen);
            screens.AllScreens.Add(allMeals);
            screens.AllScreens.Add(allReviews);
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

                    // TODO: this is done to represent the choices that are actually displayed on the screen
                    // Integrate this in the displayScreen class as this is more temporary
                    List<Choice> tempChoicesList = new List<Choice>();

                    foreach (Choice choice in choices)
                    {
                        if (choice.DisplayCondition != null)
                        {
                            if (choice.DisplayCondition()) tempChoicesList.Add(choice);
                        }
                        else
                        {
                            tempChoicesList.Add(choice);
                        }
                    }

                    Choice selectedChoice = tempChoicesList[inputAsInteger - 1];

                    if (selectedChoice.ScreenName == screen.Name)
                    {
                        if (screen.GetType() == typeof(FunctionScreen) && ((FunctionScreen)screen).NoOutput)
                        {
                            ((FunctionScreen)screen).GetCurrentFunction()("");
                            break;
                        }

                        if (currentScreen.Name != "InvalidInputScreen") 
                        {
                            screen.PreviousScreen = currentScreen;
                        }

                        currentScreen = screen;
                        break;
                    } 
                    else if (selectedChoice.ScreenName == "" && selectedChoice.ScreenFlowDirection == Choice.SCREEN_BACK)
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
                            "De huidige actie is geannuleerd."
                        );
                        functionCancelledScreen.Choices.Add(new Choice("", "Ga terug", Choice.SCREEN_BACK));

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
                } else if (!funcScreen.IsLastStep() && !funcScreen.Repeat) funcScreen.NextFunction();
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

                if (currentScreen.Name != "InvalidInputScreen")
                {
                    currentScreen = screens.GetScreenByName("InvalidInputScreen");
                    currentScreen.PreviousScreen = previousScreen;
                }

                invalidInput = false;
            }

            string output = currentScreen.Text;

            if (currentScreen.GetType() == typeof(DisplayScreen))
            {
                DisplayScreen ds = (DisplayScreen)currentScreen;
                int counter = 0;

                foreach (Choice choice in ds.Choices)
                {
                    if (choice.DisplayCondition != null)
                    {
                        if (choice.DisplayCondition()) output += $"\n[{++counter}] {choice.Text}";
                    }
                    else
                    {
                        output += $"\n[{++counter}] {choice.Text}";
                    }
                }
            } else if (currentScreen.GetType() == typeof(FunctionScreen))
            {
                FunctionScreen fs = (FunctionScreen)currentScreen;

                if (fs.Text.Trim() != "" && fs.Functions.Count > 0)
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
            else if (input == "105")
            {
                IO.Reset_filesystem();

                Login_gegevens dataEigenaar = new Login_gegevens();
                dataEigenaar.email = "eigenaarr@gmail.com";
                dataEigenaar.password = "rU3#)J2A8$E";
                dataEigenaar.type = "Eigenaar";

                Login_gegevens dataMedewerker = new Login_gegevens();
                dataMedewerker.email = "medewerker@gmail.com";
                dataMedewerker.password = "rU3#)J2A8$E";
                dataMedewerker.type = "Medewerker";

                Login_gegevens dataGebruiker = new Login_gegevens();
                dataGebruiker.email = "gebruiker@gmail.com";
                dataGebruiker.password = "rU3#)J2A8$E";
                dataGebruiker.type = "Gebruiker";
                dataGebruiker.klantgegevens = new Klantgegevens();
                dataGebruiker.klantgegevens.klantnummer = 1;
                dataGebruiker.klantgegevens.voornaam = "Bob";
                dataGebruiker.klantgegevens.achternaam = "De Boer";

                DateTime resDate = new DateTime(2050, 3, 1, 7, 0, 0);

                var list = new List<int>();
                list.Add(dataGebruiker.klantgegevens.klantnummer);

                Code_login.Register(dataEigenaar);
                Code_login.Register(dataMedewerker);
                Code_login.Register(dataGebruiker);
                Code_Gebruiker.MakeCustomerReservation(resDate, list, 1, false);
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