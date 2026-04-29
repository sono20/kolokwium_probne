namespace Kolokwium_probne.DTOs;

public class CreateRentalDetailsDto
{
    public DateOnly RentalDate { get; set; }
    public List<CreateMovieDetailsDto> Rentals { get; set; } = [];
}