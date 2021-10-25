using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

public static class RecNetLogin
{
    public static HttpClient client = new();

    public static async Task<LogInReturnValue> LogInToRecNetAsync(string username, string password)
    {       
        Uri uri = new("https://auth.rec.net/Account/Login?ReturnUrl=%2Fconnect%2Fauthorize%2Fcallback%3Fclient_id%3Drecnet%26redirect_uri%3Dhttps%253A%252F%252Frec.net%252Fauthenticate%252Fdefault%26response_type%3Did_token%2520token%26scope%3Dopenid%2520rn.api%2520rn.notify%2520rn.match.read%2520rn.chat%2520rn.accounts%2520rn.auth%2520rn.link%2520rn.clubs%2520rn.rooms%26state%3Dc12d4989a6ce472c97f8428cb0778c22%26nonce%3D51ce5569285e423eb5c365940cd7b9cb");

        HttpResponseMessage verifyRequest = await client.GetAsync(uri.ToString()); //Obtain unique verif token.
        string verifContent = await verifyRequest.Content.ReadAsStringAsync();

        string verifToken = verifContent
            .Split("<input name=\"__RequestVerificationToken\" type=\"hidden\" value=\"")[1]
            .Split("\" /><input name=\"Input.RememberMe\"")[0];

        //The request data
        string data = $"Input.Username={username}&Input.Password={password}&Input.RememberMe=true&button=login&__RequestVerificationToken={verifToken}&Input.RememberMe=false";
        StringContent content = new StringContent(data, Encoding.UTF8, "application/x-www-form-urlencoded");

        //Post the request to the server and return the url
        var postRequest = await client.PostAsync(uri, content);
        var postUrl = postRequest.RequestMessage.RequestUri.ToString();

        if (postUrl == uri.ToString())
            {
                return new LogInReturnValue()
                {
                    Auth = null,
                    Success = false,
                    Error = "Invalid Credentials."
                };
            } // Credentials are invalid
        if (postUrl == "https://auth.rec.net/Account/Lockout")
            {
                return new LogInReturnValue()
                {
                    Auth = null,
                    Success = false,
                    Error = "Account Locked Out."
                };
            } //Account is locked out

        return new LogInReturnValue()
            {
                Auth = postUrl.Split("access_token=")[1].Split("&token_type=")[0],
                Success = true,
                Error = null
            }; // Return correct values if it worked.
    }      
}
public class LogInReturnValue
    {
        public bool Success { get; set; }
        public string Auth { get; set; }
        public string Error { get; set; }
    }

