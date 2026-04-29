using Kolokwium_probne.DTOs;

namespace Kolokwium_probne.Services;

public interface IDbService
{
    Task<GetCustomerDetailsDto> GetCustomerDetailsAsync(int id);
    Task CreateRentalWithMoviesAsync(int customedId, CreateRentalDetailsDto rentalDetails);
}