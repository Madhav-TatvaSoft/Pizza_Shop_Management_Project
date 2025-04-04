namespace BLL.common;

public class NotificationMessage
{
    public const string InvalidCredentials = "Invalid credentials. Please try again.";
    public const string LoginSuccess = "You have successfully logged In.";
    public const string LogoutSuccess = "You have successfully logged Out.";
    
    public const string EntityUpdated = "{0} has been updated successfully!";
    public const string EntityCreated = "{0} has been added successfully!";
    public const string EntityDeleted = "{0} has been deleted successfully!";
    public const string ProfileUpdated = "Your profile has been updated successfully!";
    public const string EmailSentSuccessfully = "Email has been sent successfully!";
    public const string PasswordChanged = "Your password has been changed successfully.";

    // Error Messages
    public const string EntityUpdatedFailed = "Failed Updating {0}";
    public const string EntityCreatedFailed = "Failed Adding {0}";
    public const string EntityDeletedFailed = "Failed Deleting {0}";
    public const string EmailSendingFailed = "Failed to send the email. Please try again.";
    public const string EmailDoNotExists = "Email Does Not Exists!";
    public const string AccountEmailAlreadyExists = "Account with this email already exists!";
    public const string UserNameAlreadyExists = "UserName Already Exists!";   
    public const string PasswordCheck = "Password and Confirm Password should be same";
    public const string ResetPasswordChangedError =  "You have already changed the Password once.";
    public const string PasswordChangeFailed = "Failed to change the password. Please try again.";
    public const string ImageFormat = "The Image format is not supported.";
    // public const string InvalidModelState = "Model State Is Invalid!";
}
