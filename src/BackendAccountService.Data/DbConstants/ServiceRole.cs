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
}