using Microsoft.EntityFrameworkCore;
using ShieldStore.Data;
using ShieldStore.Models;
using ShieldStore.Repositories.interfaces;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ShieldStore.Repositories.implementations
{
	public class UserRepository : IUserRepository
	{
		private readonly ApplicationDbContext dbContext;
		private readonly ILogger<UserRepository> _logger;

		public UserRepository(ApplicationDbContext dbContext , ILogger<UserRepository> logger)
		{
			this.dbContext = dbContext;
			_logger = logger;
		}

		public async Task<User> SignupAsync(User user)
		{
			// Validate user input
			if (user == null || string.IsNullOrEmpty(user.Password) || string.IsNullOrEmpty(user.Email))
			{
				_logger.LogWarning("User signup failed: invalid user or password.");
				throw new ArgumentException("Invalid user or password");
			}

			// Validate email format
			if (!IsValidEmail(user.Email))
			{
				_logger.LogWarning("User signup failed: invalid email format.");
				throw new ArgumentException("Invalid email format");
			}

			// Check for existing user
			if (await dbContext.Users.AnyAsync(u => u.Email == user.Email))
			{
				_logger.LogWarning("User signup failed: user already exists.");
				throw new InvalidOperationException("User already exists");
			}

			// Validate password strength
			if (!IsValidPassword(user.Password))
			{
				_logger.LogWarning("User signup failed: password does not meet strength requirements.");
				throw new ArgumentException("Password does not meet strength requirements");
			}

			// Hash the password
			user.Password = HashPassword(user.Password);

			// Assign a new GUID
			user.Id = Guid.NewGuid();

			// Save the user to the database
			dbContext.Users.Add(user);
			await dbContext.SaveChangesAsync();

			_logger.LogInformation("User signup succeeded: {userId}", user.Id);
			return user;
		}

		private bool IsValidEmail(string email)
		{
			var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
			return emailRegex.IsMatch(email);
		}

		private bool IsValidPassword(string password)
		{
			// Password strength criteria: at least 8 characters, at least one uppercase letter, one lowercase letter, one digit, and one special character
			var passwordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$");
			return passwordRegex.IsMatch(password);
		}

		private string HashPassword(string password)
		{
			using (var sha256 = SHA256.Create())
			{
				var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
				return BitConverter.ToString(bytes).Replace("-", "").ToLower();
			}
		}
	}
}
