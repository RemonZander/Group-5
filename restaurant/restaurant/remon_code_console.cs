using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace restaurant
{
    class remon_code_console
    {
        int CurrentScreen;
        List<dynamic> screens = new List<dynamic>();
        public remon_code_console()
        {
            screens.Add(new StartScreen());
            screens.Add(new GerechtenScreen());
            CurrentScreen = 0;
        }

        public void Display()
        {
            CurrentScreen = screens[CurrentScreen].DoWork();
        }

        public void Refresh()
        {
            Console.Clear();
        }
    }


    public abstract class screens
    {
        protected const string GFLogo = @" _____                     _  ______         _         
|  __ \                   | | |  ___|       (_)            
| |  \/_ __ __ _ _ __   __| | | |_ _   _ ___ _  ___  _ __  
| | __| '__/ _` | '_ \ / _` | |  _| | | / __| |/ _ \| '_ \ 
| |_\ \ | | (_| | | | | (_| | | | | |_| \__ \ | (_) | | | |
 \____/_|  \__,_|_| |_|\__,_| \_|  \__,_|___/_|\___/|_| |_|";


    }

    public class StartScreen : screens
    {
        public int DoWork()
        {
            Console.WriteLine(GFLogo);
            Console.WriteLine("Kies een optie:");
            Console.WriteLine("[1] Laat alle gerechten zien");
            Console.WriteLine("[2] Laat alle reviews zien");
            Console.WriteLine("[3] Registreer");
            Console.WriteLine("[4] Login");

            string choise = Console.ReadLine();

            if (choise != "0" && choise != "1" && choise != "2" && choise != "3" && choise != "4")
            {
                Console.WriteLine("U moet wel een juiste keuze maken...");
                Console.WriteLine("Druk op enter om verder te gaan.");
            }
            else
            {
                switch (Convert.ToInt32(choise))
                {
                    case 1:
                        return 1;
                    case 2:
                        return 2;
                    case 3:
                        return 3;
                    case 4:
                        return 4;
                }
            }

            return 0;
        }
    }

    public class GerechtenScreen : screens
    {
        public int DoWork()
        {
            Console.WriteLine(GFLogo);
            string choise = Console.ReadLine();

            return 1;
        }
    }
}
