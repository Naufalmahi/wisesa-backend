using HelpDesk.Data;
using HelpDesk.Models.Entities;
using HelpDesk.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;
        public CategoryService(ApplicationDbContext context) { _context = context; }

        public async Task<IEnumerable<Category>> GetAllAsync()
            => await _context.Categories.OrderBy(c => c.Name).ToListAsync();

        public async Task<Category?> GetByIdAsync(Guid id)
            => await _context.Categories.FindAsync(id);

        public async Task<Category> CreateAsync(Category category)
        {
            category.CreatedAt = DateTime.UtcNow;
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<bool> UpdateAsync(Category category)
        {
            var existing = await _context.Categories.FindAsync(category.Id);
            if (existing == null) return false;
            existing.Name = category.Name;
            existing.Description = category.Description;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var category = await _context.Categories.Include(c => c.Tickets).FirstOrDefaultAsync(c => c.Id == id);
            if (category == null || category.Tickets.Any()) return false;
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
