using System.ComponentModel.DataAnnotations;

namespace DAL.ViewModels;

public class ChangePasswordViewModel
{
    public string CurrentPassword { get; set; }

    [MinLength(6,ErrorMessage ="Password must contains at least 6 characters")]
    [MaxLength(20,ErrorMessage ="Password should not exceed 20 characters")]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$",ErrorMessage ="Password must contain at least one uppercase letter, one number,and one special character.")]
    public string NewPassword { get; set; }
    public string NewConfirmPassword { get; set; }
}


