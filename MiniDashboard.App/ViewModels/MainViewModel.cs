using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using MiniDashboard.App.Commands;
using MiniDashboard.App.Services;

namespace MiniDashboard.App.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly IItemApiService _itemApiService;
    private readonly ILogger<MainViewModel> _logger;
    private readonly CollectionViewSource _itemsViewSource;
    private string _searchQuery = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isLoading;
    private ItemViewModel? _selectedItem;
    private string _nameInput = string.Empty;
    private string _descriptionInput = string.Empty;
    private bool _isEditMode;

    public MainViewModel(IItemApiService itemApiService, ILogger<MainViewModel> logger)
    {
        _itemApiService = itemApiService ?? throw new ArgumentNullException(nameof(itemApiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _logger.LogInformation("MainViewModel initialized");

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
        _logger.LogInformation("Action: RefreshItemsView - Refreshing items view with search query: {Query}", SearchQuery);
        ItemsView.Refresh();
        _logger.LogInformation("Action: RefreshItemsView - Items view refreshed. Visible count: {Count}", 
            ItemsView.Cast<object>().Count());
    }

    private void SortItems(string sortBy)
    {
        _logger.LogInformation("Action: SortItems - Sorting by: {SortBy}", sortBy);
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
        _logger.LogInformation("Action: SortItems - Items sorted by: {SortBy}", sortBy);
    }

    private async Task LoadItemsAsync()
    {
        _logger.LogInformation("Action: LoadItems - Started");
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            _logger.LogInformation("Action: LoadItems - Calling API to get all items");
            var items = await _itemApiService.GetAllItemsAsync();
            _logger.LogInformation("Action: LoadItems - Received {Count} items from API", items.Count);
            
            Items.Clear();
            foreach (var item in items)
            {
                Items.Add(item);
            }
            _logger.LogInformation("Action: LoadItems - Successfully loaded {Count} items", items.Count);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Action: LoadItems - HTTP error: {ErrorMessage}", ex.Message);
            ErrorMessage = $"Unable to connect to server: {ex.Message}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Action: LoadItems - Failed: {ErrorMessage}", ex.Message);
            ErrorMessage = $"Failed to load data: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            _logger.LogInformation("Action: LoadItems - Completed");
        }
    }

    private async Task AddItemAsync()
    {
        _logger.LogInformation("Action: AddItem - Started. Name: {ItemName}", NameInput);
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            _logger.LogInformation("Action: AddItem - Calling API to create item");
            var newItem = await _itemApiService.CreateItemAsync(NameInput, DescriptionInput);
            _logger.LogInformation("Action: AddItem - Item created successfully with ID: {ItemId}, Name: {ItemName}", 
                newItem.Id, newItem.Name);
            
            Items.Insert(0, newItem);
            NameInput = string.Empty;
            DescriptionInput = string.Empty;
            _logger.LogInformation("Action: AddItem - Successfully added item to collection");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Action: AddItem - HTTP error: {ErrorMessage}", ex.Message);
            ErrorMessage = $"Unable to connect to server: {ex.Message}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Action: AddItem - Failed: {ErrorMessage}", ex.Message);
            ErrorMessage = $"Failed to add item: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            _logger.LogInformation("Action: AddItem - Completed");
        }
    }

    private void EditItem()
    {
        if (SelectedItem == null)
        {
            _logger.LogWarning("Action: EditItem - No item selected");
            return;
        }

        _logger.LogInformation("Action: EditItem - Started. Item ID: {ItemId}, Name: {ItemName}", 
            SelectedItem.Id, SelectedItem.Name);
        IsEditMode = true;
        NameInput = SelectedItem.Name;
        DescriptionInput = SelectedItem.Description ?? string.Empty;
        _logger.LogInformation("Action: EditItem - Entered edit mode for item ID: {ItemId}", SelectedItem.Id);
    }

    private async Task SaveItemAsync()
    {
        if (SelectedItem == null)
        {
            _logger.LogWarning("Action: SaveItem - No item selected");
            return;
        }

        _logger.LogInformation("Action: SaveItem - Started. Item ID: {ItemId}, New Name: {ItemName}", 
            SelectedItem.Id, NameInput);
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            _logger.LogInformation("Action: SaveItem - Calling API to update item ID: {ItemId}", SelectedItem.Id);
            var updatedItem = await _itemApiService.UpdateItemAsync(
                SelectedItem.Id,
                NameInput,
                DescriptionInput);

            _logger.LogInformation("Action: SaveItem - Item updated successfully. ID: {ItemId}, Name: {ItemName}", 
                updatedItem.Id, updatedItem.Name);

            var index = Items.IndexOf(SelectedItem);
            if (index >= 0)
            {
                Items[index] = updatedItem;
                SelectedItem = updatedItem;
                _logger.LogInformation("Action: SaveItem - Updated item in collection at index: {Index}", index);
            }

            IsEditMode = false;
            NameInput = string.Empty;
            DescriptionInput = string.Empty;
            _logger.LogInformation("Action: SaveItem - Exited edit mode");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Action: SaveItem - HTTP error: {ErrorMessage}", ex.Message);
            ErrorMessage = $"Unable to connect to server: {ex.Message}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Action: SaveItem - Failed: {ErrorMessage}", ex.Message);
            ErrorMessage = $"Failed to update item: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            _logger.LogInformation("Action: SaveItem - Completed");
        }
    }

    private void CancelEdit()
    {
        _logger.LogInformation("Action: CancelEdit - Started");
        IsEditMode = false;
        if (SelectedItem != null)
        {
            _logger.LogInformation("Action: CancelEdit - Resetting to original values for item ID: {ItemId}", 
                SelectedItem.Id);
            NameInput = SelectedItem.Name;
            DescriptionInput = SelectedItem.Description ?? string.Empty;
        }
        else
        {
            _logger.LogInformation("Action: CancelEdit - Clearing input fields");
            NameInput = string.Empty;
            DescriptionInput = string.Empty;
        }
        _logger.LogInformation("Action: CancelEdit - Completed. Edit mode cancelled");
    }

    private async Task DeleteItemAsync(ItemViewModel? item)
    {
        if (item == null)
        {
            _logger.LogWarning("Action: DeleteItem - No item provided for deletion");
            return;
        }

        _logger.LogInformation("Action: DeleteItem - Started. Item ID: {ItemId}, Name: {ItemName}", 
            item.Id, item.Name);
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            _logger.LogInformation("Action: DeleteItem - Calling API to delete item ID: {ItemId}", item.Id);
            var success = await _itemApiService.DeleteItemAsync(item.Id);
            if (success)
            {
                _logger.LogInformation("Action: DeleteItem - Item deleted successfully from API");
                Items.Remove(item);
                _logger.LogInformation("Action: DeleteItem - Item removed from collection");
                
                if (SelectedItem == item)
                {
                    SelectedItem = null;
                    NameInput = string.Empty;
                    DescriptionInput = string.Empty;
                    _logger.LogInformation("Action: DeleteItem - Cleared selected item");
                }
                _logger.LogInformation("Action: DeleteItem - Successfully deleted item ID: {ItemId}", item.Id);
            }
            else
            {
                _logger.LogWarning("Action: DeleteItem - Delete operation returned false for item ID: {ItemId}", item.Id);
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Action: DeleteItem - HTTP error: {ErrorMessage}", ex.Message);
            ErrorMessage = $"Unable to connect to server: {ex.Message}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Action: DeleteItem - Failed: {ErrorMessage}", ex.Message);
            ErrorMessage = $"Failed to delete item: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            _logger.LogInformation("Action: DeleteItem - Completed");
        }
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

