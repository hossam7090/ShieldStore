using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ShieldStore.Configurations;
using ShieldStore.DTOs;
using ShieldStore.helpers;
using ShieldStore.Models;
using ShieldStore.Repositories.interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ShieldStore.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthenticationController : ControllerBase
	{
		private readonly UserManager<IdentityUser> _userManager;
		private readonly IUserRepository userRepository;

		private readonly Jwt _jwt;
		public AuthenticationController(UserManager<IdentityUser> userManager, IOptions<Jwt> jwt, IUserRepository userRepository)
		{
			_userManager = userManager;
			_jwt = jwt.Value;
			this.userRepository = userRepository;
		}

		[HttpPost]
		[Route("Register")]
		public async Task<IActionResult> Register([FromBody] UserRegistrationRequestDto requestDto)
		{
			if (ModelState.IsValid)
			{
				var user = new User()
				{
					UserName = requestDto.UserName,
					Email = requestDto.Email,
					Password = requestDto.Password,
					PhoneNumber = requestDto.PhoneNumber,
				};
				User AddedUser = await userRepository.SignupAsync(user);
				if(AddedUser != null)
				{
					var jwtToken = GenerateToken(AddedUser);
					return Ok(new AuthResult()
					{
						Result = true,
						Token = jwtToken
					});
				}

			}
			return BadRequest();
		}

		[HttpPost]
		[Route("Login")]
		public async Task<IActionResult> Login([FromBody] UserLoginRequestDto loginRequest)
		{
			if (ModelState.IsValid)
			{ 
				var existing_user = await _userManager.FindByEmailAsync(loginRequest.Email);
				if (existing_user == null) {

					return BadRequest(new AuthResult()
					{
						Result = false,
						Errors = new List<string>()
						{
							"Invalid payload"
						}
					});
				}

				var isCorrect = _userManager.CheckPasswordAsync(existing_user,loginRequest.Password);

				if(!await isCorrect)
				{
					return BadRequest(new AuthResult()
					{
						Result = false,
						Errors = new List<string>()
						{
							"Invalid Credentials"
						}
					});
				}
				/*var jwtToken = GenerateToken(existing_user);
				return Ok(new AuthResult()
				{
					Result= true,
					Token = jwtToken
				});*/
			}
			return BadRequest(new AuthResult()
			{
				Result = false,
				Errors = new List<string>()
				{
					"Invalid payload"
				}
			});
		}

			private string GenerateToken(User user)
			{
				var handler = new JwtSecurityTokenHandler();
				var key = Encoding.UTF8.GetBytes(_jwt.Key);
				var credentials = new SigningCredentials(
				new SymmetricSecurityKey(key),
				SecurityAlgorithms.HmacSha256Signature);

				var tokenDescriptor = new SecurityTokenDescriptor
				{
					Subject = new ClaimsIdentity(new[]
					{
						new Claim("Id",user.Id.ToString()),
						new Claim(JwtRegisteredClaimNames.Sub,user.Email),
						new Claim(JwtRegisteredClaimNames.Email,user.Email),
						new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
						new Claim(JwtRegisteredClaimNames.Iat,DateTime.Now.ToUniversalTime().ToString()),
					}),
					Expires = DateTime.UtcNow.AddMinutes(60),
					SigningCredentials = credentials,
				};

				var token = handler.CreateToken(tokenDescriptor);
				return handler.WriteToken(token);
			}

	}
}
