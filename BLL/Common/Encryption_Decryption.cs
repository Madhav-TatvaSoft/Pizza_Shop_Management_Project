namespace BLL.Common;

public class Encryption_Decryption
{
    public string Base64Encode(string plainText)
    {
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        return System.Convert.ToBase64String(plainTextBytes);
    }

    public string Base64Decode(string base64EncodedData)
    {
        var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
        return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
    }
}







//     public string Base64Encode(string plainText)
//     {
//         var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
//         return System.Convert.ToBase64String(plainTextBytes);
//     }

//     public string Base64Decode(string base64EncodedData)
//     {
//         var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
//         return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
//     }

// This are my 2 function for encryp0tion and decryption please help me to generate the middleware to encrypt the Id and Email which I am sending in the URL and decode it where it is needed  i will also provide the ID and Email which i am linking in the URL