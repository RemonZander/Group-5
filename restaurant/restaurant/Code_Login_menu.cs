using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant
{
    public class Code_Login_menu
    {
        private IO io = new IO();
        
        private Database database = new Database();

        public void Debug()
        {

        }

        public Login_gegevens Login_Check(string email, string password)
        {
            database = io.GetDatabase();
            if (database.login_gegevens != null)
            {
                foreach (var login_Gegevens in database.login_gegevens)
                {
                    if (email == login_Gegevens.email && password == login_Gegevens.password)
                    {
                        return login_Gegevens;
                    }
                }
            }
            if (database.werknemers != null)
            {
                foreach (var werknemer in database.werknemers)
                {
                    if (email == werknemer.login_gegevens.email && password == werknemer.login_gegevens.password)
                    {
                        return werknemer.login_gegevens;
                    }
                }
            }
            if (database.eigenaar != null)
            {
                if (email == database.eigenaar.login_gegevens.email && password == database.eigenaar.login_gegevens.password)
                {
                    return database.eigenaar.login_gegevens;
                }
            }

            return new Login_gegevens
            {
                type = "No account found"
            };
        }


        public string Register(Login_gegevens login_Gegevens)
        {
            database = io.GetDatabase();
            if (login_Gegevens.type == "Gebruiker")
            {
                if (database.login_gegevens != null)
                {
                    foreach (var item in database.login_gegevens)
                    {
                        if (item.email == login_Gegevens.email && item.type == login_Gegevens.type)
                        {
                            return "This email and account type is already in use";
                        }
                    }
                }
                char[] pswChars = login_Gegevens.password.ToCharArray();

                int punctFoundAmount = 0;
                int digitFoundAmount = 0;

                for (int i = 0; i < pswChars.Length; i++)
                {
                    if (char.IsPunctuation(pswChars[i])) punctFoundAmount++;
                    if (char.IsDigit(pswChars[i])) digitFoundAmount++;
                }

                if (login_Gegevens.password.Length < 8 || punctFoundAmount < 1 || digitFoundAmount < 1)
                {
                    return "Password must contain at least 8 characters, 1 punctuation mark and 1 number.";
                }

                if (database.login_gegevens == null)
                {
                    database.login_gegevens = new List<Login_gegevens> { login_Gegevens };
                }
                else
                {
                    database.login_gegevens.Add(login_Gegevens);
                }

                io.Savedatabase(database);
                return "Succes!";
            }
            else if (login_Gegevens.type == "Medewerker")
            {
                if (database.werknemers != null)
                {
                    foreach (var item in database.werknemers)
                    {
                        if (item.login_gegevens.email == login_Gegevens.email && item.login_gegevens.type == login_Gegevens.type)
                        {
                            return "This email and account type is already in use";
                        }
                    }
                }
                char[] pswChars = login_Gegevens.password.ToCharArray();

                int punctFoundAmount = 0;
                int digitFoundAmount = 0;

                for (int i = 0; i < pswChars.Length; i++)
                {
                    if (char.IsPunctuation(pswChars[i])) punctFoundAmount++;
                    if (char.IsDigit(pswChars[i])) digitFoundAmount++;
                }

                if (login_Gegevens.password.Length < 8 || punctFoundAmount < 1 || digitFoundAmount < 1)
                {
                    return "Password must contain at least 8 characters, 1 punctuation mark and 1 number.";
                }

                Werknemer werknemer = new Werknemer();
                werknemer.ID = 0;
                werknemer.lease_auto = 500.0;
                werknemer.prestatiebeloning = 0.0;
                werknemer.salaris = 3000.0;
                werknemer.login_gegevens = login_Gegevens;
                if (database.werknemers == null)
                {
                    database.werknemers = new List<Werknemer> { werknemer };
                }
                else
                {
                    database.werknemers.Add(werknemer);
                }

                io.Savedatabase(database);
                return "Succes!";
            }
            return "Fail!";
        }
    }
}