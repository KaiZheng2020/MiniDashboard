using MiniDashboard.Models.Common;
using MiniDashboard.Models.DTOs;
using MiniDashboard.Api.Service;
using Microsoft.AspNetCore.Mvc;

namespace MiniDashboard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IItemService _itemService;
    private readonly ILogger<ItemsController> _logger;

    public ItemsController(IItemService itemService, ILogger<ItemsController> logger)
    {
        _itemService = itemService;
        _logger = logger;
    }

    /// <summary>
    /// Get all items (with optional pagination)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(WebApiResponse<List<ItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<WebApiResponse<List<ItemDto>>>> GetAllItems([FromQuery] int? page = null, [FromQuery] int? pageSize = null)
    {
        _logger.LogInformation("GET /api/items - Request received to get items. Page: {Page}, PageSize: {PageSize}", page, pageSize);
        try
        {
            if (page.HasValue && pageSize.HasValue)
            {
                // Normalize page and pageSize (even if invalid values are provided)
                var normalizedPage = page.Value < 1 ? 1 : page.Value;
                var normalizedPageSize = pageSize.Value < 1 ? 10 : (pageSize.Value > 100 ? 100 : pageSize.Value);
                
                var (items, totalCount) = await _itemService.GetAllPagedAsync(normalizedPage, normalizedPageSize);
                var totalPages = (int)Math.Ceiling(totalCount / (double)normalizedPageSize);
                _logger.LogInformation("GET /api/items - Successfully retrieved {Count} items (Page {Page} of {TotalPages}, Total: {TotalCount})", 
                    items.Count, normalizedPage, totalPages, totalCount);
                return Ok(WebApiResponse<List<ItemDto>>.Ok(items, totalCount, normalizedPage, normalizedPageSize, totalPages));
            }
            else
            {
                var items = await _itemService.GetAllAsync();
                _logger.LogInformation("GET /api/items - Successfully retrieved {Count} items (all items)", items.Count);
                return Ok(WebApiResponse<List<ItemDto>>.Ok(items));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET /api/items - Error retrieving items: {ErrorMessage}", ex.Message);
            return StatusCode(500, WebApiResponse<List<ItemDto>>.Fail("Internal server error"));
        }
    }

    /// <summary>
    /// Get all items with cursor-based pagination (optimized for large datasets)
    /// </summary>
    [HttpGet("cursor")]
    [ProducesResponseType(typeof(WebApiResponse<List<ItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<WebApiResponse<List<ItemDto>>>> GetAllItemsWithCursor([FromQuery] string? cursor = null, [FromQuery] int pageSize = 20)
    {
        _logger.LogInformation("GET /api/items/cursor - Request received to get items with cursor. Cursor: {Cursor}, PageSize: {PageSize}", cursor, pageSize);
        try
        {
            // Normalize pageSize
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100; // Limit max page size

            var (items, nextCursor) = await _itemService.GetAllPagedAsync(cursor ?? string.Empty, pageSize);
            _logger.LogInformation("GET /api/items/cursor - Successfully retrieved {Count} items. NextCursor: {NextCursor}", 
                items.Count, nextCursor ?? "null");
            return Ok(WebApiResponse<List<ItemDto>>.Ok(items, items.Count, nextCursor));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("GET /api/items/cursor - Bad request: {ErrorMessage}", ex.Message);
            return BadRequest(WebApiResponse<List<ItemDto>>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET /api/items/cursor - Error retrieving items: {ErrorMessage}", ex.Message);
            return StatusCode(500, WebApiResponse<List<ItemDto>>.Fail("Internal server error"));
        }
    }

    /// <summary>
    /// Get item by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(WebApiResponse<ItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebApiResponse<ItemDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WebApiResponse<ItemDto>>> GetItemById(int id)
    {
        _logger.LogInformation("GET /api/items/{ItemId} - Request received to get item by ID", id);
        try
        {
            var item = await _itemService.GetByIdAsync(id);
            if (item == null)
            {
                _logger.LogWarning("GET /api/items/{ItemId} - Item not found", id);
                return NotFound(WebApiResponse<ItemDto>.Fail($"Item with id {id} not found"));
            }
            _logger.LogInformation("GET /api/items/{ItemId} - Successfully retrieved item: {ItemName}", id, item.Name);
            return Ok(WebApiResponse<ItemDto>.Ok(item));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET /api/items/{ItemId} - Error retrieving item: {ErrorMessage}", id, ex.Message);
            return StatusCode(500, WebApiResponse<ItemDto>.Fail("Internal server error"));
        }
    }

    /// <summary>
    /// Search items by query string (with optional pagination)
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(WebApiResponse<List<ItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<WebApiResponse<List<ItemDto>>>> SearchItems([FromQuery(Name = "query")] string? query = null, [FromQuery] int? page = null, [FromQuery] int? pageSize = null)
    {
        _logger.LogInformation("GET /api/items/search?query={Query} - Request received to search items. Page: {Page}, PageSize: {PageSize}", query, page, pageSize);
        try
        {
            if (page.HasValue && pageSize.HasValue)
            {
                // Normalize page and pageSize
                var normalizedPage = page.Value < 1 ? 1 : page.Value;
                var normalizedPageSize = pageSize.Value < 1 ? 10 : (pageSize.Value > 100 ? 100 : pageSize.Value);
                
                var (items, totalCount) = await _itemService.SearchPagedAsync(query ?? string.Empty, normalizedPage, normalizedPageSize);
                var totalPages = (int)Math.Ceiling(totalCount / (double)normalizedPageSize);
                _logger.LogInformation("GET /api/items/search?query={Query} - Successfully found {Count} items (Page {Page} of {TotalPages}, Total: {TotalCount})", 
                    query, items.Count, normalizedPage, totalPages, totalCount);
                return Ok(WebApiResponse<List<ItemDto>>.Ok(items, totalCount, normalizedPage, normalizedPageSize, totalPages));
            }
            else
            {
                var items = await _itemService.SearchAsync(query ?? string.Empty);
                _logger.LogInformation("GET /api/items/search?query={Query} - Successfully found {Count} items (all results)", query, items.Count);
                return Ok(WebApiResponse<List<ItemDto>>.Ok(items));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET /api/items/search?query={Query} - Error searching items: {ErrorMessage}", query, ex.Message);
            return StatusCode(500, WebApiResponse<List<ItemDto>>.Fail("Internal server error"));
        }
    }

    /// <summary>
    /// Create a new item
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(WebApiResponse<ItemDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(WebApiResponse<ItemDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WebApiResponse<ItemDto>>> CreateItem([FromBody] CreateItemRequestDto request)
    {
        _logger.LogInformation("POST /api/items - Request received to create new item. Name: {ItemName}", request?.Name);
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("POST /api/items - Invalid model state. Errors: {Errors}", 
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(WebApiResponse<ItemDto>.Fail("Invalid request data"));
            }

            var item = await _itemService.CreateAsync(request);
            _logger.LogInformation("POST /api/items - Successfully created item with ID: {ItemId}, Name: {ItemName}", 
                item.Id, item.Name);
            return CreatedAtAction(nameof(GetItemById), new { id = item.Id },
                WebApiResponse<ItemDto>.Ok(item));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("POST /api/items - Bad request: {ErrorMessage}", ex.Message);
            return BadRequest(WebApiResponse<ItemDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "POST /api/items - Error creating item: {ErrorMessage}", ex.Message);
            return StatusCode(500, WebApiResponse<ItemDto>.Fail("Internal server error"));
        }
    }

    /// <summary>
    /// Update an existing item
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(WebApiResponse<ItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebApiResponse<ItemDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(WebApiResponse<ItemDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WebApiResponse<ItemDto>>> UpdateItem(int id, [FromBody] UpdateItemRequestDto request)
    {
        _logger.LogInformation("PUT /api/items/{ItemId} - Request received to update item. Name: {ItemName}", id, request?.Name);
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("PUT /api/items/{ItemId} - Invalid model state. Errors: {Errors}", 
                    id, string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(WebApiResponse<ItemDto>.Fail("Invalid request data"));
            }

            var item = await _itemService.UpdateAsync(id, request!);
            _logger.LogInformation("PUT /api/items/{ItemId} - Successfully updated item. Name: {ItemName}", 
                id, item.Name);
            return Ok(WebApiResponse<ItemDto>.Ok(item));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("PUT /api/items/{ItemId} - Item not found: {ErrorMessage}", id, ex.Message);
            return NotFound(WebApiResponse<ItemDto>.Fail(ex.Message));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("PUT /api/items/{ItemId} - Bad request: {ErrorMessage}", id, ex.Message);
            return BadRequest(WebApiResponse<ItemDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PUT /api/items/{ItemId} - Error updating item: {ErrorMessage}", id, ex.Message);
            return StatusCode(500, WebApiResponse<ItemDto>.Fail("Internal server error"));
        }
    }

    /// <summary>
    /// Delete an item
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(WebApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WebApiResponse<string>>> DeleteItem(int id)
    {
        _logger.LogInformation("DELETE /api/items/{ItemId} - Request received to delete item", id);
        try
        {
            await _itemService.DeleteAsync(id);
            _logger.LogInformation("DELETE /api/items/{ItemId} - Successfully deleted item", id);
            return Ok(WebApiResponse<string>.Ok("Item deleted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("DELETE /api/items/{ItemId} - Item not found: {ErrorMessage}", id, ex.Message);
            return NotFound(WebApiResponse<string>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DELETE /api/items/{ItemId} - Error deleting item: {ErrorMessage}", id, ex.Message);
            return StatusCode(500, WebApiResponse<string>.Fail("Internal server error"));
        }
    }
}

