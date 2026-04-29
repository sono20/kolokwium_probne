using Kolokwium_probne.Services;
using Kolokwium_probne.Exceptions;
using Kolokwium_probne.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kolokwium_probne.Controllers;
[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly IDbService _dbService;

    public CustomersController(IDbService dbService)
    {
        _dbService = dbService;
    }

    [Route("{id}/rentals")]
    [HttpGet]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var result = await _dbService.GetCustomerDetailsAsync(id);
            return Ok(result);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);

        }
    }

    [Route("{id}/rentals")]
    [HttpPost]
    public async Task<IActionResult> Post([FromRoute] int id, [FromBody] CreateRentalDetailsDto dto)
    {
        if (!dto.Movies.Any())
        {
            return BadRequest("At least one item is required. ");
        }

        try
        {
            await _dbService.CreateRentalDetailsAsync(id, dto);
            return Created($"api/customers/{id}/rentals", dto);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [Route("{id}/rentals/{rentalId}")]
    [HttpDelete]
    public async Task<IActionResult> Delete([FromRoute] int id, [FromRoute] int rentalId)
    {
        try
        {
            //await _dbService.deleteRentalAsync(id, rentalId);
            return NoContent();
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [Route("{id}/rentals/{rentalId}")]
    [HttpPut]
    public async Task<IActionResult> Put([FromRoute] int id, [FromRoute] int rentalId,
        [FromBody] CreateRentalDetailsDto dto) //DeleteRentalDetailsDto
    {
        try
        {
            //await _dbService.UpdateRentalAsync(id, rentalId, dto); 
            return Ok();
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
        
    }
}
