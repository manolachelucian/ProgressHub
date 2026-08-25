using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgressHub.Core.Models
{
    public class DailyLog
    {

        public int Id { get; set; }

        //Relation to user
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public DateOnly Date {  get; set; }
        public double Weight { get; set; } 
        

        //Consumed by day
        public int ConsumedCalories { get; set; }
        public int ConsumedProteins { get; set; }
        public int ConsumedCarbs { get; set; }
        public int ConsumedFats { get; set; }

        public string? Note {  get; set; }
    }
}
