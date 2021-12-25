namespace Lab2.Models
{
    public class ParkingModel
    {
        public string Id { get; }
        public string CarLicensePlate
        {
            get => _carLicensePlate;
            set
            {
                _carLicensePlate = value;
                IsBusy = _carLicensePlate != null;
            }
        }
        public bool IsBusy { get; private set; } 
        private string _carLicensePlate;

        public ParkingModel(string id)
        {
            Id = id;
        }
    }
}