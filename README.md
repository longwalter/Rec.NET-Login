# Rec.NET Login
A C# script that allows you to programmatically obtain a recnet auth token.

## How to use
To use this script. Create a new instance of the `RecNETClient` class. The code already creates a HttpClient for you to use, but feel free to pass your own through!

### Logging in
Using your existing `RecNETClient`, call the method `LogInAsync()` or `TryLogInAsync()`, the difference being that one will trigger an exception and the other will not, and pass through your username and password. There is an optional `testToken` value (defaults to true) in the function that will make an extra request to check if the returned token is valid or not.