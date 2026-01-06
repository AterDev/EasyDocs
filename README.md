# EasyDocs

![NuGet Version](https://img.shields.io/nuget/v/Ater.EasyDocs)


🌐 [English](./README.md)   🌐[中文](./README_cn.md)

Do you want your own tech blog or documentation site? EasyDoc helps you generate pure static blogs and documentation portals that you can deploy anywhere with zero fuss.

The tool works as a command-line utility and will be published on both `npm` and `nuget`.

Demo: [NilTor's Blog](https://dusi.dev/)

> [!NOTE]
> Version V1.0 is no longer maintained—V2 brings richer features and more flexibility.

## 🎖️ Features

Compared with similar tools, EasyDoc offers:

- Minimal configuration to get started quickly
- Unified generation of homepage, blog, documentation, and about pages for a complete personal tech site
- Blog search plus category and archive filtering out of the box
- Support for local image paths inside documentation
- Light and dark themes that follow the system preference while staying mobile-friendly
- Enhanced markdown rendering with TOC, mermaid, nomnoml, Math, plus code highlighting and copy controls

## 🧩 Documentation support

- Render multiple documentation projects within the same site
- Publish multiple versions per documentation
- Deliver documentation in multiple languages

## 🚀 Install the tool

EasyDoc is distributed on `nuget`. Run the following command to install it globally:

```powershell
dotnet tool install -g Ater.EasyDocs
```

After installation, use the `ezdoc` command to operate the tool.

## 🛠️ Use EasyDoc

Pick a repository to store your markdown documents—assume it is located in the folder `MyDocs`. Open a terminal inside `MyDocs` to work in that workspace.

### Configure webinfo.json

Run `ezdoc init` or create `webinfo.json` manually with the structure below:

```json
{
  "Name": "Niltor Blog",
  "Description": "🗽 for freedom",
  "AuthorName": "Ater",
  "BaseHref": "/blazor-blog/",
  "Domain": "https://aterdev.github.io",
  "DocInfos": [
    {
      "Name": "EasyDoc",
      "Languages": [
        "zh-cn",
        "en-us"
      ],
      "Versions": [
        "2.0"
      ]
    },
    {
      "Name": "example",
      "Languages": [
        "zh-cn"
      ],
      "Versions": [
        "1.0"
      ]
    }
  ]
}
```

The `DocInfos` array tells EasyDoc which documentation projects to render and which languages/versions each project supports. Always keep a trailing `/` on `BaseHref`; set it to `/` when publishing from a root domain with no subfolder.

### Authoring content

Create a content directory (for example `Content`) containing the following:

- `blogs`: all markdown files here become blog posts
- `docs`: documentation files organized as `docs/<DocName>/<language>/<version>/...`
- `about.md`: markdown for the about page

The `docs` folder must mirror the structure defined in `DocInfos`. For example:

- docs
  - EasyDoc
    - zh-cn
      - 2.0
        - doc1.md
        - doc2.md
    - en-us
      - 2.0
        - doc1.md
- example
  - zh-cn
    - 1.0
      - doc.md

### Generate the static site

From the repository root run:

```pwsh
ezdoc build .\Content .\WebApp
```

This command converts every markdown file inside `Content` into a static site under `WebApp`. Preview with `http-server`, then deploy the resulting `WebApp` folder anywhere you host static content.

More details: [Official documentation](https://github.com/AterDev/EasyDocs)
