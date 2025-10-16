using Oracle.ManagedDataAccess.Client;

internal class UserStore
{
    public static User TraceUser(string email)
    {
        using var conn = DbConnectionHelper.Connect();
        using var cmd = conn.CreateCommand();

        cmd.BindByName = true;
        cmd.CommandText = "SELECT * FROM users WHERE email = :email";

        cmd.Parameters.Add(":email", OracleDbType.Varchar2, 50).Value = email;

        var reader = cmd.ExecuteReader();

        if (reader.Read())
        {
            return new User
            {
                UserId = reader.GetInt32(reader.GetOrdinal("user_id")),
                FirstName = reader.GetString(reader.GetOrdinal("first_name")),
                LastName = reader.GetString(reader.GetOrdinal("last_name")),
                Email = reader.GetString(reader.GetOrdinal("email")),
                PwHash = reader.GetString(reader.GetOrdinal("pw_hash")),
                Salt = reader.GetString(reader.GetOrdinal("salt")),
                ImageUrl = reader.IsDBNull(reader.GetOrdinal("image_url")) ? null : reader.GetString(reader.GetOrdinal("image_url"))
            };
        }
        else
        {
            Console.WriteLine("Es konnte kein Nutzer gefunden werden.");
        }

        return null;
    }
    

    public static bool EmailExists(string email) {
        using var conn = DbConnectionHelper.Connect();
        using var cmd = conn.CreateCommand();

        cmd.BindByName = true;
        cmd.CommandText = "SELECT 1 FROM users WHERE email = :email FETCH FIRST 1 ROWS ONLY";
        cmd.Parameters.Add(":email", OracleDbType.Varchar2, 50).Value = email;

        using var reader = cmd.ExecuteReader();
        return reader.Read();
    }

    public static int CreateUser(User user) {
        using var conn = DbConnectionHelper.Connect();
        using var cmd = conn.CreateCommand();

        cmd.BindByName = true;

        cmd.CommandText =
            @"INSERT INTO users (first_name, last_name, email, pw_hash, image_url, salt)
              VALUES (:first_name, :last_name, :email, :pw_hash, :image_url, :salt)
              RETURNING user_id INTO :new_id";

        cmd.Parameters.Add(":first_name", OracleDbType.Varchar2, 50).Value = user.FirstName;
        cmd.Parameters.Add(":last_name" , OracleDbType.Varchar2, 50).Value = user.LastName;
        cmd.Parameters.Add(":email"     , OracleDbType.Varchar2, 50).Value = user.Email;
        cmd.Parameters.Add(":pw_hash"   , OracleDbType.Varchar2, 200).Value = user.PwHash;

        var imgParam = new OracleParameter(":image_url", OracleDbType.Varchar2, 255);
        if (string.IsNullOrWhiteSpace(user.ImageUrl))
            imgParam.Value = DBNull.Value;     // IMAGE_URL ist NULL-able
        else
            imgParam.Value = user.ImageUrl;
        cmd.Parameters.Add(imgParam);

        cmd.Parameters.Add(":salt", OracleDbType.Varchar2, 50).Value = user.Salt; // NOT NULL

        var outParam = new OracleParameter(":new_id", OracleDbType.Int32)
        {
            Direction = System.Data.ParameterDirection.Output
        };
        cmd.Parameters.Add(outParam);

        try
        {
            cmd.ExecuteNonQuery();
            return Convert.ToInt32(outParam.Value.ToString());
        }
        catch (OracleException ox) when (ox.Number == 1) 
        {
            //throw new InvalidOperationException("E-Mail bereits vergeben.", ox);
            return -1;
        }
    }
}