using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiffinBox.Domain.Common;

namespace TiffinBox.Domain.ValueObjects
{
    public class GeoLocation : ValueObject
    {
        public int Id { get; set; }
        public double Latitude { get; init; }
        public double Longitude { get; init; }

  
        private GeoLocation()
        {
            Latitude = 0;
            Longitude = 0;
        }

        public GeoLocation(double latitude, double longitude)
        {
            if (latitude < -90 || latitude > 90)
                throw new ArgumentException("Invalid latitude");
            if (longitude < -180 || longitude > 180)
                throw new ArgumentException("Invalid longitude");

            Latitude = latitude;
            Longitude = longitude;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Latitude;
            yield return Longitude;
        }
    }
}
