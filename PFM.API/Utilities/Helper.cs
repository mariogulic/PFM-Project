namespace PFM.API.Utilities
{
    public static class Helper
    {
        public static bool ValidateKinds(string kinds)
        {
            var validKinds = new List<string> { "dep", "fee", "pmt", "sal", "wdw" };
            var inputKinds = kinds?.Split(',').Select(k => k.Trim().ToLower()) ?? new List<string>();

            if (inputKinds.Any(k => !validKinds.Contains(k)))
            {
                return false;
            }

            return true;
        }
    }
}
