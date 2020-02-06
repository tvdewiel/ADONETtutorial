using System;
using System.Collections.Generic;
using System.Text;

namespace ADONETgeneric
{
    public class Klas
    {
        public Klas(string klasnaam)
        {
            this.klasnaam = klasnaam;
        }

        public Klas(int id, string klasnaam)
        {
            this.id = id;
            this.klasnaam = klasnaam;
        }

        public int id { get; set; }
        public string klasnaam { get; set; }
    }
}
