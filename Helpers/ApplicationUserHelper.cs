using ProjectManagementApplication.Authentication;

namespace ProjectManagementApplication.Helpers
{
    public static class ApplicationUserHelper
    {
        public static string UserInitials(ApplicationUser user)
        {
            var first = user.FirstName?.Trim();
            var last = user.LastName?.Trim();
            string initials;
            if (!string.IsNullOrEmpty(first) && !string.IsNullOrEmpty(last))
            {
                initials = $"{first[0]}{last[0]}".ToUpper();
            }
            else
            {
                var un = user.UserName ?? string.Empty;
                initials = un.Length >= 2 ? un.Substring(0, 2).ToUpper() : un.ToUpper();
            }
            return initials;
        }
    }
}
