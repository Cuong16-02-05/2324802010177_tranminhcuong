namespace ASC.Model
{
    public class Constants
    {
        public static class Roles
        {
            public const string Admin = "Admin";
            public const string Engineer = "Engineer";
            public const string User = "User";
        }

        public static class Cache
        {
            public const string NavigationCache = "NavigationCache";
        }
    }

    public enum MasterKeys
    {
        VehicleName, VehicleType
    }

    public enum Status
    {
        New, Denied, Pending, Initiated, InProgress,
        QuoteSent, PendingCustomerApproval, RequestForInformation, Completed
    }

    public static class QuoteStatuses
    {
        public const string Pending  = "Pending";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
    }

    public static class PaymentStatuses
    {
        public const string Unpaid = "Unpaid";
        public const string Paid   = "Paid";
    }
}
