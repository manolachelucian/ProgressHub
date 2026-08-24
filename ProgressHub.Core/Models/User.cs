using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgressHub.Core.Models
{
    public class User
    {
        public int Id { get; private set; } 
        private string Name { get; set; }

        private DateTime BirthDate { get; set; }

    }
}
