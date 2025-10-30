namespace BackendAccountService.Data.DbConstants;

public static class ServiceRole
{
    public static class Packaging
    {
        public static class ApprovedPerson
        {
            public const int Id = 1;
            public const string Key = "Packaging.ApprovedPerson";
        }

        public static class DelegatedPerson
        {
            public const int Id = 2;
            public const string Key = "Packaging.DelegatedPerson";
        }

        public static class BasicUser
        {
            public const int Id = 3;
            public const string Key = "Packaging.BasicUser";
        }
    }

	public static class Regulator
	{
		public static class Admin
		{
			public const int Id = 4;
			public const string Key = "Regulator.Admin";
		}

		public static class Basic
		{
			public const int Id = 5;
			public const string Key = "Regulator.Basic";
		}
	}

	public static class LaPayment
	{
		public static class Admin
		{
			public const int Id = 6;
			public const string Key = "LaPayment.UserAdministrator";
		}

		public static class BasicUser
		{
			public const int Id = 7;
			public const string Key = "LaPayment.BasicUser";
		}
    }

    public static class ReprocessorExporter
    {
        public static class ApprovedPerson
        {
            public const int Id = 8;
            public const string Key = "Re-Ex.ApprovedPerson";
        }

        public static class DelegatedPerson
        {
            public const int Id = 9;
            public const string Key = "Re-Ex.DelegatedPerson";
        }

        public static class BasicUser
        {
            public const int Id = 10;
            public const string Key = "Re-Ex.BasicUser";
        }

        public static class AdminUser
        {
            public const int Id = 11;
            public const string Key = "Re-Ex.AdminUser";
        }

        public static class StandardUser
        {
            public const int Id = 12;
            public const string Key = "Re-Ex.StandardUser";
        }
    }
}