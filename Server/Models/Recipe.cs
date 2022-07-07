using System;

class Recipe
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Ingredients { get; set; }
    public string Instructions { get; set; }
    public List<String> Categories { get; set; } = new();

    public Recipe(string title, string ingredients, string instructions, List<String> categories)
    {
        Id = Guid.NewGuid();
        Title = title;
        Ingredients = ingredients;
        Instructions = instructions;
        Categories = categories;
    }
}


