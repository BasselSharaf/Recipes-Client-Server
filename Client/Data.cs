using System.Text.Json;
class Data
{
    public List<Recipe> Recipes { get; set; }
    private string _filePath;

    public Data ()
    {
        Recipes = new();
        // This method creates a path where we have access to read and write data inside the ProgramData folder
        var systemPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        _filePath = Path.Combine(systemPath, "Recipes.json");
        if (!File.Exists(this._filePath))
        {
            Recipes = new List<Recipe>();
            File.WriteAllText(this._filePath, JsonSerializer.Serialize(Recipes));
        }
        else
        {
            using (StreamReader r = new StreamReader(this._filePath))
            {
                var data = r.ReadToEnd();
                var json = JsonSerializer.Deserialize<List<Recipe>>(data);
                if (json != null)
                    Recipes = json;
            }
        }
    }

    public void AddRecipe(Recipe r)
    {
        Recipes.Add(r);
    }

    public void RemoveRecipe(Guid id)
    {
        Recipes.Remove(Recipes.Find(r => r.Id == id));
    }

    public void EditTitle(Guid id, string newTitle)
    {
        Recipes.Find(r => r.Id == id).Title = newTitle;
    }

    public void EditIngredients(Guid id, string newIngredients)
    {
        Recipes.Find(r => r.Id == id).Ingredients = newIngredients;
    }

    public void EditInstructions(Guid id, string newInstructions)
    {
        Recipes.Find(r => r.Id == id).Instructions = newInstructions;
    }

    public void AddCategory(Guid id, string newCategory)
    {
        Recipes.Find(r => r.Id == id).Categories.Add(newCategory);

    }

    public void RemoveCategory(Guid id, string category)
    {
        Recipes.Where(r => r.Id == id).ToList()[0].Categories.RemoveAll(c => c == category);
    }

    public void EditCategory(Guid id, string category, string newCategory)
    {
        RemoveCategory(id, newCategory);
        AddCategory(id, newCategory);
    }

    public void SaveRecipes()
    {
        File.WriteAllText(_filePath, JsonSerializer.Serialize(Recipes));
    }

}