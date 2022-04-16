namespace RecNetLogin
{
    public class Result
    {
        public string Auth { get; set; }
        public bool Success { get; set; }
        public LogInErrors Error { get; set; }
    }
}
