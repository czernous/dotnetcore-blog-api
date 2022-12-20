using api.Models;
using api.Filters;
using api.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;

#pragma warning disable 1591

namespace api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ApiKeyAuth]
    public class CategoriesController : ControllerBase
    {


        private readonly IMongoRepository<Category> _categoriesRepository;
        private readonly IMongoRepository<Post> _postsRepository;

        public CategoriesController(
            IMongoRepository<Category> categoriesRepository,
            IMongoRepository<Post> postsRepository)
        {
            _categoriesRepository = categoriesRepository;
            _postsRepository = postsRepository;
        }

        [HttpGet]
        public IEnumerable<Category> Get() => _categoriesRepository.FilterBy(Id => true);



        [HttpGet("{id:length(24)}", Name = "GetCategory")]
        public ActionResult<Category> Get(string id)
        {
            var category = _categoriesRepository.FindById(id);

            if (category == null) return NotFound();

            return category;
        }

        [HttpPost]
        public ActionResult<Category> CreateOne(Category category)
        {
            if (ModelState.IsValid)
            {

                var categoryFilter = Builders<Category>.Filter.Eq("Name", category.Name);

                var foundCategory = _categoriesRepository.FindOne(categoryFilter);

                if (foundCategory != null) return BadRequest("This category already exists") as BadRequestObjectResult;

                _categoriesRepository.InsertOne(category);

                return CreatedAtRoute("GetCategory", new { id = category._id }, category);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [HttpPut("{id:length(24)}")]
        public async Task<ActionResult<Category>> Update(string id, Category categoryIn)
        {
            var category = _categoriesRepository.FindById(id);

            if (category == null) return NotFound();

            categoryIn.Id = new ObjectId(id);

            var categoryFilter = Builders<Category>.Filter.Eq("Name", categoryIn.Name);
            var foundCategory = _categoriesRepository.FindOne(categoryFilter);

            if (foundCategory != null) return BadRequest("This category already exists") as BadRequestObjectResult;

            var postsFilter = Builders<Post>.Filter.AnyEq("Categories", category);
            var postsUpdate = Builders<Post>.Update.Set("Categories.$[].Name", categoryIn.Name);

            await _postsRepository.ReplaceManyAsync(postsFilter, postsUpdate);



            await _categoriesRepository.ReplaceOneAsync(categoryIn);

            return NoContent();
        }

        [HttpDelete("{id:Length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var category = _categoriesRepository.FindById(id);

            if (category == null) return NotFound();

            var postsFilter = Builders<Post>.Filter.AnyEq("Categories", category);
            var postsUpdate = Builders<Post>.Update.Pull("Categories", category);

            _categoriesRepository.DeleteById(category.Id.ToString());
            await _postsRepository.ReplaceManyAsync(postsFilter, postsUpdate);

            return NoContent();
        }
    }

}