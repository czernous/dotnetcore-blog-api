using System;
using api.Models;
using api.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ganss.Xss;
using api.Filters;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Extensions.Logging;

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

        private readonly ILogger<PostsController> _logger;

        public PostsController(
            IMongoRepository<CldImage> imageRepository,
            IMongoRepository<Category> categoriesRepository,
            IMongoRepository<Post> postsRepository,
              ILogger<PostsController> logger)

        {
            _imageRepository = imageRepository;
            _categoriesRepository = categoriesRepository;
            _postsRepository = postsRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<PagedData<Post>> Get(string? search, string? sortOrder, int? page, int? pageSize)
        {

            PagedData<Post> resultPosts = null;

            var sortDefinition = sortOrder == "asc"
                ? Builders<Post>.Sort.Ascending(p => p.Id)
                : Builders<Post>.Sort.Descending(p => p.Id);

            if (string.IsNullOrEmpty(search))
                return await _postsRepository.FilterByAndPaginateAsync(Id => true, sortDefinition, page, pageSize);

            PagedData<Post> postsByTitle = await _postsRepository
                .FilterByAndPaginateAsync(
                    p => p.Title.ToLower().Contains(search.ToLower()), sortDefinition, page, pageSize
                );

            resultPosts = postsByTitle.Data.Any() ? postsByTitle
                : await _postsRepository
                    .FilterByAndPaginateAsync(
                        p => p.Body.ToLower().Contains(search.ToLower()), sortDefinition, page, pageSize
                    );

            return resultPosts; // returns empty list if no posts found

        }

        [HttpGet("{id:length(24)}", Name = "GetPost")]
        public ActionResult<Post> Get(string id)
        {
            if (!string.IsNullOrWhiteSpace(id) && ObjectId.TryParse(id, out _))
            {
                Post post = _postsRepository.FindById(id);

                if (post != null)

                    return post;
            }
            return NotFound();
        }

        [HttpGet("slug/{slug}", Name = "GetPostBySlug")]
        public ActionResult<Post> GetBySlug(string slug)
        {
            var postFilter = Builders<Post>.Filter.Eq("Slug", slug);
            Post postBySlug = _postsRepository.FindOne(postFilter);
            if (postBySlug == null) return NotFound();
            return postBySlug;
        }




        [HttpPost(Name = "CreatePost")]
        public async Task<ActionResult<Post>> Create(Post post)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    var batchTasks = new List<Task>
                {
                    ProcessCategoriesAsync(post),
                    ValidateAndGetImageAsync(post.ImageUrl),
                    CheckPostUniquenessAsync(post)
                };

                    if (post.ImageUrl == null) return BadRequest("The post must contain a feature image. Please upload one");

                    await Task.WhenAll(batchTasks);
                    CldImage image = ((Task<CldImage>)batchTasks[1]).Result;
                    if (image == null)
                    {
                        return BadRequest("The image link must come from Cloudinary. Please use /images to upload an image to Cloudinary and use the link provided");
                    }
                    post.ResponsiveImgs = image.ResponsiveUrls;
                    post.BlurredImageUrl = image.BlurredImageUrl;
                    post.ImageAltText = image.Name;
                    image.UsedInPost = post;

                    // TODO: ADD USEDINPOST ATTRIBUTE TO IMAGE WHEN ADDED TO POST

                    SanitizeBody(post);

                    post.Meta.OpenGraph.Title = post.Title;
                    post.Meta.OpenGraph.Description = post.Meta.MetaDescription;

                    await _postsRepository.InsertOneAsync(post);

                    return CreatedAtRoute("CreatePost", new { id = post.Id.ToString() }, post);
                }
                else
                {
                    return BadRequest(ModelState);
                }
            }
            catch (Exception ex)
            {

                _logger.LogError($"An error occurred: {ex}");

                // Return an appropriate error response
                return StatusCode(500, "An internal server error occurred");
            }
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, Post postIn)
        {
            var post = await _postsRepository.FindByIdAsync(id);


            if (post == null) return NotFound();

            await ProcessCategoriesAsync(postIn);

            // add id to post object
            postIn.Id = new ObjectId(id);


            if (postIn.Title != post.Title || postIn.Slug != post.Slug)
            {
                await CheckPostUniquenessAsync(postIn);

            }

            if (postIn.ImageUrl != null)
            {
                CldImage image = await ValidateAndGetImageAsync(postIn.ImageUrl);

                if (image == null) return BadRequest("ImageUrl in the Post object is invalid or not found in the database");

                postIn.ResponsiveImgs = image.ResponsiveUrls;
                postIn.BlurredImageUrl = image.BlurredImageUrl;
                postIn.ImageAltText = image.Name;
                image.UsedInPost = postIn;
                postIn.Meta.OpenGraph.Title = postIn.Title;
                await _imageRepository.ReplaceOneAsync(image);

            }


            SanitizeBody(postIn);
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

        private async Task<CldImage> ValidateAndGetImageAsync(string imageUrl)
        {
            _logger.LogInformation("Checking if the image reference exists in DB");
            var imageFilter = Builders<CldImage>.Filter.Eq("SecureUrl", imageUrl);
            return await _imageRepository.FindOneAsync(imageFilter);
        }

        private void SanitizeBody(Post post)
        {
            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedAttributes.Add("class");
            post.Body = sanitizer.Sanitize(post.Body);
        }

        private async Task ProcessCategoriesAsync(Post post)
        {
            if (post.Categories != null)
            {
                _logger.LogInformation("Processing post categories");

                var existingCategories = await _categoriesRepository.FilterByAsync(Id => true);

                var categoryTasks = post.Categories.Select(async category =>
                {
                    var existingCategory = existingCategories.ToList().Find(ec => ec.Name == category.Name);

                    if (existingCategory != null)
                    {
                        // Category exists, reference it
                        category.Id = existingCategory.Id;
                    }
                    else
                    {
                        // Category doesn't exist, add it to the repository and set the reference
                        await _categoriesRepository.InsertOneAsync(category);
                    }

                    return category;
                });

                post.Categories = await Task.WhenAll(categoryTasks);
            }
        }


        private async Task CheckPostUniquenessAsync(Post post)
        {
            _logger.LogInformation("Checking post uniqueness");

            var filter = Builders<Post>.Filter.Or(
                Builders<Post>.Filter.Eq("Title", post.Title),
                Builders<Post>.Filter.Eq("Slug", post.Slug)
            );

            var existingPost = await _postsRepository.FindOneAsync(filter);

            if (existingPost != null)
            {
                if (existingPost.Title == post.Title)
                {
                    ModelState.AddModelError("Title", "The post with such title already exists. Please create a post with a unique title");
                }

                if (existingPost.Slug == post.Slug)
                {
                    ModelState.AddModelError("Slug", "The post with such slug already exists. Please make sure slugs are unique");
                }
            }
        }
    }

}