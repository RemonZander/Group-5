using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant
{
    public class Code_Eigenaar_menu
    {
        private Database database = new Database();
        Reserveringen[] remon = new Reserveringen[100];
        IO io = new IO();
        public void fillReservations()
        {
            for (int i = 0; i < 100; i++)
            {
                long[] telefoonnummer = new long[1];
                telefoonnummer[0] = 0613122228;
                Klantgegevens[] klantgegevens = new Klantgegevens[2];
                klantgegevens[0] = new Klantgegevens { 
                    voornaam = "Rémon",
                    achternaam = "Zander",
                    adres = new adres
                    {
                        land = "NL",
                        huisnummer = 6,
                        postcode = "3233XK",
                        straatnaam = "Zeehoeveweg",
                        woonplaats = "Oostvoorne"
                    },
                    klantnummer = 0948619 - i,
                    geb_datum = new DateTime(2000 + i*i, 10, 20),
                    telefoonnummer = telefoonnummer,
                };

                telefoonnummer[0] = 0681295232;
                klantgegevens[1] = new Klantgegevens
                {
                    voornaam = "Ludo",
                    achternaam = "de Vries",
                    adres = new adres
                    {
                        land = "NL",
                        huisnummer = 9,
                        postcode = "3311JG",
                        straatnaam = "Burgemeester de Raadtsingel",
                        woonplaats = "Dordrecht"
                    },
                    klantnummer = 1013007 - i,
                    geb_datum = new DateTime(2000 + i * i, 10, 20),
                    telefoonnummer = telefoonnummer,
                };
                remon[i].klantgegevens = klantgegevens;
                remon[i].datum = new DateTime(2000 - i, 1, 14);
            }

            database.reserveringen = remon;
            io.Savedatabase(database);
        }
        public string sayHello()
        {
            string s = "";
            foreach (var item in database.reserveringen)
            {
                s += item;
                s += Environment.NewLine;
            }
            return s;
        }
    }
}