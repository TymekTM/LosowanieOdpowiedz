using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LosowanieOdpowiedz.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsPresent { get; set; } = true;
        public bool HasBeenQueried { get; set; } = false;
        public bool HasLuckyNumber { get; set; } = false;
        public string Class { get; set; }
    }
}
