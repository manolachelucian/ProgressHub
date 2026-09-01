using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgressHub.Core.Models.DTOs.ClientDTOs
{
    public class UpdateClientDto : CreateClientDto
    {
        public int Id { get; set; }
    }
}
