using System;
using System.Threading;
using Akka.Actor;
using Lab2.Messages;

namespace Lab2.Actors
{
    public class CarActor: ReceiveActor 
    {
        private string LicensePlate { get; }
        private readonly IActorRef _parking;
        private readonly int _delay;

        public CarActor(IActorRef parking, string licensePlate, int delay)
        {
            _parking = parking;
            _delay = delay;
            LicensePlate = licensePlate;
            const int parkingDuration = 3333;

            
            Receive<ParkingRejected>(message =>
            {
                Console.WriteLine($"Car with this License plate {message.LicensePlate} didn't get a parking place");
            });
            Receive<ReceivedParking>(message =>
            {
                Console.WriteLine($"Car with this License plate {message.LicensePlate} got a parking place {message.ParkingId} and will be here for {parkingDuration}");
                Thread.Sleep(parkingDuration);
                Console.WriteLine($"Car with this License plate {message.LicensePlate} has left the parking place {message.ParkingId}");
                Sender.Tell(new LeaveParking(message.LicensePlate));
            });
            
            Start();
        }
        private void Start()
        {
            Thread.Sleep(_delay);
            Console.WriteLine($"Car  with this License plate {LicensePlate} going to the parking place");
            _parking.Tell(new RequestParking(LicensePlate));
        }
    }
}