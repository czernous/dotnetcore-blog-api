using api.Models;
using api.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using api.Filters;
using Microsoft.AspNetCore.Mvc;

#pragma warning disable 1591 

namespace api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ApiKeyAuth]
    public class CategoriesController : ControllerBase
    {


        private readonly CategoryService _categoryService;
        private readonly PostService _postService;

        public CategoriesController(CategoryService categoryService, PostService postService)
        {
            _categoryService = categoryService;
            _postService = postService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Category>>> Get() =>
            await _categoryService.GetAllAsync();

        [HttpGet("{id:length(24)}", Name = "GetCategory")]
        public ActionResult<Category> Get(string id)
        {
            var category = _categoryService.GetById(id);

            if (category == null) return NotFound();

            return category;
        }

        [HttpPost]
        public async Task<ActionResult<Category>> CreateOne(Category category)
        {
            if (ModelState.IsValid)
            {
                var foundCategory = await _categoryService.GetByNameAsync(category.Name);

                if (foundCategory != null) return BadRequest("This category already exists");

                await _categoryService.CreateAsync(category);

                return CreatedAtRoute("GetCategory", new { id = category.Id.ToString() }, category);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, Category categoryIn)
        {
            var category = _categoryService.GetById(id);

            if (category == null) return NotFound();

            categoryIn.Id = id;

            await _postService.UpdMatchingCatAsync(category, categoryIn);
            await _categoryService.UpdateAsync(id, categoryIn);

            return NoContent();
        }

        [HttpDelete("{id:Length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var category = _categoryService.GetById(id);

            if (category == null) return NotFound();

            await _categoryService.RemoveAsync(category.Id);
            await _postService.DelMatchingCatAsync(category);

            return NoContent();
        }
    }

}