// usings here

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace DvdRentals.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilmsController : ControllerBase
    {
        public FilmsController(IFilmsRepository filmsRepository)
        {
            _filmsRepository = filmsRepository ?? throw new ArgumentNullException(nameof(filmsRepository));
        }

        private IFilmsRepository _filmsRepository;

        [HttpGet("")]
        public ActionResult<IEnumerable<Film>> List()
            => Ok(_filmsRepository.List());

        [HttpGet("{id}")]
        public ActionResult<Film> Get(int id)
            => Ok(_filmsRepository.Get(id));
    }
}