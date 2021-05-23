using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager
{
    class Combo
    {
        public Combo() { }

        public Combo(string description, string password)
        {
            // Description with 64 max characters
            StringBuilder _description = new StringBuilder(description);
            _description.Length = 64;
            this.Description = _description.ToString();

            // Password with 256 max characters
            StringBuilder _password = new StringBuilder(password);
            _password.Length = 256;
            this.Password = _password.ToString();
        }
        public string Description { get; set; }

        public string Password { get; set; }
    }
}
