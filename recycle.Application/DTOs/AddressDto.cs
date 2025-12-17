using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.DTOs
{
    public class AddressDto
    {
        //public Guid Id { get; set; }

        public string Street { get; set; }
        public string City { get; set; }
        public string Governorate { get; set; }
        public string PostalCode { get; set; }

    }
}
