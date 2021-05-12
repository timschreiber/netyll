## ![Netyll Leaf Logo](./netyll-logo-250x102.png)

**Netyll** is a simple, blog-aware, static website generation tool that is mostly compatible with [Jekyll](https://github.com/jekyll/jekyll) and aims maximum compatibility with [GitHub Pages](https://pages.github.com/). Unlike Jekyll, which does not officially support Windows, Netyll is built on the Microsoft .NET Core LTS release (currently 3.1) and does not require complicated Ruby installation and configuration to work on Windows.

Netyll follows the same conventions as Jekyll and should be compatible with basic Jekyll websites. If you are not familiar with Jekyll, have a read at [http://jekyllrb.com/docs/usage/](http://jekyllrb.com/docs/usage/).

### Usage

The principal commands are the following, and more information will be forthcoming in the wiki.

**New** is used to create the folder structure for a new website:

    netyll new

If the site should be created at a specific folder, it can be specified with a command line option:

    netyll --source-path c:\path\to\source

**Build** is used to generate a site based on the contents of a folder

To build a website from the current directory, with the output in the default `_site` subdirectory:

    netyll build

To build a website from a specific source directory, with the output in a specific destination directory:

    netyll build --source-path c:\path\to\source --destination-path c:\path\to\destination

**Serve** is for testing a website locally. When a file is changed in the source directory, the website is automatically regenerated.

To test a website from the current directory, and test on the default port (42069):

    netyll serve

To test a website from a specific source directory, with the output in a specific destination directory, using a specific port:

    netyll serve --source-path c:\path\to\source --destination-path c:\path\to\destination --port 8080

To stop testing press CTRL-C in the terminal window.

*More information will be forthcoming in the wiki*

### Plugins

Netyll will eventually support the same plugin features that Github Pages supports.

### Contributing

A Contribution policy will be created soon.

### Intent & Lineage

My primary intent for Netyll is to preview local changes to GitHub Pages websites before committing to their repositories. Previously, I used the now archived/abandoned [Pretzel](https://github.com/Code52/pretzel) project from [Code52](https://github.com/Code52). For Netyll, I started with the broken Pretzel source code and am modifying it to better match the primary intent of Netyll. Some of the major modifications include:

- Aligning the command names to their Jekyll counterparts,
- Removal of Razor template engine,
- Resolving other [Jekyll differences](https://github.com/Code52/pretzel/wiki/Jekyll-differences) (future).
- Working toward 100% compatibility with Github Pages (future).

The "Netyll" name evokes the project's relationship to .NET and Jekyll and is represented by the nettle leaf logo.

### License

Netyll: Copyright (c) 2021 Tim Schreiber, released under the terms of the [Microsoft Public License (MS-PL)](https://github.com/timschreiber/netyll/blob/main/LICENSE).

Pretzel source code used in Netyll: Copyright (c) 2012-present Code52, released under the terms of the Microsoft Public License.
