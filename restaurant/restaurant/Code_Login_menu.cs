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

        public Code_Login_menu()
        {
            database = io.Getdatabase();
        }

        public void Debug()
        {

        }

        public Login_gegevens Login_Check(string email, string password)
        {
            foreach (var login_Gegevens in database.login_gegevens)
            {
                if (email == login_Gegevens.email && password == login_Gegevens.password)
                {
                    return login_Gegevens;
                }
            }
            return new Login_gegevens
            {
                type = "No account found"
            };
        }

        
        public string Register(Login_gegevens login_Gegevens)
        {
            List<string> chars = Make_chararray();
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

            for (int b = 0; b < chars.Count(); b++)
            {
                if (login_Gegevens.password.Contains(chars[b]) && login_Gegevens.password.Length < 8 &&
                    (login_Gegevens.password.Contains("0") || login_Gegevens.password.Contains("1") || login_Gegevens.password.Contains("2") ||
                    login_Gegevens.password.Contains("3") || login_Gegevens.password.Contains("4") || login_Gegevens.password.Contains("5") ||
                    login_Gegevens.password.Contains("6") || login_Gegevens.password.Contains("7") || login_Gegevens.password.Contains("8") || login_Gegevens.password.Contains("9")))
                {
                    database.login_gegevens.Add(login_Gegevens);

                    io.Savedatabase(database);
                    return "Succes!";
                }
            }

            return "Password must contain at least 8 characters, 1 punctuation mark and 1 number.";
        }

        private List<string> Make_chararray()
        {
            List<string> chars = new List<string>();
            chars.AddRange(new List<string>
                {
                    "!", "@", "#", "$", "%", "^", "&", "*", "(", ")", "-", "_", "=", "+", "[", "]", "{", "}", @"\", "|", ";", ":", @"'", ",", ".", "<", ">", "/", "?"
                });

            return chars;
        }
    }
}