var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/recipes", () =>
{
    Data data = new();
    return data.GetRecipes();
});

app.MapGet("/recipes/{id}", (Guid id) =>
{
    Data data = new();
    Recipe recipe = data.getRecipe(id);
    return Results.Ok(recipe);
});

app.MapPost("/recipes", (Recipe recipe) =>
{
    Data data = new();
    recipe.Id = Guid.NewGuid();
    data.AddRecipe(recipe);
    data.SaveRecipes();
    return Results.Created($"/recipes/{recipe.Id}",recipe);
});

app.MapPut("/recipes/{id}", (Guid id) =>
{
    // TODO: Code update recipe
});

app.MapDelete("/recipes/{id}", (Guid id, Recipe recipe) =>
{
    Data data = new();
    data.RemoveRecipe(id);
    data.SaveRecipes();
    return Results.NotFound();
});

app.MapPost("recipes/category/{id}", (Guid id, string category) =>
{
    Data data = new();
    data.AddCategory(id,category);
    data.SaveRecipes();
    return Results.Created($"recipes/category/{category}",category);
});

app.MapPut("recipes/category/{id}", (Guid id, string category, string newCategory) => 
{
    Data data = new();
    data.EditCategory(id,category,newCategory);
    data.SaveRecipes();
    return Results.NoContent(); 
});

app.MapDelete("recipes/category/{id}", (Guid id, string category) =>
{
    Data data = new();
    data.RemoveCategory(id,category);
    data.SaveRecipes();
    return Results.NotFound();
});

app.Run();