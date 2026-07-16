# EasyDocs

![NuGet Version](https://img.shields.io/nuget/v/Ater.EasyDocs)

🌐 [English](./README.md)　🌐 [中文](./README_cn.md)

EasyDocs (`ezdoc`) turns Markdown content into a pure static blog and documentation site. The generated output contains only HTML, CSS, JavaScript, JSON, and static assets, so it can be deployed to GitHub Pages, a CDN, or any ordinary static-file server.

Demo: [NilTor's Blog](https://dusi.dev/)

> [!NOTE]
> Version 1.0 is no longer maintained. The current 2.x line includes the static documentation portal, multilingual/versioned docs, search, SEO metadata, and custom static assets.

## Features

- Generate a homepage, blog list, blog pages, documentation pages, documentation homepages, search pages, and an about page.
- Configure multiple documentation projects, languages, and versions in one site.
- Blog search, catalog filtering, and archive filtering.
- Markdown rendering with tables, task lists, alerts, citations, figures, auto-links, heading anchors, mathematics, Mermaid/nomnoml diagrams, syntax highlighting, and code-copy controls.
- Local images in blogs and documentation are copied to the generated site.
- Responsive light/dark styling that follows the browser or operating-system preference.
- Optional `Domain`-based canonical URLs and `sitemap.xml` generation.
- Optional repository/branch information for “Edit on GitHub” links on documentation pages.
- A `Content/custom` directory for overriding packaged assets or adding custom static files.

## Install

```powershell
dotnet tool install -g Ater.EasyDocs
```

The installed command is `ezdoc`.

## Quick start

Create a workspace and its sample content:

```powershell
ezdoc init .
```

This creates `webinfo.json` and a `Content` directory containing `blogs`, `docs/example/zh-cn/1.0`, `docs/example/en-us/1.0`, and `about.md`. The path is optional; without it, the current directory is used.

Build from the configuration file:

```powershell
ezdoc build .\webinfo.json
```

`build` accepts one argument: the path to `webinfo.json`. Input and output locations are read from `ContetPath` and `OutputPath` in that file. The generated site is written to `OutputPath` (by default `./WebSite`).

Preview the output with any static-file server, for example:

```powershell
npx http-server .\WebSite
```

## Configuration

`webinfo.json` contains site-wide metadata and the documentation catalog:

```json
{
  "Name": "Niltor Blog",
  "Description": "A personal blog and documentation site",
  "AuthorName": "Ater",
  "ContetPath": "./Content",
  "OutputPath": "./WebSite",
  "BaseHref": "/blazor-blog/",
  "Domain": "https://aterdev.github.io",
  "RepositoryUrl": "https://github.com/AterDev/EasyDocs",
  "Branch": "main",
  "Icon": "favicon.ico",
  "Keywords": "docs,blog,EasyDocs",
  "DocInfos": [
    {
      "Name": "EasyDoc",
      "Description": "Official documentation",
      "Logo": "logo.png",
      "Languages": ["zh-cn", "en-us"],
      "Versions": ["2.0"]
    }
  ]
}
```

Important configuration details:

- `ContetPath` is intentionally spelled this way for compatibility with existing configuration files.
- `BaseHref` must end with `/`. Use `/` when the site is deployed at the domain root.
- `Domain` is optional. When set, it is used for page canonical URLs and for `sitemap.xml`.
- `RepositoryUrl` and `Branch` are optional. When set, they enable documentation edit links.
- `Icon` is the favicon path relative to the generated site. `Logo` is used for the site/documentation presentation where applicable.
- Each `DocInfos[].Name`, language, and version must match the corresponding directory names under `Content/docs`.

## Content layout

```text
Content/
├── about.md
├── blogs/
│   ├── first-post.md
│   └── engineering/
│       └── second-post.md
├── custom/
└── docs/
    └── EasyDoc/
        ├── en-us/2.0/
        │   ├── .order
        │   └── getting-started.md
        └── zh-cn/2.0/
            └── 开始使用.md
```

- Every Markdown file under `blogs` becomes a blog page. Subdirectories become blog catalogs.
- `about.md` becomes `about.html`.
- Documentation uses `docs/<name>/<language>/<version>/...`. The directory names must be declared in `DocInfos`.
- A `.order` file controls the order of files or directories at that level. Entries use names without the `.md` suffix.
- Image files in blogs and docs are copied while the site is generated. Relative Markdown links to `.md` files are converted to `.html`; absolute HTTP(S) links are preserved.

## Custom styles and static files

The current release supports file-based customization through `Content/custom`:

```text
Content/
└── custom/
    ├── css/
    │   ├── app.css
    │   ├── docs.css
    │   └── markdown.css
    ├── js/
    │   └── site.js
    ├── index.html
    └── images/
        └── banner.svg
```

After all generated pages and packaged assets are created, every file under `Content/custom` is copied recursively to the same relative path under `OutputPath`. Existing files are overwritten. This gives you two practical customization options:

1. Put a replacement at the same path to override a packaged asset, such as `custom/css/app.css`, `custom/css/docs.css`, or `custom/css/markdown.css`.
2. Put additional images, fonts, JavaScript, or other static files under `custom`; reference them from Markdown or from your overridden HTML/CSS.

An arbitrary file such as `custom/css/site.css` is copied, but it is not automatically linked by the built-in templates. To load a new stylesheet on every page, either override the relevant template output in `custom` or override one of the built-in CSS files. Because custom files are copied last, a custom `index.html`, `blogs.html`, `about.html`, or another generated page can also replace the generated version.

The built-in templates load these packaged stylesheets:

- Homepage and blog list: `css/app.css`
- Documentation pages/search pages: `css/app.css`, `css/docs.css`, and `css/markdown.css`
- Blog content/about pages: `css/app.css` and `css/markdown.css`

Custom files are copied only during `ezdoc build`; editing `Content/custom` alone does not update an already generated output directory.

## Deployment

The `OutputPath` directory is the complete static site. Deploy its contents without requiring a .NET runtime. When changing the deployment subpath, update `BaseHref` and rebuild so generated links, scripts, stylesheets, canonical URLs, and sitemap entries use the correct base path.

## Development

Build the CLI project with:

```powershell
dotnet build .\src\BuildSite\BuildSite.csproj -c Release
```

The repository also contains the source CSS/JavaScript under `WebApp`. If those packaged assets are changed, run `./pack.ps1` to refresh `src/Share/template/web.zip` before packaging the tool.
