using System;
using System.Threading.Tasks;
using RecNetLogin;

namespace Example
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string username = null; // Your username.
            string password = null; // Your passowrd.

            var rnClient = new RecNETClient();
            var loginAttempt = await rnClient.TryLogInAsync(username, password, true);

            if (loginAttempt.Success)
                Console.WriteLine("Auth: " + loginAttempt.Auth);
            else
                Console.WriteLine("Failed to login: " + loginAttempt.Error);

            Console.ReadLine();
        }
    }
}
