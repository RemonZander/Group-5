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
        private Code_Login_menu Code_login;

        private restaurant.Code_Eigenaar_menu Code_Eigenaar;

        private restaurant.Code_Medewerker_menu Code_Medewerker;

        private restaurant.Code_Gebruiker_menu Code_Gebruiker;

        private Dictionary<string, dynamic> currentScreen;

        private readonly List<Dictionary<string, dynamic>> screens = new List<Dictionary<string, dynamic>>();

        private string input = "0";

        private bool firstInit = true;

        private int screenIds = 0;

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
            dict["output"] = @"
 _____                     _  ______         _             
|  __ \                   | | |  ___|       (_)            
| |  \/_ __ __ _ _ __   __| | | |_ _   _ ___ _  ___  _ __  
| | __| '__/ _` | '_ \ / _` | |  _| | | / __| |/ _ \| '_ \ 
| |_\ \ | | (_| | | | | (_| | | | | |_| \__ \ | (_) | | | |
 \____/_|  \__,_|_| |_|\__,_| \_|  \__,_|___/_|\___/|_| |_|

Welkom bij de menu van GrandFusion!
[1] Test
[2] Say Hello!";
            dict["choices"].Add(2);
            dict["choices"].Add(3);
            return dict;
        }

        private Dictionary<string, dynamic> TestScreen()
        {
            var dict = CreateScreen();
            dict["output"] = "Hello! You are now at the test screen... it's pretty empty here.\n[1] Go back";
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
            if (firstInit)
            {
                screens.Add(StartMenu());
                screens.Add(TestScreen());
                screens.Add(HelloScreen());

                currentScreen = screens[0];
                firstInit = false;
            }

            Console.WriteLine(currentScreen["output"]);

            input = Console.ReadLine();

            if (input != null)
            {
                int inputAsInteger = Convert.ToInt32(input);

                if (!(inputAsInteger < currentScreen["choices"].Capacity - 1))
                {
                    Console.WriteLine("Please write a valid choice");
                }
                else
                {
                    // Proceed
                    ScreenManager(inputAsInteger);
                }
            }
        }

        public void Refresh()
        {
            Console.Clear();
        }
    }
}