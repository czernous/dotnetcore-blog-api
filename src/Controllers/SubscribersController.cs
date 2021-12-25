using api.Models;
using api.Services;
using api.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;
using api.Filters;
using Microsoft.AspNetCore.Mvc;

#pragma warning disable 1591 

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKeyAuth]
    public class SubscribersController : ControllerBase
    {


        private readonly SubscriberService _subscriberService;

        public SubscribersController(SubscriberService subscriberService) =>
         _subscriberService = subscriberService;

        [HttpGet]
        public async Task<ActionResult<List<Subscriber>>> Get() =>
            await _subscriberService.GetAllAsync();

        [HttpGet("{id:length(24)}", Name = "GetSubscriber")]
        public ActionResult<Subscriber> Get(string id)
        {
            var subscriber = _subscriberService.GetById(id);

            if (subscriber == null) return NotFound();

            return subscriber;
        }

        [HttpPost]
        public async Task<ActionResult<Subscriber>> CreateOne(Subscriber subscriber)
        {
            if (ModelState.IsValid)
            {
                var foundSub = await _subscriberService.GetByEmailAsync(subscriber.Email);

                if (foundSub != null) return BadRequest("A subscriber with this email address already exists");
                if (!RegexUtils.IsValidEmail(subscriber.Email)) return BadRequest("Provided email address is not valid");

                await _subscriberService.CreateAsync(subscriber);

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
            var subscriber = _subscriberService.GetById(id);

            if (subscriber == null) return NotFound();

            if (!RegexUtils.IsValidEmail(subscriberIn.Email)) return BadRequest("Provided email address is not valid");

            subscriberIn.Id = id;

            await _subscriberService.UpdateAsync(id, subscriberIn);

            return NoContent();
        }

        [HttpDelete("{id:Length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var subscriber = _subscriberService.GetById(id);

            if (subscriber == null) return NotFound();

            await _subscriberService.RemoveAsync(subscriber.Id);

            return NoContent();
        }
    }

}