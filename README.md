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
- Publish multilingual product landing pages with product-specific static assets.
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

The user guide is available at [dusi.dev/docs/EasyDocs.html](https://dusi.dev/docs/EasyDocs.html), and the source code is hosted at [github.com/AterDev/EasyDocs](https://github.com/AterDev/EasyDocs).

The CLI uses Spectre.Console.Cli for structured help and styled output:

```powershell
ezdoc --help
ezdoc --version
```

`init` and `build` are the command names in every locale. The CLI selects one localized resource set from the current UI culture, so Chinese systems show Chinese descriptions and English systems show English descriptions. Set `DOTNET_CLI_UI_LANGUAGE` to `zh-CN` or `en-US` to override the detected culture.

## Quick start

Create a workspace and its sample content:

```powershell
ezdoc init .
```

This creates `webinfo.json`, `preview.cs`, and a `Content` directory containing `blogs`, `docs/example/zh-cn/1.0`, `docs/example/en-us/1.0`, and `about.md`. The path is optional; without it, the current directory is used. Update `webinfo.json` after initialization and refer to the [official documentation](https://dusi.dev/docs/EasyDocs.html) for configuration details.

Build from the configuration file:

```powershell
ezdoc build .\webinfo.json
```

`build` accepts one argument: the path to `webinfo.json`. Input and output locations are read from `ContetPath` and `OutputPath` in that file. The generated site is written to `OutputPath` (by default `./WebSite`).

Preview the output with any static-file server, for example:

```powershell
npx http-server .\WebSite
```

Alternatively, use the generated `preview.cs` with the .NET SDK:

```powershell
dotnet run .\preview.cs -- .\WebSite
```

## Configuration

`webinfo.json` contains site-wide metadata and the documentation catalog:

```json
{
  "Name": "Niltor Blog",
  "Description": "A personal blog and documentation site",
  "AuthorName": "Ater",
  "EnableBlog": true,
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
  ],
  "ProductInfos": [
    {
      "Name": "MyProduct",
      "Description": "A sample product with multilingual product documentation.",
      "Logo": "logo.svg",
      "Languages": ["en-us", "zh-cn"],
      "DefaultLanguage": "en-us"
    }
  ]
}
```

Important configuration details:

- `ContetPath` is intentionally spelled this way for compatibility with existing configuration files.
- `BaseHref` must end with `/`. Use `/` when the site is deployed at the domain root.
- `Domain` is optional. When set, it is used for page canonical URLs and for `sitemap.xml`.
- `RepositoryUrl` and `Branch` are optional. When set, they enable documentation edit links.
- `EnableBlog` controls blog generation. Set it to `false` to omit the Blogs navigation, homepage blog cards, blog pages/data, and blog sitemap entries.
- `Icon` is the favicon path relative to the generated site. `Logo` is used for the site/documentation presentation where applicable.
- Each `DocInfos[].Name`, language, and version must match the corresponding directory names under `Content/docs`.
- Each `ProductInfos[].Name` and language must match the corresponding directory under `Content/products/<name>/<language>`. `DefaultLanguage` must be one of the configured languages and its directory must exist. A product `Logo` is stored at the product directory root.

## Content layout

```text
Content/
├── about.md
├── blogs/
│   ├── first-post.md
│   └── engineering/
│       └── second-post.md
├── custom/
├── products/
│   └── MyProduct/
│       ├── logo.svg
│       ├── privacy-policy.html
│       ├── en-us/
│       │   ├── .order
│       │   └── overview.md
│       └── zh-cn/
│           ├── .order
│           └── overview.md
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
- Products use `products/<name>/<language>/...`. The first Markdown file in the default language is used for the product landing page at `products/<name>.html`.
- A `.order` file controls the order of files or directories at that level. Entries use names without the `.md` suffix.
- Image files in blogs and docs are copied while the site is generated. There is no required directory name such as `images`; put the image beside the Markdown file or in a subdirectory and reference it with the correct relative path. The currently copied local image extensions are `.jpg`, `.jpeg`, `.png`, `.gif`, and `.svg`. For an image in the generated documentation homepage, use the `./_images/...` convention described below.
- Relative Markdown links to `.md` files are converted to `.html`; absolute HTTP(S) links are preserved.
- Non-Markdown files under a product directory are copied unchanged. For example, `Content/products/MyProduct/privacy-policy.html` remains available at `products/MyProduct/privacy-policy.html`.

### Images in Markdown

Image paths are resolved relative to the Markdown file. The directory does not have to be named `images`:

```text
Content/docs/EasyDoc/en-us/2.0/
├── Quick-Start.md
├── logo.svg
└── assets/
    └── architecture.png
```

Both of these references are valid when the files exist at the referenced paths:

```markdown
![Logo](logo.svg)
![Architecture](assets/architecture.png)
```

For local images in blogs and documentation, keep the file under the corresponding content tree and use one of the supported extensions listed above. Remote `http://` and `https://` image URLs are kept as-is and are not copied.

Versioned documentation pages support any matching relative image path. The generated documentation homepage at `docs/<name>.html` is based on the first document, and the current builder rewrites homepage image references only when they start with `./_images`. If that image must also appear on the documentation homepage, use this convention:

```text
Content/docs/EasyDoc/en-us/2.0/
├── Quick-Start.md
└── _images/
    └── architecture.png
```

```markdown
![Architecture](./_images/architecture.png)
```

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
