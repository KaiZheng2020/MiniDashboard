using MiniDashboard.Api.Models.Common;
using MiniDashboard.Api.Models.DTOs;
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
        try
        {
            var items = await _itemService.GetAllAsync();
            return Ok(WebApiResponse<List<ItemDto>>.Ok(items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all items");
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
        try
        {
            var item = await _itemService.GetByIdAsync(id);
            if (item == null)
            {
                return NotFound(WebApiResponse<ItemDto>.Fail($"Item with id {id} not found"));
            }
            return Ok(WebApiResponse<ItemDto>.Ok(item));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving item with id: {ItemId}", id);
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
        try
        {
            var items = await _itemService.SearchAsync(query);
            return Ok(WebApiResponse<List<ItemDto>>.Ok(items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching items with query: {Query}", query);
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
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(WebApiResponse<ItemDto>.Fail("Invalid request data"));
            }

            var item = await _itemService.CreateAsync(request);
            return CreatedAtAction(nameof(GetItemById), new { id = item.Id },
                WebApiResponse<ItemDto>.Ok(item));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(WebApiResponse<ItemDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating item");
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
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(WebApiResponse<ItemDto>.Fail("Invalid request data"));
            }

            var item = await _itemService.UpdateAsync(id, request);
            return Ok(WebApiResponse<ItemDto>.Ok(item));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(WebApiResponse<ItemDto>.Fail(ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(WebApiResponse<ItemDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating item with id: {ItemId}", id);
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
        try
        {
            await _itemService.DeleteAsync(id);
            return Ok(WebApiResponse<string>.Ok("Item deleted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(WebApiResponse<string>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting item with id: {ItemId}", id);
            return StatusCode(500, WebApiResponse<string>.Fail("Internal server error"));
        }
    }
}

