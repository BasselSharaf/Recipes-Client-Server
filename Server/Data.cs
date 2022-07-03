using System.Text.Json;
class Data
{
    private List<Recipe> _recipes { get; set; }
    private string _filePath;

    public Data ()
    {
        _recipes = new();
        // This method creates a path where we have access to read and write data inside the ProgramData folder
        var systemPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        _filePath = Path.Combine(systemPath, "Recipes.json");
        if (!File.Exists(this._filePath))
        {
            _recipes = new List<Recipe>();
            File.WriteAllText(this._filePath, JsonSerializer.Serialize(_recipes));
        }
        else
        {
            using (StreamReader r = new StreamReader(this._filePath))
            {
                var data = r.ReadToEnd();
                var json = JsonSerializer.Deserialize<List<Recipe>>(data);
                if (json != null)
                    _recipes = json;
            }
        }
    }

    public List<Recipe> GetRecipes()
    {
        return _recipes;
    }

    public void AddRecipe(Recipe r)
    {
        _recipes.Add(r);
    }

    public Recipe getRecipe(Guid id)
    {
        var recipe = _recipes.Find(r => r.Id == id);
        ArgumentNullException.ThrowIfNull(recipe, "No Recipe exists with this ID");
        return recipe;
    }

    public void RemoveRecipe(Guid id)
    {
        var recipe = getRecipe(id); 
        _recipes.Remove(recipe);
    }

    public void EditTitle(Guid id, string newTitle)
    {
        var recipe = getRecipe(id);
        recipe.Title = newTitle;
    }

    public void EditIngredients(Guid id, string newIngredients)
    {
        var recipe = getRecipe(id);
        recipe.Ingredients = newIngredients;
    }

    public void EditInstructions(Guid id, string newInstructions)
    {
        var recipe = getRecipe(id);
        recipe.Instructions = newInstructions;
    }

    public void AddCategory(Guid id, string newCategory)
    {
        var recipe = getRecipe(id);
        recipe.Categories.Add(newCategory);
    }

    public void RemoveCategory(Guid id, string category)
    {
        var recipe = getRecipe(id);
        recipe.Categories.RemoveAll(c => c == category);
    }

    public void EditCategory(Guid id, string category, string newCategory)
    {
        RemoveCategory(id, category);
        AddCategory(id, newCategory);
    }

    public void SaveRecipes()
    {
        File.WriteAllText(_filePath, JsonSerializer.Serialize(_recipes));
    }
}