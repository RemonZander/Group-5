using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace restaurant
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        //[STAThread]
        static void Main()
        {
            bool debug = true;

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (debug)
            {
                Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
                Code_Console code = new Code_Console();
                NativeMethods.AllocConsole();
                Console.OutputEncoding = System.Text.Encoding.UTF8;
                EmulateRestaurant();
                do
                {
                    // Here we incorporate our visualization of the application
                    // The code in the do while loop will be called every 100ms so every visualization should be refreshed
                    // If the user asks for input the thread should be paused
                    code.Display();
                    code.Refresh();
                } while (true);
            }
        }


        private static void EmulateRestaurant()
        {
            IO io = new IO();
            Testing_class testing_Class = new Testing_class();
            Database database = io.GetDatabase();
            Random rnd = new Random();

            if (database.reserveringen == null) return;
            for (int a = 0; a < database.reserveringen.Count; a++)
            {
                if (database.reserveringen[a].datum < DateTime.Now && database.reserveringen[a].tafels != null && database.reserveringen[a].gerechten_ID == null)
                {
                    int tries = 0;
                    List<Gerechten> gerechten = new List<Gerechten>();
                    List<Ingredient> ingredienten = new List<Ingredient>(database.ingredienten);
                    List<List<string>> neededIngredients = new List<List<string>>();
                    do
                    {
                        tries++;
                        gerechten = testing_Class.Make_dishes(database.reserveringen[a].aantal * 3);
                        neededIngredients = gerechten.Select(g => g.Ingredienten).Distinct().ToList();

                        foreach (var ingredientenList in neededIngredients)
                        {
                            List<string> ingredietNamen = ingredienten.Select(i => i.name).Distinct().ToList();
                            if (ingredientenList != null && !ingredietNamen.Any(item => ingredientenList.Contains(item)))
                            {
                                break;
                            }
                            else if (ingredientenList != null)
                            {
                                foreach (var ingredient in ingredientenList)
                                {
                                    int index = ingredienten.IndexOf(ingredienten.Where(i => i.name == ingredient).FirstOrDefault());
                                    ingredienten[index] = new Ingredient();
                                }
                            }
                        }
                    } while (tries < 10);

                    if (gerechten.Count == 0)
                    {
                        database.reserveringen[a] = new Reserveringen();
                        continue;
                    }

                    database.ingredienten = ingredienten;
                    Reserveringen temp = database.reserveringen[a];
                    temp.gerechten_ID = gerechten.Select(g => g.ID).ToList();
                    database.reserveringen[a] = temp;

                    Review review = new Review
                    {
                        ID = 0,
                        klantnummer = database.reserveringen[a].klantnummer,
                        reservering_ID = database.reserveringen[a].ID,
                        Rating = rnd.Next(1, 6),
                        datum = database.reserveringen[a].datum.AddDays(rnd.Next(0, 50))
                    };
                    if (database.reviews.Count == 0)
                    {
                        database.reviews = new List<Review> { review };
                    }
                    else
                    {
                        review.ID = database.reviews[database.reviews.Count - 1].ID + 1;
                    }
                    if (rnd.Next(5) == 3)
                    {
                        review.annomeme = true;
                        review.klantnummer = -1;
                        review.reservering_ID = -1;
                        review.datum = new DateTime();
                    }

                    switch (review.Rating)
                    {
                        case 1:
                            review.message = "Verschikkelijk restaurant, hier kom ik nooit meer! Wie die sushipizza heeft uitgevonden mag branden in hell!";
                            break;
                        case 2:
                            review.message = "De service was wel goed, maar het eten wat niet zo goed. Ik denk dat ik hier niet meer terug wil komen. Geen aanrader voor vrienden!";
                            break;
                        case 3:
                            review.message = "Niet goed, niet slecht. Eten is te doen. Service was prima, ik kom nog wel terug.";
                            break;
                        case 4:
                            review.message = "gewoon goed! niet meer te zeggen.";
                            break;
                        case 5:
                            review.message = "OMG, die sushipizza was amazing!!! Dit is het beste restaurant ever, nog nooit zo'n hipster restaurant gezien in mijn leven. Ik kom hier zeker terug!!!";
                            break;
                    }
                    database.reviews.Add(review);

                    Feedback feedback = new Feedback
                    {
                        ID = 0,
                        klantnummer = database.reserveringen[a].klantnummer,
                        reservering_ID = database.reserveringen[a].ID,
                        message = "test message",
                        recipient = database.werknemers[rnd.Next(0, database.werknemers.Count)].ID,
                        datum = database.reserveringen[a].datum.AddDays(rnd.Next(0, 50))
                    };

                    if (database.feedback.Count == 0)
                    {
                        database.feedback = new List<Feedback> { feedback };
                    }
                    else
                    {
                        feedback.ID = database.feedback[database.feedback.Count - 1].ID + 1;
                        database.feedback.Add(feedback);
                    }
                }
                else if (database.reserveringen[a].datum < DateTime.Now && (database.reserveringen[a].tafels == null || database.reserveringen[a].tafels.Count == 0) && (database.reserveringen[a].gerechten_ID == null || database.reserveringen[a].gerechten_ID.Count == 0))
                {
                    database.reserveringen[a] = new Reserveringen();
                }
            }

            io.Savedatabase(database);
        }
    }

    internal static class NativeMethods
    {
        // http://msdn.microsoft.com/en-us/library/ms681944(VS.85).aspx
        /// <summary>
        /// Allocates a new console for the calling process.
        /// </summary>
        /// <returns>nonzero if the function succeeds; otherwise, zero.</returns>
        /// <remarks>
        /// A process can be associated with only one console,
        /// so the function fails if the calling process already has a console.
        /// </remarks>
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int AllocConsole();

        // http://msdn.microsoft.com/en-us/library/ms683150(VS.85).aspx
        /// <summary>
        /// Detaches the calling process from its console.
        /// </summary>
        /// <returns>nonzero if the function succeeds; otherwise, zero.</returns>
        /// <remarks>
        /// If the calling process is not already attached to a console,
        /// the error code returned is ERROR_INVALID_PARAMETER (87).
        /// </remarks>
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int FreeConsole();
    }
}
