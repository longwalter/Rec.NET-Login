using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RecNetLogin
{
    public class RecNETClient
    {
        /// <summary>
        /// Initializes an instance of the RecNetClient using the HttpClient passed through.
        /// </summary>
        /// <param name="client"></param>
        public RecNETClient(HttpClient client) => Client = client;     

        /// <summary>
        /// Initializes an instance of the RecNetClient creating the needed HttpClient.
        /// </summary>
        public RecNETClient() : this(Http.Client) { }

        private HttpClient Client { get; set; }

        /// <summary>
        /// Uses the provided credentials to log in to https://rec.net/
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="twoFactorCode"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A Bearer Token with the recnet client type</returns>
        /// <exception cref="RecNetException"></exception>
        public async Task<string> LoginAsync(
            string username,
            string password,
            string twoFactorCode = null,
            CancellationToken cancellationToken = default)
        {        
            Uri verifUri = new("https://auth.rec.net/Account/Login?ReturnUrl=%2Fconnect%2Fauthorize%2Fcallback%3Fclient_id%3Drecnet%26redirect_uri%3Dhttps%253A%252F%252Frec.net%252Fauthenticate%252Fdefault%26response_type%3Did_token%2520token%26scope%3Dopenid%2520rn.api%2520rn.notify%2520rn.match.read%2520rn.chat%2520rn.accounts%2520rn.auth%2520rn.link%2520rn.clubs%2520rn.rooms%26state%3Dc12d4989a6ce472c97f8428cb0778c22%26nonce%3D51ce5569285e423eb5c365940cd7b9cb");
            
            HttpRequestMessage verifTokenRequest = new(HttpMethod.Get, verifUri);
            HttpResponseMessage verifResponse = await Client.SendAsync(verifTokenRequest, cancellationToken);
            string verifContent = await verifResponse.Content.ReadAsStringAsync(cancellationToken);

            string verifToken = verifContent
                .Split("<input name=\"__RequestVerificationToken\" type=\"hidden\" value=\"")[1]
                .Split("\" /><input name=\"Input.RememberMe\"")[0];

            string data = $"Input.Username={username}&Input.Password={password}&Input.RememberMe=true&button=login&__RequestVerificationToken={verifToken}&Input.RememberMe=false";
            using StringContent content = new(data, Encoding.UTF8, "application/x-www-form-urlencoded");

            HttpRequestMessage postRequest = new(HttpMethod.Post, verifUri)
            {
                Content = content
            };
            HttpResponseMessage postResponse = await Client.SendAsync(postRequest, cancellationToken);
            string postUrl = postResponse.RequestMessage.RequestUri.ToString();

            string token;

            if (postUrl.Contains("LoginWith2fa"))
            {
                string TFApageContent = await postResponse.Content.ReadAsStringAsync(cancellationToken);

                string TFAverifCode = TFApageContent
                    .Split("<input name=\"__RequestVerificationToken\" type=\"hidden\" value=\"")[1]
                    .Split("\" /><input name=\"Input.RememberMachine\"")[0];

                string TFArequestData = $"Input.TwoFactorCode={twoFactorCode}&Input.RememberMachine=false&__RequestVerificationToken={TFAverifCode}";
                using StringContent TFAcontent = new(TFArequestData, Encoding.UTF8, "application/x-www-form-urlencoded");

                HttpRequestMessage TFArequest = new(HttpMethod.Post, postUrl)
                {
                    Content = TFAcontent
                };

                HttpResponseMessage TFAresponse = await Client.SendAsync(TFArequest, cancellationToken);

                string TFAurl = TFAresponse.RequestMessage.RequestUri.ToString();

                try
                {
                    token = TFAurl.Split("access_token=")[1].Split("&token_type=")[0];
                }
                catch
                {
                    throw new RecNetException("2FA Failed!");
                }
            }
            else if (postUrl == verifUri.ToString())
                throw new RecNetException("Invalid credentials!");
            else if (postUrl == "https://auth.rec.net/Account/Lockout")
                throw new RecNetException("Account has been locked out!");
            else
                token = postUrl.Split("access_token=")[1].Split("&token_type=")[0];

            return token;
        }  
    }
}
