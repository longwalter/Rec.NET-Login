using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RecNetLogin
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

        private HttpClient Client { get; set; }

        public async Task<Result> LogInAsync(string username,
            string password,
            bool testToken = true,
            CancellationToken cancellationToken = default)
        {
            var response = await TryLogInAsync(username, password, testToken, cancellationToken);

            if (!response.Success)
                throw new Exception("Failed to login: " + response.Error);

            return response;
        }

        public async Task<Result> TryLogInAsync(string username,
            string password,
            bool testToken = true,
            CancellationToken cancellationToken = default)
        {
            // Create the request.
            Uri verifUri = new("https://auth.rec.net/Account/Login?ReturnUrl=%2Fconnect%2Fauthorize%2Fcallback%3Fclient_id%3Drecnet%26redirect_uri%3Dhttps%253A%252F%252Frec.net%252Fauthenticate%252Fdefault%26response_type%3Did_token%2520token%26scope%3Dopenid%2520rn.api%2520rn.notify%2520rn.match.read%2520rn.chat%2520rn.accounts%2520rn.auth%2520rn.link%2520rn.clubs%2520rn.rooms%26state%3Dc12d4989a6ce472c97f8428cb0778c22%26nonce%3D51ce5569285e423eb5c365940cd7b9cb");
            using HttpRequestMessage verifTokenRequest = new(HttpMethod.Get, verifUri);

            //Obtain unique verif token.
            using HttpResponseMessage verifyRequest = await Client.SendAsync(verifTokenRequest, cancellationToken);
            string verifContent = await verifyRequest.Content.ReadAsStringAsync(cancellationToken);

            // Only want the verifcation token so just do the easy method.
            string verifToken = verifContent
                .Split("<input name=\"__RequestVerificationToken\" type=\"hidden\" value=\"")[1]
                .Split("\" /><input name=\"Input.RememberMe\"")[0];

            // The request data.
            string data = $"Input.Username={username}&Input.Password={password}&Input.RememberMe=true&button=login&__RequestVerificationToken={verifToken}&Input.RememberMe=false";
            using StringContent content = new(data, Encoding.UTF8, "application/x-www-form-urlencoded");

            // Create the request.
            using HttpRequestMessage postRequest = new(HttpMethod.Post, verifUri);
            postRequest.Content = content;

            // Post the request to the server and return the url.
            using HttpResponseMessage postResponse = await Client.SendAsync(postRequest, cancellationToken);
            string postUrl = postResponse.RequestMessage.RequestUri.ToString();

            // If it didn't redirect then the credentials were invalid.
            // The condition gives it away but check if the account was locked out.
            if (postUrl == verifUri.ToString())
                return new Result()
                {
                    Auth = null,
                    Success = false,
                    Error = LogInErrors.InvalidCredentials
                };

            else if (postUrl == "https://auth.rec.net/Account/Lockout")
                return new Result()
                {
                    Auth = null,
                    Success = false,
                    Error = LogInErrors.AccountLockedOut
                };


            // Get the token from the url.
            string token = postUrl.Split("access_token=")[1].Split("&token_type=")[0];

            // Try get the users account data to test the token.
            if (testToken)
            {
                using HttpRequestMessage accountRequest = new(HttpMethod.Get, new Uri("https://accounts.rec.net/account/me"));
                accountRequest.Headers.Add("Authorization", $"Bearer {token}");

                using HttpResponseMessage response = await Client.SendAsync(accountRequest, cancellationToken);

                if (!response.IsSuccessStatusCode)
                    return new Result()
                    {
                        Auth = null,
                        Success = false,
                        Error = LogInErrors.InvalidToken
                    };
            }

            // Auth is valid so return it.
            return new Result()
            {
                Auth = token,
                Success = true,
                Error = LogInErrors.None
            };
        }
    }
}
