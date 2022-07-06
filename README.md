# Recipes service driven application
- This is small web application to manage recipes, I did it to learn more about C# and ASP.NET Core
![image](https://user-images.githubusercontent.com/15571269/177624728-7f472c05-e980-4a46-b69f-d693d3e0f3dd.png)

## Functionality
### User can add a recipe entry with the following details:

* Title
* Ingredients
* Instructions
* One or more categories 

### User can perform the following:

* Add and edit recipe categories.
* List, add and edit recipes.
* All the data must be stored in a json file on disk. 

### How the System works
* Application is divided into two parts console and backend
* The backend will provide REST APIs that the console will use.
* JSON is used to communicate data between the backend and the console.
* Backend handles all the business logic including the JSON reading/writing, creating a new recipe, etc.
* The console will make HTTP calls to the backend.

## What I learned
* C# coding standard
* Used ASP.Net Core 6
* Used C# 10.
* ASP.Net Core 6 Minimal Web API
* HttpClientFactory

## External Libraries Used
* [Spectre.Console](https://spectreconsole.net/)
