namespace BLL.common;

public class NotificationMessage
{
    public const string InvalidCredentials = "Invalid credentials. Please try again.";
    public const string LoginSuccess = "Logged In Successfully.";
    public const string LogoutSuccess = "Logged Out successfully .";
    
    public const string EntityUpdated = "{0} updated successfully!";
    public const string EntityCreated = "{0} added successfully!";
    public const string EntityDeleted = "{0} deleted successfully!";
    public const string ProfileUpdated = "Your profile updated successfully!";
    public const string EmailSentSuccessfully = "Email has been sent successfully!";
    public const string PasswordChanged = "Your password has been changed successfully.";

    // Error Messages
    public const string EntityUpdatedFailed = "Failed to Update {0}";
    public const string EntityCreatedFailed = "Failed to Add {0}";
    public const string EntityDeletedFailed = "Failed to Delete {0}";
    public const string EmailSendingFailed = "Failed to send the email. Please try again.";
    public const string DoesNotExists = "{0} Does Not Exists!";
    public const string PasswordCheck = "Password and Confirm Password should be same";
    public const string ResetPasswordChangedError =  "You have already changed the Password once.";
    public const string PasswordChangeFailed = "Failed to change the password. Please try again.";
    public const string ImageFormat = "The Image format is not supported.";
    public const string AlreadyExists = "{0} Already Exists!";
    // public const string InvalidModelState = "Model State Is Invalid!";
}