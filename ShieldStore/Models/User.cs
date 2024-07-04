using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace ShieldStore.Models
{
	public class User
	{
		public Guid Id { get; set; }
		[Required(ErrorMessage = "Name is required")]
		[MaxLength(30)]
		public string UserName { get; set; } = string.Empty;
		[Required]
		public string Email { get; set; } = string.Empty;
		[Required]
		public string Password { get; set; } = string.Empty;
		[Required]
		public string PhoneNumber { get; set; } = string.Empty;
		[ValidateNever]
		public string ImageUrl { get; set; } = string.Empty;
		public string Bio { get; set; } = string.Empty;
		public long CreditCardNum { get; set; }
		[ValidateNever]
		public int Money { get; set; }
	}
}
