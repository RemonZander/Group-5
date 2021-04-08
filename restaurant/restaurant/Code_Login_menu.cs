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

            if (!(login_Gegevens.password.Length < 8) && !(punctFoundAmount > 0) && !(digitFoundAmount > 0))
            {
                return "Password must contain at least 8 characters, 1 punctuation mark and 1 number.";
            }

            database.login_gegevens.Add(login_Gegevens);

            io.Savedatabase(database);
            return "Succes!";
        }
    }
}