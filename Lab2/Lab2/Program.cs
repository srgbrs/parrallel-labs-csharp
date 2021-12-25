using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;
using Lab2.Actors;

namespace Lab2
{
    internal static class Program
    {
        private const int NumberOfCars = 25;
        
        public static void Main(string[] args)
        {
            using var system = ActorSystem.Create("CarParking");
            
            var parking = system.ActorOf<ParkingActor>("parking");
            var carIndexes = new List<int>();
            for (var i = 0; i < NumberOfCars; i++)
            {
                carIndexes.Add(i);
            }

            Parallel.ForEach(carIndexes, index =>
            {
                var delay = (index + 1) * 1000;
                system.ActorOf(Props.Create(() =>
                    new CarActor(parking, $"car{index}", delay)));

            });

            Console.ReadLine();
        }
    }
}