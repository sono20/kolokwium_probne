using Kolokwium_probne.DTOs;
using Microsoft.Data.SqlClient;
using Kolokwium_probne.Exceptions;

namespace Kolokwium_probne.Services;

public class DbService : IDbService
{
    private readonly string _connectionString;
    public DbService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? String.Empty;
        
    }

    public async Task<GetCustomerDetailsDto> GetCustomerDetailsAsync(int id)
    {
        var query = """
                    select c.first_name AS FirstName,
                    c.last_name as LastName,
                    r.rental_id as RentalId,
                    r.rental_date as RentalDate,
                    r.return_date as ReturnDate,
                    s.name as StatusName,
                    m.title as MovieTitle,
                    ri.price_at_rental as PriceAtRental
                    from Customer c
                    join Rental r on r.customer_id = c.customer_id
                    join Status s on r.status_id = s.status_id
                    join Rental_Item ri on r.rental_id = ri.rental_id
                    join Movie m on ri.movie_id = m.movie_id
                    WHERE c.customer_id = @id
                    """;
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@id", id);

        await using var reader = await command.ExecuteReaderAsync();

        GetCustomerDetailsDto? result = null;
        
        var ordFirstName = reader.GetOrdinal("FirstName");
        var ordLastName = reader.GetOrdinal("LastName");
        var ordRentalId = reader.GetOrdinal("RentalId");
        var ordRentalDate = reader.GetOrdinal("RentalDate");
        var ordReturnDate = reader.GetOrdinal("ReturnDate");
        var ordStatusName = reader.GetOrdinal("StatusName");
        var ordMovieTitle = reader.GetOrdinal("MovieTitle");
        var ordPriceAtRental = reader.GetOrdinal("PriceAtRental");


        while (await reader.ReadAsync())
        {
            if (result is null)
            {
                result = new GetCustomerDetailsDto()
                {
                    FirstName = reader.GetString(ordFirstName),
                    LastName = reader.GetString(ordLastName),
                    Rentals = new List<GetRentalDetailsDto>()
                };
            }

            var rentalId = reader.GetInt32(ordRentalId);

            var rental = result.Rentals.FirstOrDefault(x => x.Id.Equals(rentalId));

            if (rental is null)
            {
                rental = new GetRentalDetailsDto
                {
                    Id = rentalId,
                    RentalDate = reader.GetDateTime(ordRentalDate),
                    ReturnDate = reader.IsDBNull(ordReturnDate) ? null : reader.GetDateTime(ordReturnDate),
                    Status = reader.GetString(ordStatusName),
                    Movies = new List<GetMovieDetailsDto>()
                };
                result.Rentals.Add(rental);
            }

            rental.Movies.Add(new GetMovieDetailsDto
            {
                title = reader.GetString(ordMovieTitle),
                priceAtRental = reader.GetDecimal(ordPriceAtRental)
            });
        }
        
        return result ?? throw new NotFoundException("No rentals found for the specified customer.");
        
    }

    public async Task CreateRentalDetailsAsync(int customerId, CreateRentalDetailsDto dto)
    {
        var createRentalQuery = """
                                Insert into Rental
                                values (@RentalDate, @ReturnDate, @CustomerId, @StatusId)
                                Select @@Identity;
                                """;

        var createRentalItemQuery = """
                                    Insert into Rental_Item
                                    values (@RentalId, @MovieId, @PriceAtRental)
                                    """;

        var getMovieIdQuery = """
                              Select movie_id
                              from Movie
                              where title = @MovieTitle;
                              """;

        var checkCustomerQuery = """
                                 Select 1
                                 from Customer
                                 where customer_id = @CustomerId
                                 """;
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var transaction = await connection.BeginTransactionAsync();

        await using var command = new SqlCommand();
        command.Connection = connection;
        command.Transaction = transaction as SqlTransaction;

        try
        {
            command.Parameters.Clear();
            command.CommandText = checkCustomerQuery;
            command.Parameters.AddWithValue("@CustomerId", customerId);

            var customerIdRes = await command.ExecuteScalarAsync();
            if (customerIdRes == null)
            {
                throw new NotFoundException($"Customer with id {customerId} not found.");
            }

            command.Parameters.Clear();
            command.CommandText = createRentalQuery;
            command.Parameters.AddWithValue("@RentalDate", dto.RentalDate);
            command.Parameters.AddWithValue("@ReturnDate", DBNull.Value);
            command.Parameters.AddWithValue("@CustomerId", customerId);
            command.Parameters.AddWithValue("@StatusId", 1);

            var rentalObject = await command.ExecuteScalarAsync();
            var rentalId = Convert.ToInt32(rentalObject);

            foreach (var movie in dto.Movies)
            {
                command.Parameters.Clear();
                command.CommandText = getMovieIdQuery;
                command.Parameters.AddWithValue("@MovieTitle", movie.Title);

                var movieObject = await command.ExecuteScalarAsync();
                if (movieObject == null)
                {
                    throw new NotFoundException($"Movie with title {movie.Title} not found.");
                }

                var movieId = Convert.ToInt32(movieObject);
                command.Parameters.Clear();
                command.CommandText = createRentalItemQuery;
                command.Parameters.AddWithValue("@RentalId", rentalId);
                command.Parameters.AddWithValue("@MovieId", movieId);
                command.Parameters.AddWithValue("@PriceAtRental", movie.RentalPrice);

                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    
    
}