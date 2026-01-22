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
    public class CommentController : ControllerBase
    {
        private readonly ICommentsService _commentsService;

        public CommentController(ICommentsService commentsService)
        {
            _commentsService = commentsService;
        }

        // POST: api/Comment
        [HttpPost]
        public async Task<IActionResult> PostComment([FromBody] CommentDto commentDto)
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

            var comment = new Comment
            {
                Body = commentDto.Body,
                PostId = commentDto.PostId,
                UserId = userId
            };

            var success = await _commentsService.AddCommentAsync(comment);
            if (success)
            {
                // Return the created comment with user info for the UI
                var createdComment = await _commentsService.GetCommentByIdAsync(comment.Id);
                return Ok(new
                {
                    id = createdComment.Id,
                    body = createdComment.Body,
                    createdAt = createdComment.CreatedAt,
                    userName = createdComment.User.Name,
                    userId = createdComment.UserId
                });
            }

            return BadRequest("Failed to add comment.");
        }

        // DELETE: api/Comment/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(string id)
        {
            var comment = await _commentsService.GetCommentByIdAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (comment.UserId != userId && !User.IsInRole("ADMIN"))
            {
                return Forbid();
            }

            var success = await _commentsService.DeleteCommentAsync(Guid.Parse(id));
            if (success)
            {
                return NoContent();
            }

            return BadRequest("Failed to delete comment.");
        }
    }

    public class CommentDto
    {
        public string Body { get; set; } = string.Empty;
        public string PostId { get; set; } = string.Empty;
    }
}
