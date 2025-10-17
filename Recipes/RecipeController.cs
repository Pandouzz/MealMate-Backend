using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class RecipesController : ControllerBase
{
    [HttpGet]
    public ActionResult<List<RecipeDto>> GetAllRecipes([FromQuery] int? userId)
    {
        try
        {
            var recipes = RecipeStore.GetAllRecipes(userId);
            var recipeDtos = recipes.Select(r => MapToDto(r)).ToList();
            return Ok(recipeDtos);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FEHLER: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            return StatusCode(500, $"Fehler beim Laden der Rezepte: {ex.Message}");
        }
    }

    [HttpGet("{id}")]
    public ActionResult<RecipeDto> GetRecipe(int id)
    {
        try
        {
            var recipe = RecipeStore.GetRecipeById(id);
            if (recipe == null)
                return NotFound($"Rezept mit ID {id} nicht gefunden");
            
            return Ok(MapToDto(recipe));
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Fehler beim Laden des Rezepts: {ex.Message}");
        }
    }

    [HttpPost]
    public ActionResult<RecipeDto> CreateRecipe([FromBody] RecipeDto recipeDto)
    {
        try
        {
            var recipe = MapToEntity(recipeDto);
            var createdRecipe = RecipeStore.CreateRecipe(recipe);
            return CreatedAtAction(nameof(GetRecipe), new { id = createdRecipe.RecipeId }, MapToDto(createdRecipe));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FEHLER: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            return StatusCode(500, $"Fehler beim Erstellen des Rezepts: {ex.Message}");
        }
    }

    [HttpPut("{id}")]
    public ActionResult<RecipeDto> UpdateRecipe(int id, [FromBody] RecipeDto recipeDto)
    {
        try
        {
            // Debug: Zeige empfangene Daten
            Console.WriteLine($"=== UPDATE REQUEST ===");
            Console.WriteLine($"ID: {id}");
            Console.WriteLine($"RecipeDto: {System.Text.Json.JsonSerializer.Serialize(recipeDto)}");
            Console.WriteLine($"ModelState Valid: {ModelState.IsValid}");
            
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"Validation Error - Key: {error.Key}, Errors: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
                return BadRequest(ModelState);
            }
            
            var recipe = MapToEntity(recipeDto);
            recipe.RecipeId = id;
            var updatedRecipe = RecipeStore.UpdateRecipe(recipe);
            if (updatedRecipe == null)
                return NotFound($"Rezept mit ID {id} nicht gefunden");
            
            return Ok(MapToDto(updatedRecipe));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FEHLER: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            return StatusCode(500, $"Fehler beim Aktualisieren des Rezepts: {ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    public ActionResult DeleteRecipe(int id)
    {
        try
        {
            var success = RecipeStore.DeleteRecipe(id);
            if (!success)
                return NotFound($"Rezept mit ID {id} nicht gefunden");
            
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Fehler beim Löschen des Rezepts: {ex.Message}");
        }
    }

    private RecipeDto MapToDto(Recipe recipe)
    {
        var zutatenDtos = recipe.Ingredients?.Select(ing => 
        {
            var item = ItemStore.GetItemById(ing.ItemId);
            return new ZutatDto
            {
                Zutat = item?.ItemName ?? "unbekannt",
                Menge = ing.Amount,
                Einheit = ing.Unit
            };
        }).ToList() ?? new List<ZutatDto>();

        return new RecipeDto
        {
            Id = recipe.RecipeId,
            Titel = recipe.RecipeName,
            Bild = recipe.ImageUrl,
            Zeit = $"{recipe.PreparationTime} Min",
            Portionen = $"{recipe.Portions} Portionen",
            Beschreibung = recipe.Description,
            Anleitung = recipe.Instructions,
            Vegetarisch = recipe.Vegetarian,
            Vegan = recipe.Vegan,
            Zutaten = zutatenDtos
        };
    }

    private Recipe MapToEntity(RecipeDto dto)
    {
        // Parse Zeit und Portionen
        int preparationTime = 0;
        int portions = 0;
        
        if (!string.IsNullOrEmpty(dto.Zeit))
        {
            var timeMatch = System.Text.RegularExpressions.Regex.Match(dto.Zeit, @"\d+");
            if (timeMatch.Success)
                int.TryParse(timeMatch.Value, out preparationTime);
        }

        if (!string.IsNullOrEmpty(dto.Portionen))
        {
            var portionMatch = System.Text.RegularExpressions.Regex.Match(dto.Portionen, @"\d+");
            if (portionMatch.Success)
                int.TryParse(portionMatch.Value, out portions);
        }

        var recipe = new Recipe
        {
            RecipeId = dto.Id,
            UserId = 5, // TODO: Aus Authentication Context holen
            RecipeName = dto.Titel,
            Description = dto.Beschreibung,
            Instructions = dto.Anleitung ?? "",
            Visibility = true,
            Vegetarian = dto.Vegetarisch,
            Vegan = dto.Vegan,
            ImageUrl = dto.Bild,
            PreparationTime = preparationTime,
            Portions = portions,
            Ingredients = new List<Ingredient>()
        };

        // Zutaten verarbeiten (jetzt strukturiert!)
        if (dto.Zutaten != null)
        {
            foreach (var zutatDto in dto.Zutaten)
            {
                if (!string.IsNullOrWhiteSpace(zutatDto.Zutat))
                {
                    var item = ItemStore.GetOrCreateItem(zutatDto.Zutat);
                    recipe.Ingredients.Add(new Ingredient
                    {
                        ItemId = item.ItemId,
                        Amount = (int)zutatDto.Menge,
                        Unit = zutatDto.Einheit ?? ""
                    });
                }
            }
        }

        return recipe;
    }
}