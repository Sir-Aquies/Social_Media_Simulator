using Microsoft.AspNetCore.Identity;
using WebProject.Models;

namespace WebProject.Policies
{
	public class UserNamePolicies : UserValidator<UserModel>
	{
		public override async Task<IdentityResult> ValidateAsync(UserManager<UserModel> userManager, UserModel userModel)
		{
			//IdentityResult result = await base.ValidateAsync(userManager, userModel);
			List<IdentityError> errors = new List<IdentityError>();

			if (userModel.UserName.Length <= 5)
			{
				errors.Add(new IdentityError
				{
					Description = "Username should be longer than 5 characters"
				});
			}

			return errors.Count == 0 ? IdentityResult.Success : IdentityResult.Failed(errors.ToArray());
		}
	}
}
