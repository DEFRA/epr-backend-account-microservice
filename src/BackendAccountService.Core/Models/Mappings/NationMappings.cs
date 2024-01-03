namespace BackendAccountService.Core.Models.Mappings
{
    public static class NationMappings
    {
        public static int GetNationId(Nation nation)
        {
            return nation switch
            {
                Nation.NotSet => Data.DbConstants.Nation.NotSet,
                Nation.England => Data.DbConstants.Nation.England,
                Nation.NorthernIreland => Data.DbConstants.Nation.NorthernIreland,
                Nation.Scotland => Data.DbConstants.Nation.Scotland,
                Nation.Wales => Data.DbConstants.Nation.Wales,
                _ => throw new ArgumentException($"Unrecognised nation: '{nation}'")
            };
        }

        public static Nation GetNation(int nationId)
        {
            return nationId switch
            {
                Data.DbConstants.Nation.NotSet => Nation.NotSet,
                Data.DbConstants.Nation.England => Nation.England,
                Data.DbConstants.Nation.NorthernIreland => Nation.NorthernIreland,
                Data.DbConstants.Nation.Scotland => Nation.Scotland,
                Data.DbConstants.Nation.Wales => Nation.Wales,
                _ => throw new ArgumentException($"Unrecognised nation ID: '{nationId}'")
            };
        }
        
        public static Nation? GetNation(int? nationId)
        {
            if (!nationId.HasValue)
            {
                return null;
            }

            return GetNation(nationId.Value);
        }
    }
}
