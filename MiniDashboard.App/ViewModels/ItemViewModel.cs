using System.ComponentModel;
using MiniDashboard.Models.DTOs;

namespace MiniDashboard.App.ViewModels;

public class ItemViewModel : INotifyPropertyChanged
{
    private int _id;
    private string _name = string.Empty;
    private string? _description;
    private DateTime _createdAt;
    private DateTime _updatedAt;

    public int Id
    {
        get => _id;
        set
        {
            if (_id != value)
            {
                _id = value;
                OnPropertyChanged(nameof(Id));
            }
        }
    }

    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
    }

    public string? Description
    {
        get => _description;
        set
        {
            if (_description != value)
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }
    }

    public DateTime CreatedAt
    {
        get => _createdAt;
        set
        {
            if (_createdAt != value)
            {
                _createdAt = value;
                OnPropertyChanged(nameof(CreatedAt));
            }
        }
    }

    public DateTime UpdatedAt
    {
        get => _updatedAt;
        set
        {
            if (_updatedAt != value)
            {
                _updatedAt = value;
                OnPropertyChanged(nameof(UpdatedAt));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public static ItemViewModel FromDto(ItemDto dto)
    {
        return new ItemViewModel
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt
        };
    }
}

