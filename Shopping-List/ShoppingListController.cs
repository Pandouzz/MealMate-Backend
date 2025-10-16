using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

[ApiController]
[Route("api/shoppinglist")]
public class ShoppingListController : ControllerBase
{
    [HttpGet("{userId}")]
    public IActionResult GetLists(int userId)
    {
        var lists = ShoppingListManager.GetListsForUser(userId);
        return Ok(lists);
    }

    [HttpPost]
    public IActionResult CreateList([FromBody] ShoppingList list)
    {
        Console.WriteLine("===== 📩 POST /api/shoppinglist aufgerufen =====");

        // bool success = ShoppingListManager.CreateList(list);
        // return Ok(new { success });

        
        if (list == null)
        {
            Console.WriteLine("❌ FEHLER: Body kam als NULL rein!");
            return BadRequest("Body war null – evtl. JSON-Struktur im Frontend falsch?");
        }

        // JSON-Ausgabe für Debug-Zwecke
        try
        {
            string json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine("📦 Empfangenes Objekt:");
            Console.WriteLine(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Fehler beim Serialisieren: {ex.Message}");
        }

        // Debug für Items
        if (list.Items == null || list.Items.Count == 0)
        {
            Console.WriteLine("ℹ️ Keine Items in der Liste enthalten.");
        }
        else
        {
            Console.WriteLine($"🧾 {list.Items.Count} Items empfangen:");
            foreach (var item in list.Items)
            {
                Console.WriteLine($"   - {item.ItemName} | Menge: {item.Amount} | Einheit: {item.Unit}");
            }
        }

        // Optional: Versuch zu speichern
        bool success = ShoppingListManager.CreateList(list);
        Console.WriteLine($"💾 Ergebnis Insert: {(success ? "✅ Erfolgreich" : "❌ Fehlgeschlagen")}");

        Console.WriteLine("==============================================");
        return Ok(new { success });
    }
}
