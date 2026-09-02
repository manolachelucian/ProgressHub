using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ProgressHub.Core.Models
{
    public class WeightTrendPoint
    {
        public DateOnly Date { get; set;  }
        public double? Weight { get; set; }

        public double? MovingAverage { get; set; }
    }
}
