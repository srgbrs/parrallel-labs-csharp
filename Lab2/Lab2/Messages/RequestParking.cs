namespace Lab2.Messages
{
    public class RequestParking
    {
        public string LicensePlate { get; }

        public RequestParking(string licensePlate)
        {
            LicensePlate = licensePlate;
        }
    }
}