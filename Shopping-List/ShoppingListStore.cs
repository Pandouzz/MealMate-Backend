// using Oracle.ManagedDataAccess.Client;

// internal class ShoppingListStore
// {
//     public static List<ShoppingList> GetLists(int userId)
//     {
//         var lists = new List<ShoppingList>();

//         using var conn = DbConnectionHelper.Connect();
//         using var cmd = conn.CreateCommand();

//         cmd.BindByName = true;
//         cmd.CommandText = "SELECT list_id, list_name FROM fs231_aretzer.grocery_lists WHERE user_id = :userId";
//         cmd.Parameters.Add(":userId", OracleDbType.Int32).Value = userId;

//         var reader = cmd.ExecuteReader();
//         while (reader.Read())
//         {
//             lists.Add(new ShoppingList
//             {
//                 ListId = reader.GetInt32(reader.GetOrdinal("list_id")),
//                 ListName = reader.GetString(reader.GetOrdinal("list_name")),
//                 Items = new List<ShoppingItem>()
//             });
//         }

//         return lists;
//     }

//     public static bool InsertList(ShoppingList list)
//     {
//         using var conn = DbConnectionHelper.Connect();
//         using var cmd = conn.CreateCommand();

//         cmd.BindByName = true;
//         cmd.CommandText = @"INSERT INTO fs231_aretzer.grocery_lists (user_id, list_name)
//                             VALUES (:userId, :listName)";
//         //cmd.Parameters.Add(":listId", OracleDbType.Int32).Value = list.ListId;
//         cmd.Parameters.Add(":userId", OracleDbType.Int32).Value = list.UserId;
//         cmd.Parameters.Add(":listName", OracleDbType.Varchar2, 50).Value = list.ListName;



//         return cmd.ExecuteNonQuery() > 0;
//     }
// }
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

internal class ShoppingListStore
{
    // Holt alle Listen inkl. Items für einen User
    public static List<ShoppingList> GetLists(int userId)
    {
        var lists = new List<ShoppingList>();

        using var conn = DbConnectionHelper.Connect();

        // Erst alle Listen holen
        using (var cmd = conn.CreateCommand())
        {
            cmd.BindByName = true;
            cmd.CommandText = @"SELECT list_id, list_name 
                                FROM fs231_aretzer.grocery_lists 
                                WHERE user_id = :userId";
            cmd.Parameters.Add(":userId", OracleDbType.Int32).Value = userId;

            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var list = new ShoppingList
                {
                    ListId = reader.GetInt32(reader.GetOrdinal("list_id")),
                    UserId = userId,
                    ListName = reader.GetString(reader.GetOrdinal("list_name")),
                    Items = new List<ShoppingListItem>()
                };

                // Items für jede Liste holen
                list.Items = GetItemsForList(list.ListId, conn);
                lists.Add(list);
            }
        }

        return lists;
    }

    // Holt alle Items zu einer bestimmten Liste
    private static List<ShoppingListItem> GetItemsForList(int listId, OracleConnection conn)
    {
        var items = new List<ShoppingListItem>();

        using var cmd = conn.CreateCommand();
        cmd.BindByName = true;
        cmd.CommandText = @"
            SELECT i.item_id, i.item_name, gli.amount, gli.unit
            FROM fs231_aretzer.grocery_list_items gli
            JOIN fs231_aretzer.items i ON gli.item_id = i.item_id
            WHERE gli.list_id = :listId";
        cmd.Parameters.Add(":listId", OracleDbType.Int32).Value = listId;

        var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            items.Add(new ShoppingListItem
            {
                ItemId = reader.GetInt32(reader.GetOrdinal("item_id")),
                ItemName = reader.GetString(reader.GetOrdinal("item_name")),
                Amount = reader.IsDBNull(reader.GetOrdinal("amount")) ? "" : reader.GetString(reader.GetOrdinal("amount")),
                Unit = reader.IsDBNull(reader.GetOrdinal("unit")) ? "" : reader.GetString(reader.GetOrdinal("unit")),
                Category = "" // optional, falls du das in Items irgendwann brauchst
            });
        }

    return items;
    }


    // Fügt eine neue Liste + Items ein
    public static bool InsertList(ShoppingList list)
    {
        using var conn = DbConnectionHelper.Connect();
        using var trans = conn.BeginTransaction();

        try
        {
            // 1️⃣ Neue Liste anlegen
            using var cmd = conn.CreateCommand();
            cmd.BindByName = true;
            cmd.Transaction = trans;
            cmd.CommandText = @"
                INSERT INTO fs231_aretzer.grocery_lists (user_id, list_name)
                VALUES (:userId, :listName)
                RETURNING list_id INTO :listId";

            cmd.Parameters.Add(":userId", OracleDbType.Int32).Value = list.UserId;
            cmd.Parameters.Add(":listName", OracleDbType.Varchar2, 100).Value = list.ListName;

            var listIdParam = new OracleParameter(":listId", OracleDbType.Int32);
            listIdParam.Direction = System.Data.ParameterDirection.Output;
            cmd.Parameters.Add(listIdParam);

            cmd.ExecuteNonQuery();

            int listId = ((OracleDecimal)listIdParam.Value).ToInt32();

            // 2️⃣ Alle Items einfügen/verknüpfen
            foreach (var item in list.Items)
            {
                var existingItem = ItemStore.GetOrCreateItem(item.ItemName);

                using var itemCmd = conn.CreateCommand();
                itemCmd.BindByName = true;
                itemCmd.Transaction = trans;
                itemCmd.CommandText = @"
                    INSERT INTO fs231_aretzer.grocery_list_items (list_id, item_id, amount, unit)
                    VALUES (:listId, :itemId, :amount, :unit)";

                itemCmd.Parameters.Add(":listId", OracleDbType.Int32).Value = listId;
                itemCmd.Parameters.Add(":itemId", OracleDbType.Int32).Value = existingItem.ItemId;
                itemCmd.Parameters.Add(":amount", OracleDbType.Varchar2, 50).Value = item.Amount ?? "";
                itemCmd.Parameters.Add(":unit", OracleDbType.Varchar2, 50).Value = item.Unit ?? "";

                itemCmd.ExecuteNonQuery();
            }


            trans.Commit();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Fehler beim Einfügen der Liste: " + ex.Message);
            trans.Rollback();
            return false;
        }
    }
}
