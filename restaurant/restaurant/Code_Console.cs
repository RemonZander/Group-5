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
    * Screens:
    *   id: int
    *   output: string,
    *   choices: List<int>
    */
    public class Code_Console
    {
        private readonly Code_Login_menu Code_login = new Code_Login_menu();

        private readonly restaurant.Code_Eigenaar_menu Code_Eigenaar = new Code_Eigenaar_menu();

        private readonly restaurant.Code_Medewerker_menu Code_Medewerker = new Code_Medewerker_menu();

        private readonly restaurant.Code_Gebruiker_menu Code_Gebruiker = new Code_Gebruiker_menu();

        private Dictionary<string, dynamic> currentScreen;

        private readonly List<Dictionary<string, dynamic>> screens = new List<Dictionary<string, dynamic>>();

        private bool invalidInput = false;

        private string input = "0";

        private int screenIds = 0;

        public Code_Console()
        {
            screens.Add(StartMenu());
            screens.Add(TestScreen());
            screens.Add(HelloScreen());

            currentScreen = screens[0];
        }

        /*
         * Creates a dict with string keys and different type of values.
         * id: int
         * output: string
         * choices: List<int> holds the id's
         */
        private Dictionary<string, dynamic> CreateScreen()
        {
            screenIds++;

            var dict = new Dictionary<string, dynamic>();
            dict.Add("id", screenIds);
            dict.Add("output", "");
            dict.Add("choices", new List<int>());

            return dict;
        }

        private Dictionary<string, dynamic> StartMenu()
        {
            var dict = CreateScreen();
            dict["output"] = @" _____                     _  ______         _             
|  __ \                   | | |  ___|       (_)            
| |  \/_ __ __ _ _ __   __| | | |_ _   _ ___ _  ___  _ __  
| | __| '__/ _` | '_ \ / _` | |  _| | | / __| |/ _ \| '_ \ 
| |_\ \ | | (_| | | | | (_| | | | | |_| \__ \ | (_) | | | |
 \____/_|  \__,_|_| |_|\__,_| \_|  \__,_|___/_|\___/|_| |_|

Welkom bij de menu van GrandFusion!
[1] Mockup single meal
[2] Say Hello!";
            dict["choices"].Add(2);
            dict["choices"].Add(3);
            return dict;
        }

        private Dictionary<string, dynamic> TestScreen()
        {
            var dict = CreateScreen();
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
            dict["choices"].Add(1);
            return dict;
        }

        private Dictionary<string, dynamic> HelloScreen()
        {
            var dict = CreateScreen();
            dict["output"] = "Hello!\n[1] Go back";
            dict["choices"].Add(1);
            return dict;
        }

        private Dictionary<string, dynamic> InvalidInputScreen()
        {
            var dict = CreateScreen();
            dict["output"] = "Please type a valid choice.\nValid choices are the ones marked with -> [] with a number inside it.\nDon't type your choice with the brackets (These things -> [])\nExample: When you see this option -> [1] you press the number: 1 and then click on enter\n[1] Go back";
            dict["choices"].Add(1);
            return dict;
        }

        private void ScreenManager(int input)
        {
            // Choice represents a screen ID
            int choice = currentScreen["choices"][input - 1];

            // for every screen match the id with choice then get that screen and set it as current screen.
            foreach (var screen in screens)
            {
                if (screen["id"] == choice)
                {
                    currentScreen = screen;
                    break;
                }
            }
        }

        public void Display()
        {
            if (invalidInput)
            {
                currentScreen = InvalidInputScreen();
                invalidInput = false;
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