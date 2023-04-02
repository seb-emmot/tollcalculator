namespace TollCalculations.Vehicles;

public class Vehicle : IVehicle
{
    public VehicleTypes VehicleType { get; }

    public Vehicle(VehicleTypes type)
    {
        VehicleType = type;
    }
}
