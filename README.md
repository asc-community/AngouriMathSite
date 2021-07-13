## Repository of am.angouri.org

This repo contains all the files for <a href="https://am.angouri.org">the website</a> of [AngouriMath](https://github.com/asc-community/AngouriMath).

The master branch only contains files necessary for the generation itself. The generation happens automatically on every push to gh-pages branch. The content of the website is located at `_generator/content`.

There's a custom generator which wraps the content files with the given templates, which are located at `_generator/content/_templates`.

### Local running

To run the website locally, get [.NET 6](https://dotnet.microsoft.com/download/dotnet/6.0)
clone it correctly:
```
git clone --recurse-submodules https://github.com/asc-community/AngouriMathSite
cd AngouriMathSite
```
Now, build AngouriMath:
```
cd _generator/AngouriMath/Sources/AngouriMath
dotnet build -c release
```
Next, run the generator:
```
cd _generator/NaiveStaticGenerator
dotnet run -c release
```
Done, all files are generated in the root of the repository. 
