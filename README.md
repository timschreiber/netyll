## Netyll

A simple static website generation tool that is mostly compatible with [Jekyll](https://github.com/jekyll/jekyll). Unlike Jekyll, which does not officially support Windows, Netyll is built on the Microsoft .NET Core LTS release (currently 3.1) and does not require complicated Ruby installation and configuration to work on Windows.

Netyll follows the same conventions as Jekyll and should be compatible with basic Jekyll websites. If you are not familiar with Jekyll, have a read at [http://jekyllrb.com/docs/usage/](http://jekyllrb.com/docs/usage/). Netyll is particularly useful for previewing local changes to GitHub Pages websites before commiting them to a repository for publication.

Netyll is heavily based on the archived/abandoned Pretzel project from Code52. The "Netyll" name evokes the project's relationship to .NET and Jekyll and is represented by the nettle leaf logo.

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

To test a website from the current directory, and test on the default port (http://localhost:42069):

    netyll serve

To test a website from a specific source directory, with the output in a specific destination directory, using a specific port:

    netyll serve --source-path c:\path\to\source --destination-path c:\path\to\destination --port 8080

To stop testing press CTRL-C in the terminal window.

*More information will be forthcoming in the wiki*

### Plugins

Plugins are not currently supported in Netyll, but will be soon.

### Contributing

A Contribution policy will be created soon.
