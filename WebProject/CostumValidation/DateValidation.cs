#nullable disable
using System.ComponentModel.DataAnnotations;

namespace WebProject.CostumValidation
{
	public class DateValidation : ValidationAttribute
	{
		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			DateTime dt = (DateTime)value;

			if (dt <= DateTime.Now)
			{
				return ValidationResult.Success;
			}
			else
			{
				return new ValidationResult("Date is not in given range.");
			}
		}
	}
}
