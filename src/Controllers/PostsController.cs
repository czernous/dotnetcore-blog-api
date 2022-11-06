using System;
using api.Models;
using api.Db;
using api.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ganss.XSS;
using api.Filters;
using MongoDB.Driver;
using MongoDB.Bson;

#pragma warning disable 1591

namespace api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ApiKeyAuth]
    public class PostsController : ControllerBase
    {
        private readonly IMongoRepository<Category> _categoriesRepository;
        private readonly IMongoRepository<Post> _postsRepository;
        private readonly IMongoRepository<CldImage> _imageRepository;

        public PostsController(
            IMongoRepository<CldImage> imageRepository,
            IMongoRepository<Category> categoriesRepository,
            IMongoRepository<Post> postsRepository)
        {
            _imageRepository = imageRepository;
            _categoriesRepository = categoriesRepository;
            _postsRepository = postsRepository;
        }

        [HttpGet]
        public IEnumerable<Post> Get()
        {
            // TODO: implement pagination
            string searchTerm = Request.Query["search"];
            IEnumerable<Post> resultPosts = null;

            if (string.IsNullOrEmpty(searchTerm)) return _postsRepository.FilterBy(Id => true);

            IEnumerable<Post> postsByTitle = _postsRepository.FilterBy(p => p.Title.ToLower().Contains(searchTerm.ToLower()));
            IEnumerable<Post> postsByBody = _postsRepository.FilterBy(p => p.Body.ToLower().Contains(searchTerm.ToLower()));
            resultPosts = postsByTitle.Count() > 0 ? postsByTitle : postsByBody;

            return resultPosts; // returns empty list if no posts found

        }

        [HttpGet("{id:length(24)}", Name = "GetPost")]
        public ActionResult<Post> Get(string id)
        {
            Post post = _postsRepository.FindById(id);

            if (post == null) return NotFound();

            return post;
        }



        [HttpPost]
        public async Task<ActionResult<Post>> Create(Post post)
        {

            if (ModelState.IsValid)
            {
                if (post.Categories != null)
                {
                    var existingCategories = _categoriesRepository.FilterBy(Id => true);
                    List<Category> postCategories = post.Categories.ToList();
                    // find category name by ID and add to list
                    foreach (var c in postCategories)
                    {

                        foreach (var foundCategory in existingCategories)
                        {
                            if (foundCategory.Name == c.Name) c.Id = foundCategory.Id;
                        }

                        if (c._id == null) postCategories.Remove(c);
                    }
                }

                var postsFilter = Builders<Post>.Filter.Eq("Title", post.Title);

                var existingPosts = _postsRepository.FindOne(postsFilter);

                if (existingPosts != null) return BadRequest("The post with such title already exists. Please create a post with unique title");

                if (post.ImageUrl == null) return BadRequest("The post must contain a feature image. Please upload one");

                var imageFilter = Builders<CldImage>.Filter.Eq("SecureUrl", post.ImageUrl);

                CldImage image = _imageRepository.FindOne(imageFilter);

                if (image == null) return BadRequest("The image link must come from Cloudinary. Please use /images to upload an image to Cloudinary and use the link provided");
                post.ResponsiveImgs = image.ResponsiveUrls;
                image.UsedInPost = post;

                // TODO: ADD USEDINPOST ATTRIBUTE TO IMAGE WHEN ADDED TO POST

                var sanitazer = new HtmlSanitizer();
                var sanitizedBody = sanitazer.Sanitize(post.Body);

                post.Body = sanitizedBody;
                post.Meta.OpenGraph.Title = post.Title;
                post.Meta.OpenGraph.Description = post.Meta.MetaDescription;

                await _postsRepository.InsertOneAsync(post);

                return CreatedAtRoute("GetPost", new { id = post.Id.ToString() }, post);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, Post postIn)
        {
            var post = _postsRepository.FindById(id);
            var sanitizer = new HtmlSanitizer();

            if (post == null) return NotFound();

            if (postIn.Categories != null)
            {

                var existingCateories = _categoriesRepository.FilterBy(Id => true);
                List<Category> postInCategories = postIn.Categories.ToList();
                foreach (var c in postInCategories)
                {

                    foreach (var foundCategory in existingCateories)
                    {
                        if (foundCategory.Name == c.Name) c.Id = foundCategory.Id;
                    }


                    if (c._id == null) postInCategories.Remove(c);
                }
            }

            // add id to post object
            postIn.Id = new ObjectId(id);

            if (postIn.ImageUrl != null && postIn.ImageUrl != post.ImageUrl)
            {
                // Update image urls if new image url is provided
                var imageFilter = Builders<CldImage>.Filter.Eq("ImageUrl", post.ImageUrl);

                CldImage image = _imageRepository.FindOne(imageFilter);

                if (image == null) return BadRequest("ImageUrl in the Post object is invalid or not found in the database");

                postIn.ResponsiveImgs = image.ResponsiveUrls;
                image.UsedInPost = postIn;
                postIn.Meta.OpenGraph.Title = postIn.Title;
                await _imageRepository.ReplaceOneAsync(image);


            }


            var sanitizedBody = sanitizer.Sanitize(postIn.Body);
            postIn.Body = sanitizedBody;
            postIn.UpdatedAt = DateTime.UtcNow;
            postIn.Meta.OpenGraph.Description = postIn.Meta.MetaDescription;
            postIn.Meta.OpenGraph.Title = postIn.Title;

            await _postsRepository.ReplaceOneAsync(postIn);

            return NoContent();
        }

        [HttpDelete("{id:Length(24)}")]
        public IActionResult Delete(string id)
        {
            var post = _postsRepository.FindById(id);

            if (post == null) return NotFound();

            var deletedPost = post.Id.ToString();

            _postsRepository.DeleteById(deletedPost);

            return NoContent();
        }
    }
}