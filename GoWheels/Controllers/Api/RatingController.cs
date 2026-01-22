using GoWheels.Models;
using GoWheels.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GoWheels.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RatingController : ControllerBase
    {
        private readonly IRatingsService _ratingsService;
        private readonly IUsersService _usersService;

        public RatingController(IRatingsService ratingsService, IUsersService usersService)
        {
            _ratingsService = ratingsService;
            _usersService = usersService;
        }

        // POST: api/Rating/Post
        [HttpPost("Post")]
        public async Task<IActionResult> RatePost([FromBody] PostRatingDto ratingDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var rating = new RatingPost
            {
                Value = ratingDto.Value,
                RatedPostId = ratingDto.PostId,
                OwnerId = userId
            };

            var success = await _ratingsService.SaveRatingPostAsync(rating);
            if (success)
            {
                return Ok(new { message = "Rating saved successfully." });
            }

            return BadRequest("Failed to save rating.");
        }

        // POST: api/Rating/User
        [HttpPost("User")]
        public async Task<IActionResult> RateUser([FromBody] UserRatingDto ratingDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (userId == ratingDto.UserId)
            {
                return BadRequest("You cannot rate yourself.");
            }

            var rating = new RatingUser
            {
                Value = ratingDto.Value,
                RatedUserId = ratingDto.UserId,
                OwnerId = userId
            };

            var success = await _ratingsService.SaveRatingUserAsync(rating);
            if (success)
            {
                return Ok(new { message = "Rating saved successfully." });
            }

            return BadRequest("Failed to save rating.");
        }
    }

    public class PostRatingDto
    {
        public string PostId { get; set; } = string.Empty;
        public float Value { get; set; }
    }

    public class UserRatingDto
    {
        public string UserId { get; set; } = string.Empty;
        public float Value { get; set; }
    }
}
