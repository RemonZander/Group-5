using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant
{
    public class Code_Login_menu
    {
        private IO IO = new IO();
        
        private Database database = new Database();

        public Code_Login_menu()
        {
            database = IO.Getdatabase();
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
            List<Login_gegevens> List_Login_temp = database.login_gegevens;

            foreach (var login_temp in database.login_gegevens)
            {
                if (login_Gegevens.email == login_temp.email && login_Gegevens.type == login_temp.type)
                {
                    return "Account already exists";
                }
                else
                {
                    List_Login_temp.Add(login_Gegevens);
                    database.login_gegevens = List_Login_temp;
                    IO.Savedatabase(database);
                    return "Account created";
                }
            }
            return "Unexpected error";
        }
    }
}