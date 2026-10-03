# Working in this repository

This is the source of [am.angouri.org](https://am.angouri.org), the website of
[AngouriMath](https://github.com/asc-community/AngouriMath). An agent maintaining it works as it does
in the library's repository, whose [AGENTS.md](https://github.com/asc-community/AngouriMath/blob/master/AGENTS.md)
sets the working practice for both. When there is nothing to do here, it goes back to the library.

## How the site is built and published

- `dotnet fsi amsite.fsx init` clones three things into `src/`: AngouriMath, whose XML documentation
  becomes the `/docs` pages through Yadg.NET; Yadg.NET; and the library's wiki, which becomes `/wiki`.
  `build` runs `src/NaiveStaticGenerator`, which wraps every page in `src/content` in
  `src/content/_templates/top.html` and `bottom.html` and writes `.output/final`. `run` builds and
  opens the home page.
- `.output/final` also gets the stylesheets, `img/`, `CNAME`, `robots.txt`, and a `sitemap.xml` and an
  `llms.txt` that the generator writes from the pages it produced.
- A pull request builds the site on Windows, Linux and macOS (`build-test.yml`).
- **A merge to master is live.** `deployment.yml` builds the site and pushes `.output/final` to the
  `gh-pages` branch, and GitHub Pages publishes it a few minutes later. Check the live page after a
  merge, not only the workflow.
- The `/docs` and `/wiki` pages are built from the library's master and wiki at deployment time, so a
  fix to either reaches the site at the next deployment of this repository.

## What a release of the library owes the site

The library's release checklist names it. The *What's new* page gets a block for the release, with
the link to `BREAKING-CHANGES.md` pinned to the release's tag, and the quickstart names the release.
The library's `docsamples` harness, run in its CI with this repository checked out, checks that every
code sample here compiles, runs and prints what its page says. Its report is the place to look before
editing a sample.

## Working practice

- One change per pull request, branched from master. Build it locally and look at the page it
  changes: a screenshot from a headless browser is enough.
- Read both comment endpoints of a pull request before merging it. The thread is not part of the
  checks.
- Load nothing from a third-party domain that a page does not need. A script from a domain the site
  does not control runs whatever that domain's next owner serves, which is why polyfill.io went (#36).
- Issues here carry the organisation's issue types and a milestone named after the library's
  version, as the library's do.
