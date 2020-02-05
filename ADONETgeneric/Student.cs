using System;
using System.Collections.Generic;
using System.Text;

namespace ADONETgeneric
{
    public class Student
    {
        public Student(int studentId, string naam)
        {
            this.studentId = studentId;
            this.naam = naam;
        }

        public int studentId { get; set; }
        public string naam { get; set; }
        public List<Cursus> cursussen { get; private set; }
        public void voegCursusToe(Cursus c)
        {
            cursussen.Add(c);
        }
        public void ShowStudent()
        {
            Console.WriteLine($"{studentId},{naam}");
            foreach(Cursus c in cursussen)
            {
                Console.WriteLine($"{c}");
            }
        }
    }
}
