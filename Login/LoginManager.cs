internal class LogingManager
{
    public static bool Login(string enteredEmail, string enteredPw)
    {
        User queriedDbUser = UserStore.TraceUser(enteredEmail);

        if (queriedDbUser != null)
        {
            Console.WriteLine("Nutzer konnte gefunden werden!");

            string enteredPwHash = PasswordHasher.ComputeHashWithSalt(enteredPw, queriedDbUser.Salt);

            return queriedDbUser.PwHash == enteredPwHash;
        }
        else
        {
            Console.WriteLine("Nutzer konnte nicht gefunden werden!");
            return false;  
        }
    }
}