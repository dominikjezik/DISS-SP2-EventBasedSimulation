using System.ComponentModel;
using DiscreteSimulation.FurnitureManufacturer.Entities;
using DiscreteSimulation.FurnitureManufacturer.Utilities;

namespace DiscreteSimulation.FurnitureManufacturer.DTOs;

public class WorkerDTO : INotifyPropertyChanged
{
    private string _id;
    private string _place;
    private string _order;
    private string _state;

    public string Id
    {
        get => _id;
        set
        {
            if (value == _id) return;
            _id = value;
            OnPropertyChanged(nameof(Id));
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

    public string Order
    {
        get => _order;
        set
        {
            if (value == _order) return;
            _order = value;
            OnPropertyChanged(nameof(Order));
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

    public void Update(Worker worker, WorkerGroup workerGroup)
    {
        Id = worker.DisplayId;
        
        if (worker.IsInWarehouse)
        {
            Place = "Warehouse";
        }
        else if (worker.IsMovingToWarehouse)
        {
            Place = "Moving to warehouse";
        }
        else if (worker.IsMovingToAssemblyLine)
        {
            Place = "Moving to assembly line";
        }
        else if (worker.CurrentAssemblyLine != null)
        {
            Place = $"Assembly line {worker.CurrentAssemblyLine.Id}";
        }
        else
        {
            Place = "???";
        }
        
        Order = worker.CurrentOrder?.Id.ToString() ?? string.Empty;
        
        if (worker.CurrentOrder == null)
        {
            State = "Idle";
        }
        else
        {
            State = worker.CurrentOrder.State;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
