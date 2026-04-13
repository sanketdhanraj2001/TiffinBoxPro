using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiffinBox.Domain.ValueObjects;

namespace TiffinBox.Application.DTOs.Common
{
    public class AddressDto
    {
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string? Landmark { get; set; }

        public Address ToDomain()
        {
            return new Address(Street, City, State, PostalCode, Country, Landmark);
        }
    }
}
