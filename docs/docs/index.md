---
layout : docslayout
title : "Quickstart"
section : "quickstart"
---

Netyll is a static site generator that is mostly compatible with [Jekyll](https://jekyllrb.com) and aims for full compatibility with [GitHub Pages](https://pages.github.com). It takes text written in Markdown or HTML and uses layouts to create a static website. You can change the site's look and feel, URLs, content, and more.

## Prerequisites

{% include docs-prerequisites.html %}

## Insructions

Using a Windows Command or Powershell terminal:

 1. Install the Netyll .NET tool by running the following [dotnet tool install](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-tool-install) command:
```
    dotnet tool install --global Netyll --version 1.0.0
```
 2. Create a new Netyll site at the current path
```
    netyll new
```
 3. Build the site and make it available on the built-in web server on the default port:
```
    netyll serve
```
 4. Browse to http://localhost:42069
