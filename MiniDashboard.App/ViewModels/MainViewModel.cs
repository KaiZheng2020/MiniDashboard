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
    private int _currentPage = 1;
    private int _pageSize = 10;
    private int _totalCount = 0;
    private int _totalPages = 0;
    private string? _currentSortField;
    private ListSortDirection _currentSortDirection = ListSortDirection.Ascending;

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
        SearchCommand = new AsyncRelayCommand(async _ => await LoadItemsAsync(), _ => !IsLoading);
        AddCommand = new AsyncRelayCommand(async _ => await AddItemAsync(), _ => !IsLoading && !IsEditMode && !string.IsNullOrWhiteSpace(NameInput));
        EditCommand = new RelayCommand(_ => EditItem(), _ => !IsLoading && !IsEditMode && SelectedItem != null);
        DeleteCommand = new AsyncRelayCommand<ItemViewModel>(async item => await DeleteItemAsync(item), _ => !IsLoading && !IsEditMode);
        SaveCommand = new AsyncRelayCommand(async _ => await SaveItemAsync(), _ => !IsLoading && !string.IsNullOrWhiteSpace(NameInput));
        CancelCommand = new RelayCommand(_ => CancelEdit(), _ => IsEditMode);
        SortCommand = new RelayCommand<string>(sortBy => SortItems(sortBy ?? "Name"), _ => !IsLoading);
        FirstPageCommand = new AsyncRelayCommand(async _ => await GoToPageAsync(1), _ => !IsLoading && CurrentPage > 1);
        PreviousPageCommand = new AsyncRelayCommand(async _ => await GoToPageAsync(CurrentPage - 1), _ => !IsLoading && CurrentPage > 1);
        NextPageCommand = new AsyncRelayCommand(async _ => await GoToPageAsync(CurrentPage + 1), _ => !IsLoading && CurrentPage < TotalPages);
        LastPageCommand = new AsyncRelayCommand(async _ => await GoToPageAsync(TotalPages), _ => !IsLoading && CurrentPage < TotalPages);
        
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
                // Reset to first page when search query changes
                if (CurrentPage != 1)
                {
                    CurrentPage = 1;
                }
                _ = LoadItemsAsync();
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

    public int CurrentPage
    {
        get => _currentPage;
        set
        {
            if (_currentPage != value)
            {
                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
                OnPropertyChanged(nameof(HasPreviousPage));
                OnPropertyChanged(nameof(HasNextPage));
            }
        }
    }

    public int PageSize
    {
        get => _pageSize;
        set
        {
            if (_pageSize != value && value > 0)
            {
                _pageSize = value;
                OnPropertyChanged(nameof(PageSize));
                _ = LoadItemsAsync();
            }
        }
    }

    public int TotalCount
    {
        get => _totalCount;
        set
        {
            if (_totalCount != value)
            {
                _totalCount = value;
                OnPropertyChanged(nameof(TotalCount));
            }
        }
    }

    public int TotalPages
    {
        get => _totalPages;
        set
        {
            if (_totalPages != value)
            {
                _totalPages = value;
                OnPropertyChanged(nameof(TotalPages));
                OnPropertyChanged(nameof(HasPreviousPage));
                OnPropertyChanged(nameof(HasNextPage));
                OnPropertyChanged(nameof(HasPagination));
                OnPropertyChanged(nameof(PageInfo));
            }
        }
    }

    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
    public bool HasPagination => TotalPages > 0;
    public string PageInfo => TotalPages > 0 ? $"Page {CurrentPage} of {TotalPages} ({TotalCount} items)" : string.Empty;

    public ICommand LoadItemsCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand SortCommand { get; }
    public ICommand FirstPageCommand { get; }
    public ICommand PreviousPageCommand { get; }
    public ICommand NextPageCommand { get; }
    public ICommand LastPageCommand { get; }

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
        // No longer needed - we use server-side pagination
        // Keeping for backward compatibility if needed
    }

    private void SortItems(string sortBy)
    {
        _logger.LogInformation("Action: SortItems - Sorting by: {SortBy}", sortBy);
        
        // If clicking the same field, toggle direction; otherwise, set new field with ascending
        if (_currentSortField == sortBy)
        {
            _currentSortDirection = _currentSortDirection == ListSortDirection.Ascending 
                ? ListSortDirection.Descending 
                : ListSortDirection.Ascending;
        }
        else
        {
            _currentSortField = sortBy;
            _currentSortDirection = ListSortDirection.Ascending;
        }
        
        using (ItemsView.DeferRefresh())
        {
            ItemsView.SortDescriptions.Clear();

            switch (sortBy)
            {
                case "Name":
                    ItemsView.SortDescriptions.Add(new SortDescription("Name", _currentSortDirection));
                    break;
                case "CreatedAt":
                    ItemsView.SortDescriptions.Add(new SortDescription("CreatedAt", _currentSortDirection));
                    break;
                case "UpdatedAt":
                    ItemsView.SortDescriptions.Add(new SortDescription("UpdatedAt", _currentSortDirection));
                    break;
            }
        }
        _logger.LogInformation("Action: SortItems - Items sorted by: {SortBy} ({Direction})", 
            sortBy, _currentSortDirection);
    }

    private async Task LoadItemsAsync()
    {
        _logger.LogInformation("Action: LoadItems - Started. Page: {Page}, PageSize: {PageSize}, SearchQuery: {Query}", 
            CurrentPage, PageSize, SearchQuery);
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                _logger.LogInformation("Action: LoadItems - Calling API to get paged items");
                var (items, totalCount, page, pageSize, totalPages) = await _itemApiService.GetAllItemsPagedAsync(CurrentPage, PageSize);
                _logger.LogInformation("Action: LoadItems - Received {Count} items from API (Page {Page} of {TotalPages}, Total: {TotalCount})", 
                    items.Count, page, totalPages, totalCount);
                
                Items.Clear();
                foreach (var item in items)
                {
                    Items.Add(item);
                }
                TotalCount = totalCount;
                TotalPages = totalPages;
                CurrentPage = page;
                _logger.LogInformation("Action: LoadItems - Successfully loaded {Count} items", items.Count);
            }
            else
            {
                _logger.LogInformation("Action: LoadItems - Calling API to search paged items");
                var (items, totalCount, page, pageSize, totalPages) = await _itemApiService.SearchItemsPagedAsync(SearchQuery, CurrentPage, PageSize);
                _logger.LogInformation("Action: LoadItems - Received {Count} items from API (Page {Page} of {TotalPages}, Total: {TotalCount})", 
                    items.Count, page, totalPages, totalCount);
                
                Items.Clear();
                foreach (var item in items)
                {
                    Items.Add(item);
                }
                TotalCount = totalCount;
                TotalPages = totalPages;
                CurrentPage = page;
                _logger.LogInformation("Action: LoadItems - Successfully loaded {Count} items", items.Count);
            }
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

    private async Task GoToPageAsync(int page)
    {
        if (page < 1 || page > TotalPages)
            return;

        CurrentPage = page;
        await LoadItemsAsync();
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
            
            // Reload items to show new item in correct position (might be on different page)
            await LoadItemsAsync();
            NameInput = string.Empty;
            DescriptionInput = string.Empty;
            _logger.LogInformation("Action: AddItem - Successfully added item, reloaded items");
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

            // Reload items to ensure correct order
            await LoadItemsAsync();
            // Try to find and select the updated item
            var updatedItemInList = Items.FirstOrDefault(i => i.Id == updatedItem.Id);
            if (updatedItemInList != null)
            {
                SelectedItem = updatedItemInList;
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
                
                // Reload items to update pagination
                await LoadItemsAsync();
                
                // Clear selection if deleted item was selected
                if (SelectedItem?.Id == item.Id)
                {
                    SelectedItem = null;
                    NameInput = string.Empty;
                    DescriptionInput = string.Empty;
                    _logger.LogInformation("Action: DeleteItem - Cleared selected item");
                }
                _logger.LogInformation("Action: DeleteItem - Successfully deleted item ID: {ItemId}, reloaded items", item.Id);
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

