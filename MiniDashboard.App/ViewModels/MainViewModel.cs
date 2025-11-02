using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Windows.Data;
using System.Windows.Input;
using MiniDashboard.App.Commands;
using MiniDashboard.App.Models;
using MiniDashboard.App.Services;

namespace MiniDashboard.App.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly IItemApiService _itemApiService;
    private readonly CollectionViewSource _itemsViewSource;
    private string _searchQuery = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isLoading;
    private ItemViewModel? _selectedItem;
    private string _nameInput = string.Empty;
    private string _descriptionInput = string.Empty;
    private bool _isEditMode;

    public MainViewModel(IItemApiService itemApiService)
    {
        _itemApiService = itemApiService ?? throw new ArgumentNullException(nameof(itemApiService));

        Items = new ObservableCollection<ItemViewModel>();
        _itemsViewSource = new CollectionViewSource { Source = Items };
        ItemsView = _itemsViewSource.View;
        ItemsView.Filter = FilterItems;

        LoadItemsCommand = new AsyncRelayCommand(async _ => await LoadItemsAsync(), _ => !IsLoading);
        SearchCommand = new RelayCommand(_ => RefreshItemsView(), _ => !IsLoading);
        AddCommand = new AsyncRelayCommand(async _ => await AddItemAsync(), _ => !IsLoading && !IsEditMode && !string.IsNullOrWhiteSpace(NameInput));
        EditCommand = new RelayCommand(_ => EditItem(), _ => !IsLoading && !IsEditMode && SelectedItem != null);
        DeleteCommand = new AsyncRelayCommand<ItemViewModel>(async item => await DeleteItemAsync(item), _ => !IsLoading && !IsEditMode);
        SaveCommand = new AsyncRelayCommand(async _ => await SaveItemAsync(), _ => !IsLoading && !string.IsNullOrWhiteSpace(NameInput));
        CancelCommand = new RelayCommand(_ => CancelEdit(), _ => IsEditMode);
        SortCommand = new RelayCommand<string>(sortBy => SortItems(sortBy ?? "Name"), _ => !IsLoading);
        
        // Load items on startup
        _ = LoadItemsAsync();
    }

    public ObservableCollection<ItemViewModel> Items { get; }

    public ICollectionView ItemsView { get; }

    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (_searchQuery != value)
            {
                _searchQuery = value;
                OnPropertyChanged(nameof(SearchQuery));
                RefreshItemsView();
            }
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (_errorMessage != value)
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
                OnPropertyChanged(nameof(HasError));
            }
        }
    }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (_isLoading != value)
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }
    }

    public ItemViewModel? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (_selectedItem != value)
            {
                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
                if (value != null && !IsEditMode)
                {
                    NameInput = value.Name;
                    DescriptionInput = value.Description ?? string.Empty;
                }
            }
        }
    }

    public string NameInput
    {
        get => _nameInput;
        set
        {
            if (_nameInput != value)
            {
                _nameInput = value;
                OnPropertyChanged(nameof(NameInput));
            }
        }
    }

    public string DescriptionInput
    {
        get => _descriptionInput;
        set
        {
            if (_descriptionInput != value)
            {
                _descriptionInput = value;
                OnPropertyChanged(nameof(DescriptionInput));
            }
        }
    }

    public bool IsEditMode
    {
        get => _isEditMode;
        set
        {
            if (_isEditMode != value)
            {
                _isEditMode = value;
                OnPropertyChanged(nameof(IsEditMode));
                OnPropertyChanged(nameof(IsNotEditMode));
            }
        }
    }

    public bool IsNotEditMode => !IsEditMode;

    public ICommand LoadItemsCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand SortCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool FilterItems(object obj)
    {
        if (obj is not ItemViewModel item)
            return false;

        if (string.IsNullOrWhiteSpace(SearchQuery))
            return true;

        var query = SearchQuery.ToLower();
        return item.Name.ToLower().Contains(query) ||
               (!string.IsNullOrEmpty(item.Description) && item.Description.ToLower().Contains(query));
    }

    private void RefreshItemsView()
    {
        ItemsView.Refresh();
    }

    private void SortItems(string sortBy)
    {
        using (ItemsView.DeferRefresh())
        {
            ItemsView.SortDescriptions.Clear();

            switch (sortBy)
            {
                case "Name":
                    ItemsView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                    break;
                case "CreatedAt":
                    ItemsView.SortDescriptions.Add(new SortDescription("CreatedAt", ListSortDirection.Descending));
                    break;
                case "UpdatedAt":
                    ItemsView.SortDescriptions.Add(new SortDescription("UpdatedAt", ListSortDirection.Descending));
                    break;
            }
        }
    }

    private async Task LoadItemsAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var items = await _itemApiService.GetAllItemsAsync();
            Items.Clear();
            foreach (var item in items)
            {
                Items.Add(item);
            }
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"Unable to connect to server: {ex.Message}";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load data: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task AddItemAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var newItem = await _itemApiService.CreateItemAsync(NameInput, DescriptionInput);
            Items.Insert(0, newItem);
            NameInput = string.Empty;
            DescriptionInput = string.Empty;
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"Unable to connect to server: {ex.Message}";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to add item: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void EditItem()
    {
        if (SelectedItem == null) return;

        IsEditMode = true;
        NameInput = SelectedItem.Name;
        DescriptionInput = SelectedItem.Description ?? string.Empty;
    }

    private async Task SaveItemAsync()
    {
        if (SelectedItem == null) return;

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var updatedItem = await _itemApiService.UpdateItemAsync(
                SelectedItem.Id,
                NameInput,
                DescriptionInput);

            var index = Items.IndexOf(SelectedItem);
            if (index >= 0)
            {
                Items[index] = updatedItem;
                SelectedItem = updatedItem;
            }

            IsEditMode = false;
            NameInput = string.Empty;
            DescriptionInput = string.Empty;
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"Unable to connect to server: {ex.Message}";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to update item: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void CancelEdit()
    {
        IsEditMode = false;
        if (SelectedItem != null)
        {
            NameInput = SelectedItem.Name;
            DescriptionInput = SelectedItem.Description ?? string.Empty;
        }
        else
        {
            NameInput = string.Empty;
            DescriptionInput = string.Empty;
        }
    }

    private async Task DeleteItemAsync(ItemViewModel? item)
    {
        if (item == null) return;

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var success = await _itemApiService.DeleteItemAsync(item.Id);
            if (success)
            {
                Items.Remove(item);
                if (SelectedItem == item)
                {
                    SelectedItem = null;
                    NameInput = string.Empty;
                    DescriptionInput = string.Empty;
                }
            }
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"Unable to connect to server: {ex.Message}";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to delete item: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

