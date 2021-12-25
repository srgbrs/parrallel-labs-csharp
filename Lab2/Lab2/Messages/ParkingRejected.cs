namespace Lab2.Messages
{
    public class ParkingRejected
    {
        public string LicensePlate { get; }

        public ParkingRejected(string licensePlate)
        {
            LicensePlate = licensePlate;
        }
    }
}