internal class ShoppingListManager
{
    public static List<ShoppingList> GetListsForUser(int userId)
    {
        return ShoppingListStore.GetLists(userId);
    }

    public static bool CreateList(ShoppingList list)
    {
        return ShoppingListStore.InsertList(list);
    }
}
