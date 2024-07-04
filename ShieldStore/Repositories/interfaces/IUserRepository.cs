using ShieldStore.Models;

namespace ShieldStore.Repositories.interfaces
{
	public interface IUserRepository
	{
		public Task<User> SignupAsync(User user);

	}
}
