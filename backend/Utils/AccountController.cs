using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Utils;
using TSM.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace backend.Utils
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly ModelBase _dbContext;
        private readonly TokenHelper _tokenHelper;
        private readonly PasswordHelper _passwordHelper;

        public AccountController(ModelBase dbContext, TokenHelper tokenHelper, PasswordHelper passwordHelper)
        {
            _dbContext = dbContext;
            _tokenHelper = tokenHelper;
            _passwordHelper = passwordHelper;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserRequest request)
        {
            if (request.Email.Length < 5 || request.Password.Length < 8 || request.Name.Length < 2)
            {
                return BadRequest(new
                {
                    status = "error",
                    error = new
                    {
                        code = "ERR_VALIDATION",
                        code_int = 400,
                        title = "Validation Error",
                        message = "Invalid input parameters"
                    }
                });
            }

            var user = new User
            {
                Email = request.Email,
                Pwd = _passwordHelper.HashPassword(request.Password),
                DisplayName = request.Name,
                PreferredCats = request.PreferredCats?.Select(c => (long)c).ToList(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                user.Id,
                user.Email,
                user.DisplayName,
                user.PreferredCats,
                user.DefCustomPlace,
                user.Meta,
                user.CreatedAt,
                user.UpdatedAt
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginUserRequest request)
        {
            if (request.Email.Length < 5 || request.Password.Length < 8)
            {
                return BadRequest(new
                {
                    status = "error",
                    error = new
                    {
                        code = "ERR_VALIDATION",
                        code_int = 400,
                        title = "Validation Error",
                        message = "Invalid input parameters"
                    }
                });
            }

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !_passwordHelper.VerifyPassword(request.Password, user.Pwd))
            {
                return Unauthorized(new
                {
                    status = "error",
                    error = new
                    {
                        code = "ERR_UNAUTHENTICATED",
                        code_int = 401,
                        title = "You are not logged in",
                        message = "You have to be authenticated to perform this action"
                    }
                });
            }

            var token = _tokenHelper.GenerateToken(user);

            return Ok(new
            {
                Token = token,
                user.Id,
                user.Email,
                user.DisplayName,
                user.PreferredCats,
                user.DefCustomPlace,
                user.Meta,
                user.CreatedAt,
                user.UpdatedAt
            });
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized(new
                {
                    status = "error",
                    error = new
                    {
                        code = "ERR_UNAUTHENTICATED",
                        code_int = 401,
                        title = "You are not logged in",
                        message = "You have to be authenticated to perform this action"
                    }
                });
            }

            var user = await _dbContext.Users.FindAsync(long.Parse(userId));

            if (user == null)
            {
                return NotFound(new
                {
                    status = "error",
                    error = new
                    {
                        code = "ERR_NOT_FOUND",
                        code_int = 404,
                        title = "User Not Found",
                        message = "The requested user does not exist"
                    }
                });
            }

            return Ok(new
            {
                user.Id,
                user.Email,
                user.DisplayName,
                user.PreferredCats,
                user.DefCustomPlace,
                user.Meta,
                user.CreatedAt,
                user.UpdatedAt
            });
        }

        [HttpPost("updateself")]
        public async Task<IActionResult> UpdateProfile(UpdateUserRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized(new
                {
                    status = "error",
                    error = new
                    {
                        code = "ERR_UNAUTHENTICATED",
                        code_int = 401,
                        title = "You are not logged in",
                        message = "You have to be authenticated to perform this action"
                    }
                });
            }

            var user = await _dbContext.Users.FindAsync(long.Parse(userId));

            if (user == null)
            {
                return NotFound(new
                {
                    status = "error",
                    error = new
                    {
                        code = "ERR_NOT_FOUND",
                        code_int = 404,
                        title = "User Not Found",
                        message = "The requested user does not exist"
                    }
                });
            }

            user.Email = request.Email;
            user.DisplayName = request.DisplayName;
            user.PreferredCats = request.PreferredCats?.Select(c => (long)c).ToList();
            user.DefCustomPlace = request.DefCustomPlace;
            user.Meta = request.Meta;
            user.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return Ok(new { status = "ok" });
        }
    }
}
