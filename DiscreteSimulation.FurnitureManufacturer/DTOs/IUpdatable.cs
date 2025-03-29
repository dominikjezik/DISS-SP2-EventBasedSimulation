namespace DiscreteSimulation.FurnitureManufacturer.DTOs;

public interface IUpdatable<TItem>
{
    void Update(TItem entity);
}
