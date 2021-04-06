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
            data.password = "123";
            data.type = "Eigenaar";

            Code_login.Register(data);

            screens.Add(StartScreenCustomer());
            screens.Add(StartScreenEmployee());
            screens.Add(ChoiceScreen());

            previousScreen = null;
            currentScreen = screens[screenNames.IndexOf("ChoiceScreen")];
        }

        /*
         * Creates a dict with string keys and different type of values.
         * id: int
         * name: string
         * output: string
         * choices: List<int> holds the id
         */
        private Dictionary<string, dynamic> CreateScreen(string name)
        {
            screenIds++;
            screenNames.Add(name);

            var dict = new Dictionary<string, dynamic>();
            dict.Add("id", screenIds);
            dict.Add("output", "");
            dict.Add("choices", new Dictionary<int, string>());

            return dict;
        }

        private Dictionary<string, dynamic> ChoiceScreen()
        {
            var dict = CreateScreen("ChoiceScreen");
            dict["output"] = "Kies een rol:";
            dict["choices"].Add(screenNames.IndexOf("StartScreenCustomer"), "Customer");
            dict["choices"].Add(screenNames.IndexOf("StartScreenEmployee"), "Employee");
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
            var dict = CreateScreen("LoginScreen");
            dict["output"] = $"{GFLogo}\nMedewerkers Scherm";
            return dict;
        }

        private Dictionary<string, dynamic> TestScreen()
        {
            var dict = CreateScreen("TestScreen");
            dict["output"] = @"Hello! You are now at the test screen.
Here you will see an example of the details of a single meal from a menu.
#############################
# Sushi Pizza               #
# 4.5 / 5.0 Stars           #
#                           #
# Ingredients:              #
# Fish                      #
# Rice                      #
# Dough                     #
# Tomato                    #
# Cheese                    #
#                           #
# Allergies:                #
# Lorem                     #
# Ipsum                     #
# Lorem                     #
# Dorem                     #
# Bipsum                    #
#                           #
# Extra options:            #
# Wasabi on pizza (HOT)     #
# Soy Sauce on the side     #
#############################
[1] Go back";
            dict["choices"].Add(screenNames.IndexOf("StartScreenCustomer"));
            return dict;
        }

        private Dictionary<string, dynamic> InvalidInputScreen()
        {
            var dict = CreateScreen("InvalidInputScreen");
            dict["output"] = "Please type a valid choice.\nValid choices are the ones marked with -> [] with a number inside it.\nDon't type your choice with the brackets (These things -> [])\nExample: When you see this option -> [1] you press the number: 1 and then click on enter";
            dict["choices"].Add(previousScreen["id"], "Go back");
            return dict;
        }

        private void ScreenManager(int input)
        {
            // Choice represents a screen ID
            Dictionary<int, string> choices = currentScreen["choices"];

            // for every screen match the id with choice then get that screen and set it as current screen.
            foreach (var screen in screens)
            {
                if (choices.ContainsKey(screen["id"]))
                {
                    previousScreen = currentScreen;
                    currentScreen = screen;
                    break;
                }
            }
        }

        public void Display()
        {
            if (invalidInput)
            {
                previousScreen = currentScreen;
                currentScreen = InvalidInputScreen();
                invalidInput = false;
            }
            
            foreach (KeyValuePair<int, string> choice in currentScreen["choices"])
            {
                currentScreen["output"] += $"\n[{choice.Key + 1}] {choice.Value}";
            }

            Console.WriteLine(currentScreen["output"]);

            input = Console.ReadLine();

            if (input != null && int.TryParse(input, out _))
            {
                int inputAsInteger = Convert.ToInt32(input);

                if (inputAsInteger == 100)
                {
                    Code_Gebruiker.Debug();
                }
                else if (inputAsInteger == 101)
                {
                    Code_Eigenaar.Debug();
                }
                else if (inputAsInteger == 102)
                {
                    // Code_login.Debug();
                }
                else if (inputAsInteger == 103)
                {
                    // Code_Medewerker.Debug();
                }
                else if (inputAsInteger == 104)
                {
                    TestingClass.Debug();
                }
                else if (inputAsInteger > currentScreen["choices"].Count || inputAsInteger < 1)
                {
                    invalidInput = true;
                }
                else
                {
                    // Proceed
                    ScreenManager(inputAsInteger);
                }
            } else {
                invalidInput = true;
            }
        }

        public void Refresh()
        {
            Console.Clear();
        }
    }
}