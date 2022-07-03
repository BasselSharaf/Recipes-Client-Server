using System.Net.Http;
using System.Net.Http.Headers;
using Spectre.Console;
using System.Text.Json.Serialization;
using System.Globalization;
using System.Text.Json;
class Program
{
    private static readonly HttpClient client = new HttpClient();
    private static readonly Data data = new Data();
    private static  List<Recipe> Recipes = new();
    static async Task Main(string[] args)
    {
        while (true)
        {
            Recipes = await FetchRecipes();
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
                Recipe selectedRecipe = data.Recipes.FirstOrDefault(r => r.Id == recipeId);
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
                    AddRecipe();
                    break;
                case "Edit Recipe":
                    EditRecipe();
                    break;
                case "Delete Recipe":
                    DeleteRecipe();
                    break;
                default: break;
            }
            data.SaveRecipes();
            AnsiConsole.Clear();
            if (choice == "Exit")
                break;
        }
    }

    private static async Task<List<Recipe>> FetchRecipes()
    {
        var httpRequestMessage = new HttpRequestMessage(
            HttpMethod.Get,
            "https://localhost:7182/getRecipes");
        var httpResponseMessage = await client.SendAsync(httpRequestMessage);
        using var contentStream =
            await httpResponseMessage.Content.ReadAsStreamAsync();
        var recipes = await JsonSerializer.DeserializeAsync
            <List<Recipe>>(contentStream);
        return recipes;
    }

    private static async Task AddRecipe()
    {
        var title = takeInput("title");
        string ingredients = MultiLineInput("ingredients");
        string instructions = MultiLineInput("instructions");
        List<string> categories = ListInput("categories");
        data.AddRecipe(new Recipe(title, ingredients, instructions, categories));
    }

    static void EditRecipe()
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
        if (toEdit != "Categories")
        {
            switch (toEdit)
            {
                case "Title":
                    data.EditTitle(recipeId, takeInput("title"));
                    break;
                case "Ingredients":
                    data.EditIngredients(recipeId, MultiLineInput(toEdit));
                    break;
                case "Instructions":
                    data.EditInstructions(recipeId, MultiLineInput(toEdit));
                    break;
            }
        }
        else
        {
            CategoryChoiceMaker(recipeId);
        }
    }

    static void CategoryChoiceMaker(Guid recipeId)
    {
        var choice = AnsiConsole.Prompt(
           new SelectionPrompt<string>()
               .Title("What would you like to do?")
               .PageSize(10)
               .MoreChoicesText("[grey](Move up and down to reveal more choices)[/]")
               .AddChoices(new[] {
                "Add Category","Edit Category","Delete Category"

               }));
        string category;
        switch (choice)
        {
            case "Add Category":
                data.AddCategory(recipeId, AnsiConsole.Ask<string>("What is the name of your new [dodgerblue2]category[/]?"));
                break;
            case "Edit Category":
                category = AnsiConsole.Prompt(
                 new SelectionPrompt<string>()
                       .Title("Which Category would you like to edit?")
                       .PageSize(10)
                       .MoreChoicesText("[grey](Move up and down to reveal more choices)[/]")
                       .AddChoices(data.Recipes.Where(r => r.Id == recipeId).ToList()[0].Categories.ToArray()));
                data.EditCategory(recipeId, category, AnsiConsole.Ask<string>("What is the new name of the [dodgerblue2]category[/]?"));
                break;
            case "Delete Category":
                category = AnsiConsole.Prompt(
                 new SelectionPrompt<string>()
                       .Title("Which Category would you like to remove?")
                       .PageSize(10)
                       .MoreChoicesText("[grey](Move up and down to reveal more choices)[/]")
                       .AddChoices(data.Recipes.Where(r => r.Id == recipeId).ToList()[0].Categories.ToArray()));
                data.RemoveCategory(recipeId, category);
                break;
            default: break;
        }
    }

    static void DeleteRecipe()
    {
        var recipeId = RecipeSelection("Choose which recipe you would like to delete?");
        data.RemoveRecipe(recipeId);
    }

    static Guid RecipeSelection(string text)
    {
        var recipeIndex = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                                .Title(text)
                                .PageSize(10)
                                .MoreChoicesText("[grey](Move up and down to reveal more choices)[/]")
                                .AddChoices(data.Recipes.Select((r, n) => $"{n}- {r.Title}")))[0] - '0';
        return data.Recipes[recipeIndex].Id;
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

    static string takeInput(string text)
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

    static string stringLimiter(string input)
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
        data.Recipes.ForEach(r => table.AddRow("[bold][red]" + r.Title + "[/][/]",
                                                stringLimiter(r.Ingredients),
                                                stringLimiter(r.Instructions),
                                                ListLimitedView(r.Categories)));
        AnsiConsole.Write(table);
    }
}
class Recipe
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Ingredients { get; set; }
    public string Instructions { get; set; }
    public List<String> Categories { get; set; }

    public Recipe(string title, string ingredients, string instructions, List<String> categories)
    {
        Id = Guid.NewGuid();
        Title = title;
        Ingredients = ingredients;
        Instructions = instructions;
        Categories = categories;
    }
}
public class Repository
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("html_url")]
    public Uri GitHubHomeUrl { get; set; }

    [JsonPropertyName("homepage")]
    public Uri Homepage { get; set; }

    [JsonPropertyName("watchers")]
    public int Watchers { get; set; }

    [JsonPropertyName("pushed_at")]
    public string JsonDate { get; set; }

    public DateTime LastPush =>
        DateTime.ParseExact(JsonDate, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
}