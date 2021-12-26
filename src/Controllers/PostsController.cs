using System;
using api.Models;
using api.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ganss.XSS;
using api.Filters;

#pragma warning disable 1591 

namespace api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ApiKeyAuth]
    public class PostsController : ControllerBase
    {
        private readonly PostService _postService;
        private readonly CategoryService _categoryService;

        private readonly ImageService _imageService;

        public PostsController(PostService postService, CategoryService categoryService, ImageService imageService)
        {
            _postService = postService;
            _categoryService = categoryService;
            _imageService = imageService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Post>>> Get() =>
            await _postService.GetAllAsync();
        // TODO: ADD INMEMORY OR DISTRIBUTED CACHE TO CACHE GET REQUESTS AND UPDATE CACHE ON PUT/POST

        [HttpGet("{id:length(24)}", Name = "GetPost")]
        public ActionResult<Post> Get(string id)
        {
            var post = _postService.GetById(id);

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
                    var existingCateories = await _categoryService.GetAllAsync();
                    // find category name by ID and add to list
                    foreach (var c in post.Categories.ToList())
                    {
                        existingCateories.ForEach(foundCategory => { if (foundCategory.Name == c.Name) c.Id = foundCategory.Id; });
                        if (c.Id == null) post.Categories.Remove(c);
                    }
                }

                if (post.ImageUrl == null) return BadRequest("The post must contain a feature image. Please upload one");

                CldImage image = await _imageService.GetByUrlAsync(post.ImageUrl);
                post.ResponsiveImgs = image.ResponsiveUrls;
                image.UsedInPost = post;

                // TODO: ADD USEDINPOST ATTRIBUTE TO IMAGE WHEN ADDED TO POST

                var sanitazer = new HtmlSanitizer();
                var sanitizedBody = sanitazer.Sanitize(post.Body);

                post.Body = sanitizedBody;

                await _postService.CreateAsync(post);

                return CreatedAtRoute("GetPost", new { id = post.Id.ToString() }, post);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [HttpPut("{id:length(24)}")]
        public async Task<ActionResult<Post>> Update(string id, Post postIn)
        {
            var post = _postService.GetById(id);
            var sanitizer = new HtmlSanitizer();

            if (post == null) return NotFound();

            if (postIn.Categories != null)
            {

                var existingCateories = await _categoryService.GetAllAsync();
                foreach (var c in postIn.Categories.ToList())
                {
                    existingCateories.ForEach(foundCategory => { if (foundCategory.Name == c.Name) c.Id = foundCategory.Id; });

                    if (c.Id == null) postIn.Categories.Remove(c);
                }
            }

            // add id to post object
            postIn.Id = id;

            if (postIn.ImageUrl != null && postIn.ImageUrl != post.ImageUrl)
            {
                // Update image urls if new image url is provided
                CldImage image = await _imageService.GetByUrlAsync(postIn.ImageUrl);

                if (image == null) return BadRequest("ImageUrl in the Post object is invalid or not found in the database");

                postIn.ResponsiveImgs = image.ResponsiveUrls;
                image.UsedInPost = postIn;
                await _imageService.UpdateAsync(image.Id, image);

            }


            var sanitizedBody = sanitizer.Sanitize(postIn.Body);
            postIn.Body = sanitizedBody;
            postIn.Created = post.Created;

            await _postService.UpdateAsync(id, postIn);

            return NoContent();
        }

        [HttpDelete("{id:Length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var post = _postService.GetById(id);

            if (post == null) return NotFound();

            await _postService.RemoveAsync(post.Id);

            return NoContent();
        }
    }
}