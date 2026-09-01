using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgressHub.Core.Models.DTOs.DailyLogDTOs
{
    public class UpdateDailyLogDto : CreateDailyLogDto
    {
        public int Id { get; set; }
    }
}
