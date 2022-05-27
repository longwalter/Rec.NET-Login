using System;
using System.Threading.Tasks;
using RecNetLogin;

namespace Example
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string username = "Coach"; // Your username.
            string password = "SuperSecretPassword1987"; // Your passowrd.
            string twoFactorCode = "000000"; // Your 2FA code if enabled.

            var rnClient = new RecNETClient();
            
            try
            {
                string auth = await rnClient.LoginAsync(username, password, twoFactorCode);
                Console.WriteLine("Auth: " + auth);
            }
            catch(Exception e)
            {
                Console.WriteLine("Failed to login! Error: " + e.Message);
            }

            Console.ReadLine();
        }
    }
}
