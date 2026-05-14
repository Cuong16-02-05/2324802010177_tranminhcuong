namespace ASC.Web.Configuration
{
    public class SmtpConfig
    {
        public string? Host { get; set; }
        public int Port { get; set; }
        public string? From { get; set; }
        public string? Password { get; set; }
    }

    public class ApplicationSettings
    {
        public string? ApplicationTitle { get; set; }
        public string? Title { get; set; }

        public string? AdminEmail { get; set; }
        public string? AdminName { get; set; }
        public string? AdminPassword { get; set; }

        public string? EngineerEmail { get; set; }
        public string? EngineerName { get; set; }
        public string? EngineerPassword { get; set; }

        public string? UserEmail { get; set; }
        public string? UserName { get; set; }
        public string? UserPassword { get; set; }

        public string? Roles { get; set; }

        public string? SMTPServer { get; set; }
        public int SMTPPort { get; set; }
        public string? SMTPAccount { get; set; }
        public string? SMTPPassword { get; set; }

        public SmtpConfig? Smtp { get; set; }
    }
}
