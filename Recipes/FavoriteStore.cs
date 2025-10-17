using Oracle.ManagedDataAccess.Client;

internal class FavoriteStore
{
    public static bool IsFavorite(int userId, int recipeId)
    {
        using var conn = DbConnectionHelper.Connect();
        using var cmd = conn.CreateCommand();

        cmd.BindByName = true;
        cmd.CommandText = "SELECT COUNT(*) FROM FAVORITE_RECIPES WHERE user_id = :userId AND recipe_id = :recipeId";
        cmd.Parameters.Add(":userId", OracleDbType.Int32).Value = userId;
        cmd.Parameters.Add(":recipeId", OracleDbType.Int32).Value = recipeId;

        var count = Convert.ToInt32(cmd.ExecuteScalar());
        return count > 0;
    }

    public static void AddFavorite(int userId, int recipeId)
    {
        using var conn = DbConnectionHelper.Connect();
        using var cmd = conn.CreateCommand();

        cmd.BindByName = true;
        cmd.CommandText = @"
            INSERT INTO FAVORITE_RECIPES (user_id, recipe_id)
            VALUES (:userId, :recipeId)";
        
        cmd.Parameters.Add(":userId", OracleDbType.Int32).Value = userId;
        cmd.Parameters.Add(":recipeId", OracleDbType.Int32).Value = recipeId;

        try
        {
            cmd.ExecuteNonQuery();
        }
        catch (OracleException ex)
        {
            // Ignoriere Duplicate-Key Fehler (bereits Favorit)
            if (ex.Number != 1) throw;
        }
    }

    public static void RemoveFavorite(int userId, int recipeId)
    {
        using var conn = DbConnectionHelper.Connect();
        using var cmd = conn.CreateCommand();

        cmd.BindByName = true;
        cmd.CommandText = "DELETE FROM FAVORITE_RECIPES WHERE user_id = :userId AND recipe_id = :recipeId";
        cmd.Parameters.Add(":userId", OracleDbType.Int32).Value = userId;
        cmd.Parameters.Add(":recipeId", OracleDbType.Int32).Value = recipeId;

        cmd.ExecuteNonQuery();
    }
}