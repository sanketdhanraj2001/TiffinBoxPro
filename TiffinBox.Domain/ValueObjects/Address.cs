using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiffinBox.Domain.ValueObjects
{
    public record Address
    {
        public string Street { get; }
        public string City { get; }
        public string State { get; }
        public string PostalCode { get; }
        public string Country { get; }
        public string? Landmark { get; }

        public Address(
            string street,
            string city,
            string state,
            string postalCode,
            string country,
            string? landmark = null)
        {
            if (string.IsNullOrWhiteSpace(street))
                throw new ArgumentException("Street is required");
            if (string.IsNullOrWhiteSpace(city))
                throw new ArgumentException("City is required");
            if (string.IsNullOrWhiteSpace(state))
                throw new ArgumentException("State is required");
            if (string.IsNullOrWhiteSpace(postalCode))
                throw new ArgumentException("Postal code is required");
            if (string.IsNullOrWhiteSpace(country))
                throw new ArgumentException("Country is required");

            Street = street;
            City = city;
            State = state;
            PostalCode = postalCode;
            Country = country;
            Landmark = landmark;
        }

        public override string ToString()
        {
            var parts = new[] { Street, Landmark, City, State, PostalCode, Country }
                .Where(x => !string.IsNullOrWhiteSpace(x));
            return string.Join(", ", parts);
        }
    }
}
