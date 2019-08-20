using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Dapper;

public class PostgresFilmsRepository: IFilmsRepository
{
    private readonly DbConnection _dbConnection;

    public PostgresFilmsRepository(DbConnection dbConnection)
    {
        _dbConnection = dbConnection ?? throw new ArgumentNullException();
    }

    public IEnumerable<Film> List()
        => _dbConnection.Query<Film>(
            "SELECT film_id as FilmId, title as Title, description as Description, release_year as ReleaseYear FROM film");

    public Film Get(int id)
        => _dbConnection.QuerySingle<Film>(
            "SELECT film_id as FilmId, title as Title, description as Description, release_year as ReleaseYear FROM film WHERE film_id = :film_id", 
            new { film_id = id});
}