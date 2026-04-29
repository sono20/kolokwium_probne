namespace Kolokwium_probne.DTOs;

public class GetCustomerDetailsDto
{
    public String FirstName { get; set; } = String.Empty;
    public String LastName { get; set; } = String.Empty;
    public List<GetRentalDetailsDto> Rentals { get; set; } = [];
}