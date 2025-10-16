public class ShoppingList
{
    public int ListId { get; set; }
    public int UserId { get; set; }
    public string ListName { get; set; }
    public List<ShoppingItem> Items { get; set; }
}
