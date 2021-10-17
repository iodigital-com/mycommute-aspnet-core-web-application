namespace MyCommute.Domain.Exceptions
{
    public class ForwardGeoCodeFailedException : Exception
    {
        public ForwardGeoCodeFailedException(string address) : base($"Forward geo code failed for {address}")
        {
        }
    }
}