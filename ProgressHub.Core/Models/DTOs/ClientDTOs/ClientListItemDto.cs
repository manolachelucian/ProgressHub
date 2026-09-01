using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgressHub.Core.Models.DTOs.ClientDTOs
{
    public class ClientListItemDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int TargetCalories { get; set; }
        public double? LatestWeight { get; set; }

        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}
