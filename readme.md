# dehungarian

![](http://i.imgur.com/pHZEiO1.png)


### What Is This?

A Visual Studio 2015 VSIX extension that will help rid your code of identifiers (variables, parameters, etc) using hungarian naming conventions. It finds variables and method parameters and suggests new names without the hungarian prefix.

Perhaps you have inherited a project that uses hungarian notation, and you'd like to correct that.

Use this extension to automatically rename all identifiers quickly and accurately. This uses built-in/native renaming functionality in Visual Studio.

### How Can I Get This Extension?

- Download the VSIX from the VS Gallery

   [https://visualstudiogallery.msdn.microsoft.com/46c136f0-101e-41c4-b0c3-5e62feed2a10](https://visualstudiogallery.msdn.microsoft.com/46c136f0-101e-41c4-b0c3-5e62feed2a10)

- [search for it in VS' Extensions window](http://i.imgur.com/WNMhK7V.png).

![](http://i.imgur.com/AmvjvpI.png)

### How?

- uses [Roslyn code analysis](https://github.com/dotnet/roslyn)
- finds variables and method parameters with common hungarian prefixes
   `str`, `s`, `c`, `ch`, `n`, `f`, `i`, `l`, `p`, `d`, `b`, `bln`, `o`, `obj` 
- takes the remainder of the variable name and renames the identifier.
- ignores variables who aren't camelCased and/or those without a capital letter immediately following the prefix.
- removes the prefix, and lower-cases the first char of the remaining variable name. The implicit assumption is that the variable is well named after the hungarian prefix.

### Renaming Examples

Your Variable  | Renamed To
------------- | -------------
`strCustomerName`  | `customerName`
`sFullName`| `fullName`
`objReturnVal`| `returnVal`
`bHasRedBalloons`| `hasRedBalloons`
`cQuote`| `quote`
`fAmountRefunded`| `amountRefunded`

### Ignored Variables

These will be ignored and not identified for renaming.

- `strcustomer` because this is not camel cased.
- `strangeName` because it's not exactly `str` followed by a capital letter.
- `insider`
- `boatName`
- `longRoad`
