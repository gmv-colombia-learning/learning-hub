namespace VirtualBuddy.Domain.Auth
{
    public static class PasswordRecoveryRatePolicy
    {
        public static bool CanRequest(IEnumerable<DateTime> previousRequests, DateTime now)
        {
            var requests = previousRequests.ToArray();
            if (requests.Any(requestedAt => requestedAt > now.AddMinutes(-1)))
                return false;

            return requests.Count(requestedAt => requestedAt > now.AddHours(-1)) < 5;
        }
    }
}
