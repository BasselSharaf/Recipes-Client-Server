using Spectre.Console;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
class Program
{
    private static readonly HttpClient s_httpClient = new();
    private static List<Recipe> s_recipes = new();
    static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        ConfigurationManager config = builder.Configuration;
        s_httpClient.BaseAddress = new Uri(config["url"]);
        s_httpClient.DefaultRequestHeaders
              .Accept
              .Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header
        while (true)
        {
            s_recipes = await FetchRecipesAsync();
            RecipeTableView();
            // User chooses the Action he would like to perform
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("What [red]action[/] would you like to perform ?")
                    .PageSize(10)
                    .MoreChoicesText("[grey](Move up and down to reveal more choices)[/]")
                    .AddChoices(new[] {
            "View Recipe","Add Recipe","Edit Recipe","Delete Recipe","Exit"
                    }));
            if (choice == "View Recipe")
            {
                // View recipes
                var recipeId = RecipeSelection("Choose which recipe you would like to view: ");
                AnsiConsole.Clear();
                AnsiConsole.Write(
                   new FigletText("Recipes")
               .LeftAligned()
               .Color(Color.Red));
                // detailed view of recipe
                var table = new Table().Border(TableBorder.Ascii2);
                table.Expand();
                table.AddColumn("[dodgerblue2]Title[/]");
                table.AddColumn(new TableColumn("[dodgerblue2]Ingredients[/]").LeftAligned());
                table.AddColumn(new TableColumn("[dodgerblue2]Instructions[/]").LeftAligned());
                table.AddColumn(new TableColumn("[dodgerblue2]Categories[/]").LeftAligned());
                // Add the details of the recipe to the table
                Recipe selectedRecipe = s_recipes.FirstOrDefault(r => r.Id == recipeId);
                table.AddRow(selectedRecipe.Title,
                             String.Join("\n", selectedRecipe.Ingredients.Split(",").Select(x => $"{x}")),
                             String.Join("\n", selectedRecipe.Instructions.Split(",").Select((x, n) => $"- {x}")),
                             String.Join("\n", selectedRecipe.Categories.Select((x) => $"- {x}")));
                AnsiConsole.Write(table);
                choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("What [red]action[/] would you like to perform on this recipe?")
                    .PageSize(10)
                    .MoreChoicesText("[grey](Move up and down to reveal more choices)[/]")
                    .AddChoices(new[] {
            "Edit Recipe","Delete Recipe","Back"
                    }));
            }
            switch (choice)
            {
                case "Add Recipe":
                    await AddRecipeAsync();
                    break;
                case "Edit Recipe":
                    await EditRecipeAsync();
                    break;
                case "Delete Recipe":
                    await DeleteRecipeAsync();
                    break;
                default: break;
            }
            AnsiConsole.Clear();
            if (choice == "Exit")
                break;
        }
    }

    private static async Task<List<Recipe>> FetchRecipesAsync()
    {
        var recipes = await s_httpClient.GetFromJsonAsync<List<Recipe>>("recipes");
        ArgumentNullException.ThrowIfNull(recipes, "Fetching recipes failed");
        return recipes;
    }

    private static async Task AddRecipeAsync()
    {
        var title = TakeInput("title");
        string ingredients = MultiLineInput("ingredients");
        string instructions = MultiLineInput("instructions");
        List<string> categories = ListInput("categories");
        var recipe = new Recipe(title, ingredients, instructions, categories);
        var recipeJson = new StringContent(
        JsonSerializer.Serialize(recipe),
        Encoding.UTF8,
        "application/json");
        using var httpResponseMessage =
                    await s_httpClient.PostAsync("recipes", recipeJson);
        httpResponseMessage.EnsureSuccessStatusCode();
    }

    static async Task EditRecipeAsync()
    {
        var recipeId = RecipeSelection("Choose which recipe you would like to edit?");
        var toEdit = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Which attribute would you like to edit?")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more choices)[/]")
                .AddChoices(new[] {
                "Title","Ingredients","Instructions","Categories"

                }));
        Recipe updatedRecipe = s_recipes.Where(r => r.Id == recipeId).FirstOrDefault();
        ArgumentNullException.ThrowIfNull(updatedRecipe, "Couldn`t find the recipe to update");
        if (toEdit != "Categories")
        {
            switch (toEdit)
            {
                case "Title":
                    updatedRecipe.Title = TakeInput("title");
                    break;
                case "Ingredients":
                    updatedRecipe.Ingredients = MultiLineInput(toEdit);
                    break;
                case "Instructions":
                    updatedRecipe.Instructions = MultiLineInput(toEdit);
                    break;
            }
            using var httpResponseMessage =
                        await s_httpClient.PutAsJsonAsync($"recipes/{recipeId}", updatedRecipe);
            httpResponseMessage.EnsureSuccessStatusCode();

        }
        else
        {
            await CategoryChoiceMakerAsync(recipeId);
        }
    }

    static async Task CategoryChoiceMakerAsync(Guid recipeId)
    {
        var choice = AnsiConsole.Prompt(
           new SelectionPrompt<string>()
               .Title("What would you like to do?")
               .PageSize(10)
               .MoreChoicesText("[grey](Move up and down to reveal more choices)[/]")
               .AddChoices(new[] {
                "Add Category","Edit Category","Delete Category"

               }));
        string category, newCategory;
        HttpResponseMessage httpResponseMessage;
        switch (choice)
        {
            case "Add Category":
                newCategory = AnsiConsole.Ask<string>("What is the name of your new [dodgerblue2]category[/]?");
                var categoryJson = new StringContent(
                    JsonSerializer.Serialize(newCategory),
                    Encoding.UTF8,
                    "application/json");
                httpResponseMessage =
                            await s_httpClient.PostAsync($"recipes/category?id={recipeId}&category={newCategory}", null);
                httpResponseMessage.EnsureSuccessStatusCode();
                break;
            case "Edit Category":
                category = AnsiConsole.Prompt(
                 new SelectionPrompt<string>()
                       .Title("Which Category would you like to edit?")
                       .PageSize(10)
                       .MoreChoicesText("[grey](Move up and down to reveal more choices)[/]")
                       .AddChoices(s_recipes.Where(r => r.Id == recipeId).ToList()[0].Categories.ToArray()));
                newCategory = AnsiConsole.Ask<string>("What is the new name of the [dodgerblue2]category[/]?");
                httpResponseMessage =
                            await s_httpClient.PutAsync($"recipes/category?id={recipeId}&category={category}&newCategory={newCategory}", null);
                httpResponseMessage.EnsureSuccessStatusCode();
                break;
            case "Delete Category":
                category = AnsiConsole.Prompt(
                 new SelectionPrompt<string>()
                       .Title("Which Category would you like to remove?")
                       .PageSize(10)
                       .MoreChoicesText("[grey](Move up and down to reveal more choices)[/]")
                       .AddChoices(s_recipes.Where(r => r.Id == recipeId).ToList()[0].Categories.ToArray()));
                httpResponseMessage =
                            await s_httpClient.DeleteAsync($"recipes/category?id={recipeId}&category={category}");
                httpResponseMessage.EnsureSuccessStatusCode();
                break;
            default: break;
        }
    }

    static async Task DeleteRecipeAsync()
    {
        var recipeId = RecipeSelection("Choose which recipe you would like to delete?");
        using var httpResponseMessage =
                    await s_httpClient.DeleteAsync($"recipes/{recipeId}");
        httpResponseMessage.EnsureSuccessStatusCode();
    }

    static Guid RecipeSelection(string text)
    {
        var recipeIndex = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                                .Title(text)
                                .PageSize(10)
                                .MoreChoicesText("[grey](Move up and down to reveal more choices)[/]")
                                .AddChoices(s_recipes.Select((r, n) => $"{n}- {r.Title}")))[0] - '0';
        return s_recipes[recipeIndex].Id;
    }


    static string MultiLineInput(string text)
    {
        AnsiConsole.Markup($"Please insert the [dodgerblue2]{text}[/] of your recipe \n");
        AnsiConsole.Markup("Press Enter after writing to add another\n If you are done write [red]Done[/] then press Enter \n");
        string input;
        string inputString = "";
        for (int i = 0; i < 30; i++)
        {
            input = AnsiConsole.Prompt(
            new TextPrompt<string>("- ")
            .ValidationErrorMessage("[red]This is not a valid INPUT[/]")
            .Validate(input =>
            {
                return input.Length < 3 ? ValidationResult.Error("[red]Need to write at least 3 letters[/]") : ValidationResult.Success();
            })
            ); ;
            if (input == "Done")
                break;
            inputString += $"{input}, ";
        }
        return inputString.Substring(0, inputString.Length - 2);
    }

    static List<string> ListInput(string text)
    {
        AnsiConsole.Markup($"Please insert the [dodgerblue2]{text}[/] of your recipe \n");
        AnsiConsole.Markup("Press Enter after writing to add another\n If you are done write [red]Done[/] then press Enter \n");
        string input;
        List<string> inputList = new List<string>();
        for (int i = 0; i < 30; i++)
        {
            input = AnsiConsole.Ask<string>("- ");
            if (input == "Done")
                break;
            inputList.Add(input);
        }
        return inputList;
    }

    static string TakeInput(string text)
    {
        return AnsiConsole.Prompt(
            new TextPrompt<string>($"What is the [dodgerblue2]{text}[/] of your recipe?")
            .ValidationErrorMessage("[red]This is not a valid INPUT[/]")
            .Validate(input =>
            {
                return input.Length < 3 ? ValidationResult.Error("[red]Need to write at least 3 letters[/]") : ValidationResult.Success();
            })
            );
    }

    static string StringLimiter(string input)
    {
        if (input.Length > 30)
            return input.Substring(0, 30) + "...";

        return input;
    }

    static string ListLimitedView(List<String> list)
    {
        string result = string.Join(", ", list.ToArray());
        if (result.Length > 30)
            return result.Substring(0, 30) + "...";
        return result;
    }

    static void RecipeTableView()
    {
        AnsiConsole.Write(
        new FigletText("Recipes")
            .LeftAligned()
            .Color(Color.Red));
        var table = new Table().Border(TableBorder.Ascii2);
        table.Expand();
        // Create Table Columns
        table.AddColumn("[dodgerblue2]Title[/]");
        table.AddColumn(new TableColumn("[dodgerblue2]Ingredients[/]").LeftAligned());
        table.AddColumn(new TableColumn("[dodgerblue2]Instructions[/]").LeftAligned());
        table.AddColumn(new TableColumn("[dodgerblue2]Categories[/]").LeftAligned());
        // Add the Recipes to the table
        s_recipes.ForEach(r => table.AddRow("[bold][red]" + r.Title + "[/][/]",
                                                StringLimiter(r.Ingredients),
                                                StringLimiter(r.Instructions),
                                                ListLimitedView(r.Categories)));
        AnsiConsole.Write(table);
    }
}
