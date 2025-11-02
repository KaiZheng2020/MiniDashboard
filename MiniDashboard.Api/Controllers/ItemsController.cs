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
    /// Get all items
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(WebApiResponse<List<ItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<WebApiResponse<List<ItemDto>>>> GetAllItems()
    {
        _logger.LogInformation("GET /api/items - Request received to get all items");
        try
        {
            var items = await _itemService.GetAllAsync();
            _logger.LogInformation("GET /api/items - Successfully retrieved {Count} items", items.Count);
            return Ok(WebApiResponse<List<ItemDto>>.Ok(items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET /api/items - Error retrieving all items: {ErrorMessage}", ex.Message);
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
    /// Search items by query string
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(WebApiResponse<List<ItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<WebApiResponse<List<ItemDto>>>> SearchItems([FromQuery] string query)
    {
        _logger.LogInformation("GET /api/items/search?query={Query} - Request received to search items", query);
        try
        {
            var items = await _itemService.SearchAsync(query);
            _logger.LogInformation("GET /api/items/search?query={Query} - Successfully found {Count} items", query, items.Count);
            return Ok(WebApiResponse<List<ItemDto>>.Ok(items));
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

            var item = await _itemService.UpdateAsync(id, request);
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

