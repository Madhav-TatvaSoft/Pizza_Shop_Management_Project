namespace BLL.common;

public class EmailTemplate
{
    public static string ResetPasswordEmail(string resetLink)
    {
        string body = $@"
                <div style='max-width: 500px; font-family: Arial, sans-serif; border: 1px solid #ddd;'>
                <div style='background: #006CAC; padding: 10px; text-align: center; height:90px; max-width:100%; display: flex; justify-content: center; align-items: center;'>
                    <img src='https://images.vexels.com/media/users/3/128437/isolated/preview/2dd809b7c15968cb7cc577b2cb49c84f-pizza-food-restaurant-logo.png' style='max-width: 50px;' />
                    <span style='color: #fff; font-size: 24px; margin-left: 10px; font-weight: 600;'>PIZZASHOP</span>
                </div>
                <div style='padding: 20px 5px; background-color: #e8e8e8;'>
                    <p>Pizza shop,</p>
                    <p>Please click <a href='{0}' style='color: #1a73e8; text-decoration: none; font-weight: bold;'>here</a>
                        to reset your account password.</p>
                    <p>If you encounter any issues or have any questions, please do not hesitate to contact our support team.</p>
                    <p><strong style='color: orange;'>Important Note:</strong> For security reasons, the link will expire in 24 hours.
                        If you did not request a password reset, please ignore this email or contact our support team immediately.
                    </p>
                </div>
                </div>";
        body.Replace("{0}", resetLink);
        return body;
    }

    // public static string AddUserEmail(string password,string Email){

    //     return body;
    // }

}
