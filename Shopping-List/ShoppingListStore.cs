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
        cmd.CommandText = @"INSERT INTO fs231_aretzer.grocery_lists (list_id, user_id, list_name)
                            VALUES (seq_grocery_lists.NEXTVAL, :userId, :listName)";
        cmd.Parameters.Add(":userId", OracleDbType.Int32).Value = list.UserId;
        cmd.Parameters.Add(":itemId", OracleDbType.Int32).Value = list.Items.FirstOrDefault()?.ItemId ?? 0;
        cmd.Parameters.Add(":listName", OracleDbType.Varchar2, 50).Value = list.ListName;


        return cmd.ExecuteNonQuery() > 0;
    }
}
