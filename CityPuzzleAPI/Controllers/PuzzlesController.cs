using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CityPuzzleAPI.Model;
using CityPuzzleAPI.Aspects;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Cors;

namespace CityPuzzleAPI.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("AllowMyOrigin")]
    [ApiController]
    [LogAspect]
    public class PuzzlesController : ControllerBase
    {
        private readonly CityPuzzleContext _context;
        private readonly IWebHostEnvironment _env;

        public PuzzlesController(CityPuzzleContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: api/Puzzles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Puzzle>>> GetPuzzles()
        {
            return await _context.Puzzles.ToListAsync();
        }

        [HttpGet("getfile")]
        public async Task<ActionResult> GetPuzzleImage(string name)
        {
            try
            {
                var file = Path.Combine(Directory.GetCurrentDirectory(), "Photos", name);
                var bytes = await System.IO.File.ReadAllBytesAsync(file);

                return File(bytes, "image/png", Path.GetFileName(file));
            }
            catch (Exception)
            {
                var file = Path.Combine(Directory.GetCurrentDirectory(), "Photos", "default.png");
                var bytes = await System.IO.File.ReadAllBytesAsync(file);

                return File(bytes, "image/png", Path.GetFileName(file));
            }
            
        }

        // GET: api/Puzzles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Puzzle>> GetPuzzle(int id)
        {
            var puzzle = await _context.Puzzles.FindAsync(id);

            if (puzzle == null)
            {
                return NotFound();
            }

            return puzzle;
        }

        // PUT: api/Puzzles/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPuzzle(int id, Puzzle puzzle)
        {
            if (id != puzzle.Id)
            {
                return BadRequest();
            }

            _context.Entry(puzzle).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PuzzleExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Puzzles
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Puzzle>> PostPuzzle(Puzzle puzzle)
        {
            _context.Puzzles.Add(puzzle);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPuzzle", new { id = puzzle.Id }, puzzle);
        }

        [Route("savefile")]
        [HttpPost]
        public JsonResult PostPuzzleImage()
        {
            try
            {
                var httpReqest = Request.Form;
                var postedFile = httpReqest.Files[0];
                string filename = postedFile.FileName;
                var path = _env.ContentRootPath + "/Photos/" +filename;

                using(var stream =new FileStream(path, FileMode.Create))
                {
                    postedFile.CopyTo(stream);
                }

                return new JsonResult(filename);

            }
            catch (Exception)
            {
                return new JsonResult("default.png");
            }
        }

        // DELETE: api/Puzzles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePuzzle(int id)
        {
            var puzzle = await _context.Puzzles.FindAsync(id);
            if (puzzle == null)
            {
                return NotFound();
            }

            _context.Puzzles.Remove(puzzle);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PuzzleExists(int id)
        {
            return _context.Puzzles.Any(e => e.Id == id);
        }
    }
}
