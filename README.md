# Rec.NET Login
A C# script that allows you to programmatically obtain an auth token from https://rec.net/

## How to use
Create a new instance of the `RecNETClient` class. The client will create a `HttpClient` for you if you don't pass one through, however, I recommend using your own throughout your whole project instead of making and disposing multiple clients.

### Logging in
Using your existing `RecNETClient`, call the method `LoginAsync()` and provide your username, password and, if enabled, your Two Factor Authentication code. If the login was unsuccessful, the client will throw a `RecNetException` with a message based on the error it encountered.