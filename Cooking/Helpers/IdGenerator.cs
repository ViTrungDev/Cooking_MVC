namespace Cooking.Helpers
{
    public class IdGenerator
    {
        public static string GenerateId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
