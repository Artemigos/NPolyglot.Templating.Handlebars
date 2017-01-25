# NPolyglot.Templating.Handlebars [![NuGet](https://img.shields.io/nuget/v/NPolyglot.Templating.Handlebars.svg)](https://www.nuget.org/packages/NPolyglot.Templating.Handlebars/0.1.0-alpha)

A package plugging the Handlebars template engine into NPolyglot.

To use:
* create a text file - `*.hbs` extension will get you code colorization in some editors
* set its build action to `HandlebarsTemplate` (either in Visual Studio or csproj)
* write the template

The file's name is used as the template's name (e.g. `@template my_template.hbs`)

A simple template could look like this:

```hbs
// we're assuming that the model object passed to the template is of type List<int>
public namespace DslNamespace
{
    public class DslClass
    {
        {{#each this}}
        public int Number{{this}} => {{this}};
        {{/each}}
    }
}
```

