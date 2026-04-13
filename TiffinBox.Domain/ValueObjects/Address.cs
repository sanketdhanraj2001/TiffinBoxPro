using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiffinBox.Domain.ValueObjects
{
    public record Address
    {
        public int Id { get; set; }
        public string Street { get; init; }
        public string City { get; init; }
        public string State { get; init; }
        public string PostalCode { get; init; }
        public string Country { get; init; }
        public string? Landmark { get; init; }

        // ✅ Parameterless constructor for EF Core
        private Address()
        {
            Street = string.Empty;
            City = string.Empty;
            State = string.Empty;
            PostalCode = string.Empty;
            Country = string.Empty;
        }

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
