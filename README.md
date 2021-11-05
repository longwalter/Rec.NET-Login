# Rec.NET-Login
A C# script that allows you to log in to rec.net outside the website.

## How to use

To use this script. Create a new instance of the `RecNETClient` class. The code already creates a HttpClient for you to use, but feel free to pass your own through!

Without your own client...
```cs
var rnClient = new RecNETClient();
```

...or with your own client!
```cs
HttpClient httpClient = new()

var rnClient = new RecNETClient(httpClient)
```

### Logging in

Using your exisitng `RecNETClient`, call the asyncronous method `LogInToRecNetAsync()` and pass through your username and password. There is also an optional `testToken` option (defaults to true) in the function that will make an extra HTTP Request to check if the returned token is valid or not.

Example log in:
```cs
Console.WriteLine("Please enter your username and password below.");

Console.Write("Username: ");
string username = Console.ReadLine();

Console.Write("Password: ");
string password = Console.ReadLine(); 

var rnClient = new RecNETClient();
var loginAttempt = await RecNETClient.LogInToRecNetAsync(username, password, true);

if (!loginAttempt.Success) 
    Console.WriteLine($"Error: {loginAttempt.Error}");
    
else
    Console.WriteLine($"Successfully logged in!\n\nYour auth token is: {loginAttempt.Auth}");

Console.ReadLine();
```

## Disclaimer 
Please make a pull request if there are any obvious errors or improvements that can be made. Thanks!
