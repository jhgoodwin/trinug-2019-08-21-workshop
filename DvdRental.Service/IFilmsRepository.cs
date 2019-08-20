using System.Collections.Generic;

public interface IFilmsRepository
{
    IEnumerable<Film> List();
    Film Get(int id);
}