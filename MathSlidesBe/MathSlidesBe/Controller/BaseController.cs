using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MathSlidesBe.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController<TEntity> : ControllerBase where TEntity : class
    {
        private readonly MathSlidesDbContext _context;
        private readonly DbSet<TEntity> _dbSet;

        public BaseController(MathSlidesDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<TEntity>();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TEntity>>> GetAll()
        {
            return Ok(await _dbSet.ToListAsync());
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<TEntity>> GetById(Guid id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        [HttpPost]
        public async Task<ActionResult<TEntity>> Create(TEntity entity)
        {
            _dbSet.Add(entity);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = GetKeyValue(entity) }, entity);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, TEntity entity)
        {
            var key = GetKeyValue(entity);
            if (!id.Equals(key))
                return BadRequest("ID mismatch");

            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity == null)
                return NotFound();

            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static object? GetKeyValue(TEntity entity)
        {
            var keyProperty = typeof(TEntity).GetProperties()
                .FirstOrDefault(p => p.Name.EndsWith("ID", StringComparison.OrdinalIgnoreCase));
            return keyProperty?.GetValue(entity);
        }
    }
}
