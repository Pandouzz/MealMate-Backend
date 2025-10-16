internal static class RegistrationManager 
{
    public static RegisterResult Register(RegisterRequest req)
    {
        try 
        {
            if(string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.PwHash)) 
                return RegisterResult.Fail("E-Mail und Passwort-Hash sind Pflicht.");

            if (UserStore.EmailExists(req.Email))
                return RegisterResult.Fail("E-Mail existiert bereits.", RegistrationError.EmailAlreadyExists);

            var salt = string.IsNullOrWhiteSpace(req.Salt)
                ? Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                : req.Salt!;
                
            var user = new User
            {
                FirstName = req.FirstName,
                LastName = req.LastName,
                Email = req.Email,
                PwHash = req.PwHash,
                ImageUrl = req.ImageUrl,
                Salt = salt
            };
            
            int newId = UserStore.CreateUser(user);

            if (newId == -1)
                return RegisterResult.Fail("E-Mail existiert bereits.", RegistrationError.EmailAlreadyExists);

            return RegisterResult.Ok(newId);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Registrierung fehlgeschlagen: " + ex.Message);
            return RegisterResult.Fail("Unerwarteter Fehler.");
        }
    }
}

internal enum RegistrationError { None = 0, EmailAlreadyExists = 1 }

internal readonly struct RegisterResult 
{
    public bool Success { get; }
    public int UserId { get; }
    public string ErrorMessage { get; }
    public RegistrationError ErrorCode { get; }

    private RegisterResult(bool success, int userId, string error, RegistrationError code) 
    {
        Success = success; UserId = userId; ErrorMessage = error; ErrorCode = code;
    }

    public static RegisterResult Ok (int id) => new RegisterResult(true, id, "", RegistrationError.None);
    public static RegisterResult Fail (string msg, RegistrationError code = RegistrationError.None)
        => new RegisterResult(false, 0, msg, code);     
}