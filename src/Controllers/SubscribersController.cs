using api.Models;
using api.Interfaces;
using api.Utils;
using api.Filters;
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
    public class SubscribersController : ControllerBase
    {


        private readonly IMongoRepository<Subscriber> _subscribersRepository;

        public SubscribersController(IMongoRepository<Subscriber> subscribersRepository) =>
         _subscribersRepository = subscribersRepository;

        [HttpGet]
        public IEnumerable<Subscriber> Get() => _subscribersRepository.FilterBy(Id => true);

        [HttpGet("{id:length(24)}", Name = "GetSubscriber")]
        public ActionResult<Subscriber> Get(string id)
        {
            var subscriber = _subscribersRepository.FindById(id);

            if (subscriber == null) return NotFound();

            return subscriber;
        }

        [HttpPost]
        public async Task<ActionResult<Subscriber>> CreateOne(Subscriber subscriber)
        {
            if (ModelState.IsValid)
            {
                var subscriberFilter = Builders<Subscriber>.Filter.Eq("Name", subscriber.Email);

                var foundSub = _subscribersRepository.FindOne(subscriberFilter);

                if (foundSub != null) return BadRequest("A subscriber with this email address already exists");
                if (!RegexUtils.IsValidEmail(subscriber.Email)) return BadRequest("Provided email address is not valid");

                await _subscribersRepository.InsertOneAsync(subscriber);

                return CreatedAtRoute("GetSubscriber", new { id = subscriber.Id.ToString() }, subscriber);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, Subscriber subscriberIn)
        {
            var subscriber = _subscribersRepository.FindById(id);

            if (subscriber == null) return NotFound();

            if (!RegexUtils.IsValidEmail(subscriberIn.Email)) return BadRequest("Provided email address is not valid");

            subscriberIn.Id = new ObjectId(id);

            await _subscribersRepository.ReplaceOneAsync(subscriberIn);

            return NoContent();
        }

        [HttpDelete("{id:Length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var subscriber = _subscribersRepository.FindById(id);

            if (subscriber == null) return NotFound();

            await _subscribersRepository.DeleteByIdAsync(subscriber.Id.ToString());

            return NoContent();
        }
    }

}