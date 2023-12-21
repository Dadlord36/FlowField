using System;

namespace Structs
{
    public readonly struct Latitude
    {
        private readonly float value;

        public Latitude(float value)
        {
            if (value is < -90f or > 90f)
                throw new ArgumentException("Invalid latitude value. Latitude must be between -90 and 90 degrees.");
            this.value = value;
        }

        public static Latitude FromDegrees(float degrees)
        {
            return new Latitude(degrees);
        }

        public float ToDegrees()
        {
            return value;
        }
    }

    public readonly struct Longitude
    {
        private readonly float value;

        public Longitude(float value)
        {
            if (value is < -180f or > 180f)
                throw new ArgumentException("Invalid longitude value. Longitude must be between -180 and 180 degrees.");
            this.value = value;
        }

        public static Longitude FromDegrees(float degrees)
        {
            return new Longitude(degrees);
        }

        public float ToDegrees()
        {
            return value;
        }
    }
}