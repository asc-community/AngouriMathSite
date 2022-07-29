## How do I contribute?

Depends on what change you want to make. Most of the time you don't even need to clone it, just edit a file from this repo from github web-version and make a pull request.

If you want to change something fundamental and/or you need to test the result and/or you don't know where to make the fix, welcome to the rest of this guide.

## Overview on how the website is generated

This website is automatically generated based on a few things
### 1. The main branch of this repository.

That's where the templates reside, as well as themes, static pages (like `/why` and `/research` and the main page). So if you noticed a typo or bad wording or broken link in static pages, you're welcome to fix it.
- If this is related to static page, go to [`src/content`](https://github.com/asc-community/AngouriMathSite/tree/master/src/content)
- If this is related to footer/header/something common among all pages, go to [`src/content/_templates`](https://github.com/asc-community/AngouriMathSite/tree/master/src/content/_templates)
- If the generator is working wrong, there is [`src/NaiveStaticGenerator`](https://github.com/asc-community/AngouriMathSite/tree/master/src/NaiveStaticGenerator)

### 2. The [AngouriMath](https://github.com/asc-community/AngouriMath/) repo.

Why? Because that's where it takes the documentation. It clones this repo when you do `init` and then builds and extracts the documentation. That's how `/docs/namespaces.html` is made, so if there's an issue with it, you have to
- Fix the xml comments in the source code in the AngouriMath repo if there's something bad with a particular documentation page.
- Fix [Yadg.NET](https://github.com/WhiteBlackGoose/Yadg.NET) which generates html pages for documentation if there's something wrong with all of docs (e. g. code doesn't get highlighted or something like this)


### 3. The [wiki](https://github.com/asc-community/AngouriMath/wiki) repo.

That's how `/wiki` is made. Making changes to wiki is less trivial, so just open an issue for it. If it ever gets opened, then to trigger a new website build you need to open an issue / ping maintainers / do something that we see and trigger it.

## Local running

If you want to see the changes you made / to test what you get, you need to run the generator locally. It's very simple. After you cloned the repo, you need
1. To initialize dependencies: `dotnet fsi amsite.fsx init`
2. To generate the website and open main page: `dotnet fsi amsite.fsx run`

Now you should be in the main page. After any change you make, re-run it (enough to do `build` instead of `run` if you already opened the page you want to observe).

### Full docs on `amsite.fsx`

| Command                        | Description                                                             |
|:-------------------------------|:------------------------------------------------------------------------|
| `dotnet fsi amsite.fsx init`   | Initializes the modules (clones the AM's repo, wiki repo, and Yadg.NET) |
| `dotnet fsi amsite.fsx uninit` | Uninitializes modules (just deletes those folders)                      |
| `dotnet fsi amsite.fsx build`  | Runs the generator, assuming the modules are already initialized.       |
| `dotnet fsi amsite.fsx run`    | This does `build` plus opens the main page                              |
| `dotnet fsi amsite.fsx clean`  | Cleans the output directory                                             |

