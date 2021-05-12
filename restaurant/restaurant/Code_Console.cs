using System;
using System.Collections.Generic;
using System.Linq;

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
            screens.Add(new ClientMenuScreen());
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


    public abstract class Screen
    {
        protected readonly Code_Medewerker_menu code_medewerker = new Code_Medewerker_menu();
        protected readonly Code_Gebruiker_menu code_gebruiker = new Code_Gebruiker_menu();
        protected readonly Code_Eigenaar_menu code_eigenaar = new Code_Eigenaar_menu();
        protected readonly Code_Login_menu code_login = new Code_Login_menu();
        protected readonly IO io = new IO();
        protected readonly Testing_class testing_class = new Testing_class();
        public Login_gegevens ingelogd = new Login_gegevens();

        protected const string GFLogo = @" _____                     _  ______         _         
|  __ \                   | | |  ___|       (_)            
| |  \/_ __ __ _ _ __   __| | | |_ _   _ ___ _  ___  _ __  
| | __| '__/ _` | '_ \ / _` | |  _| | | / __| |/ _ \| '_ \ 
| |_\ \ | | (_| | | | | (_| | | | | |_| \__ \ | (_) | | | |
 \____/_|  \__,_|_| |_|\__,_| \_|  \__,_|___/_|\___/|_| |_|";

        protected string BoxAroundText(List<string> input, string sym, int spacingside, int spacingtop, int maxlength, bool openbottom)
        {
            string output = new string(Convert.ToChar(sym), maxlength + 2 + spacingside * 2) + "\n";
            for (int a = 0; a < spacingtop; a++)
            {
                output += sym + new string(' ', maxlength + spacingside * 2) + sym + "\n";
            }

            foreach (var line in input)
            {
                output += sym + new string(' ', spacingside) + line + new string(' ', spacingside) + sym + "\n";
            }

            for (int a = 0; a < spacingtop; a++)
            {
                output += sym + new string(' ', maxlength + spacingside * 2) + sym + "\n";
            }

            if (openbottom)
            {
                return output;
            }
            return output += new string(Convert.ToChar(sym), maxlength + 2 + spacingside * 2) + "\n";
        }

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

        public string GetGFLogo() => ingelogd.type == null || ingelogd.type == "No account found" ? GFLogo + "\n" : GFLogo + $"\nU bent nu ingelogd als {ingelogd.klantgegevens.voornaam} {ingelogd.klantgegevens.achternaam}\n[4] Log uit\n";
    }

    public class StartScreen : Screen
    {
        public override int DoWork()
        {
            Console.WriteLine(GetGFLogo());
            Console.WriteLine("Kies een optie:");
            Console.WriteLine("[1] Laat alle gerechten zien");
            Console.WriteLine("[2] Laat alle reviews zien");
            if (ingelogd.type == null || ingelogd.type == "No account found")
            {
                Console.WriteLine("[3] Registreer");
                Console.WriteLine("[4] Login");
            }
            else
            {
                Console.WriteLine("[3] Klant menu");
            }

            string choise = Console.ReadLine();

            if (choise != "0" && choise != "1" && choise != "2" && choise != "3" && choise != "4" && choise != "100" && choise != "101" && choise != "102" && choise != "103" && choise != "104" && choise != "1000")
            {
                Console.WriteLine("U moet wel een juiste keuze maken...");
                Console.WriteLine("Druk op en knop om verder te gaan.");
                Console.ReadKey();
                return 0;
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
                        if (ingelogd.type == null || ingelogd.type == "No account found")
                        {
                            return 3;
                        }
                        else
                        {
                            return 5;
                        }
                    case 4:
                        if (ingelogd.type == null || ingelogd.type == "No account found")
                        {
                            return 4;
                        }
                        else
                        {
                            ingelogd = new Login_gegevens();
                            Console.WriteLine("U bent nu uitgelogd.");
                            Console.WriteLine("Druk op een knop om verder te gaan.");
                            Console.ReadKey();
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
                    case 1000:
                        io.ResetFilesystem();
                        return 0;
                }
            }

            return 0;
        }

        public override List<Screen> Update(List<Screen> screens)
        {


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
            Console.WriteLine(GFLogo);
            Console.WriteLine(gerechtenbox);
            Console.WriteLine("[1] Ga terug");

            string choise = Console.ReadLine();
            if (choise != "1")
            {
                Console.WriteLine("\n");
                Console.WriteLine("U moet wel een juiste keuze maken...");
                Console.WriteLine("Druk op en knop om verder te gaan.");
                Console.ReadKey();
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public override List<Screen> Update(List<Screen> screens)
        {


            return screens;
        }
    }

    public class ReviewScreen : Screen
    {
        private string reviewstostring;
        public ReviewScreen()
        {
            List<Review> reviews = io.GetReviews();
            if (reviews.Count > 0)
            {
                reviewstostring = ReviewsToString(io.GetReviews());
            }
        }

        private string ReviewsToString(List<Review> reviews)
        {
            List<Klantgegevens> klantgegevens = io.GetCustomer(reviews.Select(i => i.Klantnummer).ToList());
            List<string> block = new List<string>();
            string output = "";

            for (int a = 0; a < reviews.Count; a += 2)
            {
                block.Add(new string(' ', 50) + "##" + new string(' ', 50));
                block.Add(new string(' ', 50) + "##" + new string(' ', 50));
                if (reviews[a].annomeme && reviews[a + 1].annomeme)
                {
                    block.Add("Anoniem: " + new string(' ', 50 - ("Anoniem: ").Length) + "##  " +
                            "Anoniem: " + new string(' ', 48 - ("Anoniem: ").Length));
                    block.Add(new string(' ', 50) + "##" + new string(' ', 50));
                }
                else if (reviews[a].annomeme && !reviews[a + 1].annomeme)
                {
                    block.Add("Anoniem: " + new string(' ', 50 - ("Anoniem: ").Length) + "##  " +
                            "Voornaam: " + klantgegevens[a + 1].voornaam + new string(' ', 48 - ("Voornaam: " + klantgegevens[a + 1].voornaam).Length));
                    block.Add(new string(' ', 50) + "##  " +
                        "Achternaam: " + klantgegevens[a + 1].achternaam + new string(' ', 48 - ("Achternaam: " + klantgegevens[a + 1].achternaam).Length));
                }
                else if (!reviews[a].annomeme && reviews[a + 1].annomeme)
                {
                    block.Add("Voornaam: " + klantgegevens[a].voornaam + new string(' ', 50 - ("Voornaam: " + klantgegevens[a].voornaam).Length) + "##  " +
                        "Anoniem: " + new string(' ', 48 - ("Anoniem: ").Length));
                    block.Add("Achternaam: " + klantgegevens[a].achternaam + new string(' ', 50 - ("Achternaam: " + klantgegevens[a].achternaam).Length) + "##  " +
                        new string(' ', 48));
                }
                else
                {
                    block.Add("Voornaam: " + klantgegevens[a].voornaam + new string(' ', 50 - ("Voornaam: " + klantgegevens[a].voornaam).Length) + "##  " +
                                        "Voornaam: " + klantgegevens[a + 1].voornaam + new string(' ', 48 - ("Voornaam: " + klantgegevens[a + 1].voornaam).Length));
                    block.Add("Achternaam: " + klantgegevens[a].achternaam + new string(' ', 50 - ("Achternaam: " + klantgegevens[a].achternaam).Length) + "##  " +
                        "Achternaam: " + klantgegevens[a + 1].achternaam + new string(' ', 48 - ("Achternaam: " + klantgegevens[a + 1].achternaam).Length));
                }

                block.Add("Review: " + new string(' ', 50 - ("Review: ").Length) + "##  " +
                    "Review: " + new string(' ', 48 - ("Review: ").Length));
                block.Add("Rating: " + reviews[a].Rating + new string(' ', 50 - ("Rating: " + reviews[a].Rating).Length) + "##  " +
                    "Rating: " + reviews[a + 1].Rating + new string(' ', 48 - ("Rating: " + reviews[a + 1].Rating).Length));
                block.Add("Datum: " + reviews[a].datum + new string(' ', 50 - ("Datum: " + reviews[a].datum).Length) + "##  " +
                        "Datum: " + reviews[a + 1].datum + new string(' ', 48 - ("Datum: " + reviews[a + 1].datum).Length));
                block.Add(new string(' ', 50) + "##" + new string(' ', 50));
                block.Add(new string(' ', 50) + "##" + new string(' ', 50));

                output += BoxAroundText(block, "#", 2, 0, 102, true);
                block = new List<string>();
            }

            output += new string('#', 108);

            return output;
        }

        public override int DoWork()
        {
            Console.WriteLine(GFLogo);
            Console.WriteLine("Dit zijn alle reviews die zijn achtergelaten door onze klanten: \n");
            Console.WriteLine(reviewstostring + "\n" + "[1] Ga terug");
            string choise = Console.ReadLine();

            if (choise != "1")
            {
                Console.WriteLine("\n");
                Console.WriteLine("U moet wel een juiste keuze maken...");
                Console.WriteLine("Druk op een knop om verder te gaan.");
                Console.ReadKey();
                return 2;
            }
            else
            {
                return 0;
            }
        }

        public override List<Screen> Update(List<Screen> screens)
        {


            return screens;
        }
    }

    public class RegisterScreen : Screen
    {
        public RegisterScreen()
        {

        }

        public override int DoWork()
        {
            List<Login_gegevens> login_Gegevens = io.GetDatabase().login_gegevens;
            Login_gegevens new_gebruiker = new Login_gegevens();
            Console.WriteLine(GFLogo);
            Console.WriteLine("Hier kunt u een account aanmaken om o.a. reververingen te plaatsen voor GrandFusion!" + "\n");
            Console.WriteLine("Uw voornaam: ");
            new_gebruiker.klantgegevens = new Klantgegevens
            {
                voornaam = Console.ReadLine(),
                klantnummer = login_Gegevens[login_Gegevens.Count - 1].klantgegevens.klantnummer + 1
            };
            Console.WriteLine("Uw achternaam: ");
            new_gebruiker.klantgegevens.achternaam = Console.ReadLine();
            Console.WriteLine("Uw geboorte datum met format 1-1-2000: ");
            new_gebruiker.klantgegevens.geb_datum = Convert.ToDateTime(Console.ReadLine());
            Console.WriteLine("Hieronder vult u uw adres in. Dit is in verband met het bestellen van eten. \n");
            Console.WriteLine("Uw woonplaats: ");
            new_gebruiker.klantgegevens.adres = new adres
            {
                woonplaats = Console.ReadLine(),
                land = "NL"
            };
            Console.WriteLine("Uw postcode: ");
            new_gebruiker.klantgegevens.adres.postcode = Console.ReadLine();
            Console.WriteLine("Uw straatnaam: ");
            new_gebruiker.klantgegevens.adres.straatnaam = Console.ReadLine();
            Console.WriteLine("Uw huisnummer: ");
            new_gebruiker.klantgegevens.adres.huisnummer = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("\n Hieronder vult u uw login gegevens: ");
            Console.WriteLine("Uw email adres: ");
            new_gebruiker.email = Console.ReadLine();
            Console.WriteLine("Het wachtwoord voor uw account: ");
            new_gebruiker.password = Console.ReadLine();
            new_gebruiker.type = "Gebruiker";
            Console.WriteLine("\n Kloppen de bovenstaande gegevens?");
            Console.WriteLine("[1] Deze kloppen niet, breng me terug.");
            Console.WriteLine("[2] ja deze kloppen.");

            string choise = Console.ReadLine();

            if (choise != "1" && choise != "2")
            {
                Console.WriteLine("\n");
                Console.WriteLine("U moet wel een juiste keuze maken...");
                Console.WriteLine("Druk op en knop om verder te gaan.");
                Console.ReadKey();
                return 3;
            }
            else if (choise == "1")
            {
                return 3;
            }

        a:
            if (code_login.Register(new_gebruiker) == "Succes!")
            {
                Console.WriteLine("U bent succesfull geregistreerd!");
                Console.WriteLine("Druk op en knop om naar het hoofdmenu te gaan");
                Console.ReadKey();
                return 0;
            }
            else if (code_login.Register(new_gebruiker) == "This email and account type is already in use")
            {
                Console.WriteLine("Dit account bestaat al, probeer een ander email adres:");
                new_gebruiker.email = Console.ReadLine();
                goto a;
            }
            else if (code_login.Register(new_gebruiker) == "Password must contain at least 8 characters, 1 punctuation mark and 1 number.")
            {
                Console.WriteLine("Het wachtwoord moet minimaal 8 tekens, 1 leesteken en 1 nummer bevatten.");
                Console.WriteLine("Het wachtwoord voor uw account: ");
                new_gebruiker.password = Console.ReadLine();
                goto a;
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
            Console.WriteLine(GFLogo);
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
                return 0;
            }
        }

        public override List<Screen> Update(List<Screen> screens)
        {
            foreach (var screen in screens)
            {
                screen.ingelogd = this.ingelogd;
            }

            return screens;
        }
    }

    public class ClientMenuScreen : Screen
    {
        bool uitloggen = false;
        public ClientMenuScreen()
        {

        }

        public override int DoWork()
        {
            Console.WriteLine(GFLogo);
            Console.WriteLine("Welkom in het klanten menu.");
            Console.WriteLine("[1] Maak een reservering aan");
            Console.WriteLine("[2] Maak een review aan");
            Console.WriteLine("[3] Bewerk een review");
            Console.WriteLine("[4] Verwijder een review");
            Console.WriteLine("[5] Bekijk uw eigen reviews");
            Console.WriteLine("[6] Ga terug");
            Console.WriteLine("[7] Uitloggen");

            string choise = Console.ReadLine();

            if (choise != "1" && choise != "2" && choise != "3" && choise != "4" && choise != "5" && choise != "6" && choise != "7")
            {
                Console.WriteLine("U moet wel een juiste keuze maken...");
                Console.WriteLine("Druk op en knop om verder te gaan.");
                Console.ReadKey();
                return 5;
            }
            switch (choise)
            {
                case "1":
                    return 6;
                case "2":
                    return 7;
                case "3":
                    return 8;
                case "4":
                    return 9;
                case "5":
                    return 10;
                case "6":
                    return 0;
                case "7":
                    uitloggen = true;
                    return 0;
            }


            return 5;
        }

        public override List<Screen> Update(List<Screen> screens)
        {
            if (uitloggen)
            {
                foreach (var screen in screens)
                {
                    screen.ingelogd = new Login_gegevens();
                }
            }
            return screens;
        }
    }
}
