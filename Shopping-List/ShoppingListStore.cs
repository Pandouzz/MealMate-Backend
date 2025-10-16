using Oracle.ManagedDataAccess.Client;

internal class ShoppingListStore
{
    public static List<ShoppingList> GetLists(int userId)
    {
        var lists = new List<ShoppingList>();

        using var conn = DbConnectionHelper.Connect();
        using var cmd = conn.CreateCommand();

        cmd.BindByName = true;
        cmd.CommandText = "SELECT list_id, list_name FROM fs231_aretzer.grocery_lists WHERE user_id = :userId";
        cmd.Parameters.Add(":userId", OracleDbType.Int32).Value = userId;

        var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            lists.Add(new ShoppingList
            {
                ListId = reader.GetInt32(reader.GetOrdinal("list_id")),
                ListName = reader.GetString(reader.GetOrdinal("list_name")),
                Items = new List<ShoppingItem>()
            });
        }

        return lists;
    }

    public static bool InsertList(ShoppingList list)
    {
        using var conn = DbConnectionHelper.Connect();
        using var cmd = conn.CreateCommand();

        cmd.BindByName = true;
        cmd.CommandText = @"INSERT INTO fs231_aretzer.grocery_lists (user_id, list_name)
                            VALUES (:userId, :listName)";
        //cmd.Parameters.Add(":listId", OracleDbType.Int32).Value = list.ListId;
        cmd.Parameters.Add(":userId", OracleDbType.Int32).Value = list.UserId;
        cmd.Parameters.Add(":listName", OracleDbType.Varchar2, 50).Value = list.ListName;



        return cmd.ExecuteNonQuery() > 0;
    }
}
