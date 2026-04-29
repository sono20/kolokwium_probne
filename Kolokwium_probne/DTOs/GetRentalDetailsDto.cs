namespace Kolokwium_probne.DTOs;

public class GetRentalDetailsDto
{
    public int Id { get; set; }
    public DateTime RentalDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public String Status { get; set; } = String.Empty;
    public List<GetMovieDetailsDto> Movies { get; set; } = [];
}