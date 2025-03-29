using System.ComponentModel;
using DiscreteSimulation.FurnitureManufacturer.Entities;
using DiscreteSimulation.FurnitureManufacturer.Utilities;

namespace DiscreteSimulation.FurnitureManufacturer.DTOs;

public class OrderDTO : INotifyPropertyChanged, IUpdatable<OrderDTO>
{
    private int _id;
    private FurnitureType _type;
    private string _state;
    private string _place;
    private string _arrivalTime;

    public int Id
    {
        get => _id;
        set
        {
            if (value == _id) return;
            _id = value;
            OnPropertyChanged(nameof(Id));
        }
    }

    public FurnitureType Type
    {
        get => _type;
        set
        {
            if (value == _type) return;
            _type = value;
            OnPropertyChanged(nameof(Type));
        }
    }

    public string State
    {
        get => _state;
        set
        {
            if (value == _state) return;
            _state = value;
            OnPropertyChanged(nameof(State));
        }
    }

    public string Place
    {
        get => _place;
        set
        {
            if (value == _place) return;
            _place = value;
            OnPropertyChanged(nameof(Place));
        }
    }

    public string ArrivalTime
    {
        get => _arrivalTime;
        set
        {
            if (value.Equals(_arrivalTime)) return;
            _arrivalTime = value;
            OnPropertyChanged(nameof(ArrivalTime));
        }
    }

    public void Update(Order order)
    {
        Id = order.Id;
        Type = order.Type;
        State = order.State;
        ArrivalTime = order.ArrivalTime.ToString("F2");
    }
    
    public void Update(OrderDTO orderDTO)
    {
        Id = orderDTO.Id;
        Type = orderDTO.Type;
        State = orderDTO.State;
        ArrivalTime = orderDTO.ArrivalTime.FormatToSimulationTime(shortFormat: true);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public static class OrderDTOExtensions
{
    public static OrderDTO ToDTO(this Order order)
    {
        var orderDTO = new OrderDTO();
        orderDTO.Update(order);
        return orderDTO;
    }
}
