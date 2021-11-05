using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RecNetCredentialLogin
{
    public class RecNETClient
    {    
        public RecNETClient(HttpClient client)
        {
            Client = client;
        }
        public RecNETClient() : this(Http.Client) 
        {
        }

        private static HttpClient Client { get; set; }

        public static async Task<LogInReturnValue> LogInToRecNetAsync(string username, string password, bool testToken = true)
        {
            // Create the request.
            Uri verifUri = new("https://auth.rec.net/Account/Login?ReturnUrl=%2Fconnect%2Fauthorize%2Fcallback%3Fclient_id%3Drecnet%26redirect_uri%3Dhttps%253A%252F%252Frec.net%252Fauthenticate%252Fdefault%26response_type%3Did_token%2520token%26scope%3Dopenid%2520rn.api%2520rn.notify%2520rn.match.read%2520rn.chat%2520rn.accounts%2520rn.auth%2520rn.link%2520rn.clubs%2520rn.rooms%26state%3Dc12d4989a6ce472c97f8428cb0778c22%26nonce%3D51ce5569285e423eb5c365940cd7b9cb");
            HttpRequestMessage verifTokenRequest = new(HttpMethod.Get, verifUri);

            //Obtain unique verif token.
            HttpResponseMessage verifyRequest = await Client.SendAsync(verifTokenRequest); 
            string verifContent = await verifyRequest.Content.ReadAsStringAsync();

            // Only want the verifcation token so just do the easy method.
            string verifToken = verifContent
                .Split("<input name=\"__RequestVerificationToken\" type=\"hidden\" value=\"")[1]
                .Split("\" /><input name=\"Input.RememberMe\"")[0];

            // The request data.
            string data = $"Input.Username={username}&Input.Password={password}&Input.RememberMe=true&button=login&__RequestVerificationToken={verifToken}&Input.RememberMe=false";
            StringContent content = new(data, Encoding.UTF8, "application/x-www-form-urlencoded");

            // Create the request.
            HttpRequestMessage postRequest = new(HttpMethod.Post, verifUri);
            postRequest.Content = content;

            // Post the request to the server and return the url.
            var postResponse = await Client.SendAsync(postRequest);
            var postUrl = postResponse.RequestMessage.RequestUri.ToString();

            // If it didn't redirect then the credentials were invalid.
            if (postUrl == verifUri.ToString())
                return new LogInReturnValue()
                {
                    Auth = null,
                    Success = false,
                    Error = "Invalid Credentials."
                };

            // The condition gives it away but check if the account was locked out.
            if (postUrl == "https://auth.rec.net/Account/Lockout")
                return new LogInReturnValue()
                {
                    Auth = null,
                    Success = false,
                    Error = "Account Locked Out."
                };

            // Get the token from the url.
            var token = postUrl.Split("access_token=")[1].Split("&token_type=")[0];

            // Try get the users account data to test the token.
            if (testToken)
            {
                HttpRequestMessage accountRequest = new(HttpMethod.Get, new Uri("https://accounts.rec.net/account/me"));
                accountRequest.Headers.Add("Authorization", $"Bearer {token}");
                      
                var response = await Client.SendAsync(accountRequest);

                if (!response.IsSuccessStatusCode)
                    return new LogInReturnValue()
                    {
                        Auth = null,
                        Success = false,
                        Error = "Auth token did not pass testing."
                    };
                else
                    return new LogInReturnValue()
                    {
                        Auth = token,
                        Success = true,
                        Error = null
                    };
            }

            // Auth is valid so return it.
            return new LogInReturnValue()
            {
                Auth = token,
                Success = true,
                Error = null
            };
        }

        public class LogInReturnValue
        {
            public bool Success { get; set; }
            public string Auth { get; set; }
            public string Error { get; set; }
        }

        private static class Http
        {
            private static readonly Lazy<HttpClient> HttpClientLazy = new(() =>
            {
                return new HttpClient();
            });

            public static HttpClient Client => HttpClientLazy.Value;
        }
    }
}
