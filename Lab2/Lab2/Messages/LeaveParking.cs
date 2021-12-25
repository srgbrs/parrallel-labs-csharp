namespace Lab2.Messages
{
    public class LeaveParking
    {
        public string LicensePlate { get; }

        public LeaveParking(string licensePlate)
        {
            LicensePlate = licensePlate;
        }
    }
}