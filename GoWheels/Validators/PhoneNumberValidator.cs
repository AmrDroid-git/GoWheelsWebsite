using Microsoft.AspNetCore.Identity;
using GoWheels.Models;
using System.Text.RegularExpressions;

namespace GoWheels.Validators
{
    public class PhoneNumberValidator : IUserValidator<ApplicationUser>
    {
        public Task<IdentityResult> ValidateAsync(
            UserManager<ApplicationUser> manager,
            ApplicationUser user)
        {
            if (string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                return Task.FromResult(
                    IdentityResult.Failed(new IdentityError
                    {
                        Code = "PhoneRequired",
                        Description = "Phone number is required"
                    })
                );
            }

            // 8 à 15 chiffres, uniquement des digits
            if (!Regex.IsMatch(user.PhoneNumber, @"^\d{8,15}$"))
            {
                return Task.FromResult(
                    IdentityResult.Failed(new IdentityError
                    {
                        Code = "PhoneInvalid",
                        Description = "Phone number must contain 8 to 15 digits"
                    })
                );
            }

            return Task.FromResult(IdentityResult.Success);
        }
    }
}