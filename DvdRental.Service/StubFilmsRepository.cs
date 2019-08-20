using System.Collections.Generic;
using System.Linq;

public class StubFilmsRepository : IFilmsRepository
{
    private List<Film> _films = new List<Film>()
    {
        new Film { FilmId = 1, Title = "Test Film1", Description = "My Film1", ReleaseYear = 2018 },
        new Film { FilmId = 2, Title = "Test Film2", Description = "My Film2", ReleaseYear = 2019 }
    };
    
    public IEnumerable<Film> List() => _films;
    public Film Get(int id) => _films.First(f => f.FilmId.Equals(id));
}