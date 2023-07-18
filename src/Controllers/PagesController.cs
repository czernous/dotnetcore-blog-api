using api.Models;
using api.Filters;
using api.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using System;

#pragma warning disable 1591
namespace api.Controllers
{

    [Route("[controller]")]
    [ApiController]
    [ApiKeyAuth]

    public class PagesController : ControllerBase
    {
        private readonly IMongoRepository<Page> _pagesRepository;

        public PagesController(
            IMongoRepository<Page> pagesRepository)
        {
            _pagesRepository = pagesRepository;

        }

        [HttpGet]
        public IEnumerable<Page> Get() => _pagesRepository.FilterBy(Id => true);

        [HttpGet("{slug}")]
        public async Task<ActionResult<Page>> GetBySlug(string slug)

        {
            var pageFilter = Builders<Page>.Filter.Eq("Slug", slug);
            var page = await _pagesRepository.FindOneAsync(pageFilter);

            if (page == null)
            {
                return NotFound();
            }

            return page;
        }

        [HttpPost(Name = "CreatePage")]
        public async Task<ActionResult<Page>> Create(Page page)
        {

            var pageFilter = Builders<Page>.Filter.Eq("Slug", page.Slug);
            var foundPage = await _pagesRepository.FindOneAsync(pageFilter);

            if (foundPage != null) return BadRequest("This page already exists") as BadRequestObjectResult;

            await _pagesRepository.InsertOneAsync(page);

            return CreatedAtRoute("CreatePage", new { id = page._id }, page);

        }

        [HttpPut("{slug}")]
        public async Task<IActionResult> Update(string slug, Page pageIn)
        {

            var pageFilter = Builders<Page>.Filter.Eq("Slug", pageIn.Slug);
            var foundPage = await _pagesRepository.FindOneAsync(pageFilter);

            if (foundPage == null) return NotFound();

            pageIn.UpdatedAt = DateTime.UtcNow;
            pageIn.Id = new ObjectId(foundPage._id);

            await _pagesRepository.ReplaceOneAsync(pageIn);

            return NoContent();

        }

        [HttpDelete("{slug}")]
        public IActionResult Delete(string slug)
        {
            var pageFilter = Builders<Page>.Filter.Eq("Slug", slug);
            var foundPage = _pagesRepository.FindOne(pageFilter);

            _pagesRepository.DeleteById(foundPage.Id.ToString());

            return NoContent();
        }


    }
}