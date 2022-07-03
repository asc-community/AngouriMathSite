## Repository of am.angouri.org

[![Is site operational](https://img.shields.io/website?label=am.angouri.org&up_message=works%21&url=https%3A%2F%2Fam.angouri.org)](https://am.angouri.org)

This repo contains all the files for <a href="https://am.angouri.org">the website</a> of [AngouriMath](https://github.com/asc-community/AngouriMath).

The master branch only contains files necessary for the generation itself. The generation happens automatically on every push to gh-pages branch. The content of the website is located at `_generator/content`.

There's a custom generator which wraps the content files with the given templates, which are located at `_generator/content/_templates`.

### Local running

To run the website locally, get [.NET 6](https://dotnet.microsoft.com/download/dotnet/6.0) and clone it:
```
git clone https://github.com/asc-community/AngouriMathSite
cd AngouriMathSite
```

Now run
```
./init.sh
```

Once it's finished, you can generate the website by running
```
./run.sh
```

Or, to make it open automatically,
```
./runAndOpen.sh
```

### Other info

Yandex.Metrica counter is now [public](https://metrica.yandex.com/stat/traffic?group=month&period=year&accuracy=1&id=72666283).
