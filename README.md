## Repository of am.angouri.org

[![Is site operational](https://img.shields.io/website?label=am.angouri.org&up_message=works%21&url=https%3A%2F%2Fam.angouri.org)](https://am.angouri.org)

This repo contains all the files for <a href="https://am.angouri.org">the website</a> of [AngouriMath](https://github.com/AngouriMath/AngouriMath).

The master branch only contains files necessary for the generation itself. Every push to master generates the website and publishes it to the gh-pages branch, which GitHub Pages serves. The content of the website is located at `src/content`.

There's a custom generator which wraps the content files with the given templates, which are located at `src/content/_templates`.

## Local running

To run the website locally, get [.NET 10](https://dotnet.microsoft.com/download/dotnet/10.0) and clone it:
```
git clone https://github.com/AngouriMath/AngouriMathSite
cd AngouriMathSite
```

Now run
```
dotnet fsi amsite.fsx init
```

Once it's finished, you can run the website by doing
```
dotnet fsi amsite.fsx run
```

For more details, see [CONTRIBUTING.md](./CONTRIBUTING.md)

## Transparency

Telemetry removed, there's no tracking anymore.
