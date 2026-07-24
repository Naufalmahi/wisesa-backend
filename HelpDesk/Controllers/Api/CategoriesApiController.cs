using HelpDesk.Models.Entities;
using HelpDesk.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous] 
    public class CategoriesApiController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesApiController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _categoryService.GetAllAsync();

            return Ok(list.Select(c => new
            {
                c.Id,
                c.Name,
                c.Description
            }));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var c = await _categoryService.GetByIdAsync(id);
            if (c == null) return NotFound(new { message = "Kategori tidak ditemukan." });

            return Ok(new
            {
                c.Id,
                c.Name,
                c.Description
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    message = "Validasi gagal.",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description
            };

            var created = await _categoryService.CreateAsync(category);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, new
            {
                created.Id,
                created.Name,
                created.Description,
                message = "Kategori berhasil dibuat."
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CategoryDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var category = new Category
            {
                Id = id,
                Name = dto.Name,
                Description = dto.Description
            };

            var ok = await _categoryService.UpdateAsync(category);

            return ok
                ? Ok(new { message = "Kategori berhasil diubah." })
                : NotFound(new { message = "Kategori tidak ditemukan atau gagal diperbarui." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var ok = await _categoryService.DeleteAsync(id);

            return ok
                ? Ok(new { message = "Kategori berhasil dihapus." })
                : BadRequest(new { message = "Gagal menghapus kategori. Masih ada tiket yang menggunakan kategori ini." });
        }
    }

    public class CategoryDto
    {
        [Required(ErrorMessage = "Nama kategori tidak boleh kosong.")]
        [StringLength(100, ErrorMessage = "Nama kategori maksimal 100 karakter.")]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}