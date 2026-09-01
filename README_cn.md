# EasyDocs

![NuGet Version](https://img.shields.io/nuget/v/Ater.EasyDocs)

🌐 [English](./README.md)   🌐[中文](./README_cn.md)

您是否想拥有自己的技术博客，或者文档站点？本工具将帮助您生成博客和文档的纯静态站点，让你可以轻松的部署到任何位置。

本工具将以命令行的方式提供，将在`npm`和`nuget`上发布。

效果展示：[NilTor's Blog](https://dusi.dev/)

> [!NOTE]
> V1.0版本将不再维护，V2提供更加全面和灵活的支持！

## 🎖️功能

相比其他类似工具，本工具具有以下特点：

- 极少的配置
- 同时支持主页/博客/文档/关于页的生成，形成完整的个人技术站点
- 支持博客的搜索和分类和存档筛选
- 自定义网站名称和说明
- 支持文档中本地图片路径
- 随系统变化的Light和Dark主题
- 移动端的自适应显示
- 良好的markdown渲染支持，包括：TOC/mermaid,nomnoml,Math的渲染以及代码高亮及代码复制操作
- 生成对 SEO 友好的 Meta 信息
- 支持通过 `Content/custom` 覆盖内置 CSS/JS/HTML，或添加自定义静态资源

对技术文档的生成支持：

- 支持多技术文档
- 支持文档的多个版本
- 支持文档的多语言

## 🚀安装工具

本工具发布在`nuget`上，你可以非常方便的通常以下命令安装。

### Nuget包

```powershell
dotnet tool install -g Ater.EasyDocs
```

安装完成后，你可以使用`ezdoc`命令来操作。

EasyDocs 使用 Spectre.Console.Cli 提供结构化帮助和带样式的输出。官方文档地址为 [dusi.dev/docs/EasyDocs.html](https://dusi.dev/docs/EasyDocs.html)，源码地址为 [github.com/AterDev/EasyDocs](https://github.com/AterDev/EasyDocs)。

```powershell
ezdoc --help
ezdoc --version
```

`init` 和 `build` 是所有语言环境下统一使用的命令名。CLI 会根据当前 UI 语言只显示对应的中文或英文说明；也可以通过环境变量 `DOTNET_CLI_UI_LANGUAGE` 设置为 `zh-CN` 或 `en-US` 覆盖系统检测结果。

## 🛠️使用工具

您需要有一个`代码仓库`用来存储您的`markdown`文档，我们假设你的仓库在目录`MyDocs`中。

现在定位到`MyDocs`目录。

### 配置文件

使用`ezdoc init`命令初始化`webinfo.json`文件，或手动创建该文件，文件内容如下：

执行初始化后还会生成`preview.cs`。请先修改`webinfo.json`配置；生成站点后，可以直接运行以下命令使用 .NET SDK 预览：

```powershell
dotnet run .\preview.cs -- .\WebSite
```

更多配置和使用说明请参考[官方文档](https://dusi.dev/docs/EasyDocs.html)。

```json
{
  "Name": "Niltor Blog", // 博客名称，显示在主页顶部导航
  "Description": "🗽 for freedom",// 说明，显示在主页顶部中间
  "AuthorName": "Ater", // 作者名称，显示在博客列表
  "EnableBlog": true, // 是否启用博客功能
  "BaseHref": "/blazor-blog/", // 子目录
  "Domain": "https://aterdev.github.io", // 域名，用于 sitemap 与 canonical
  "RepositoryUrl": "https://github.com/AterDev/EasyDocs", // 仓库地址，用于生成文档编辑链接
  "Branch": "main", // 仓库分支，默认 main
  "Icon": "favicon.ico", // 站点图标
  "Logo": "logo.png", // 站点 Logo
  "Keywords": "docs,blog,EasyDocs", // SEO 关键词(可选)
  "DocInfos": [
    {
      "Name": "EasyDoc",
      "Description": "官方文档", // 文档说明
      "Logo": "logo.png", // 文档 logo
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
        "zh-cn",
      ],
      "Versions": [
        "1.0",
      ]
    },
  ]
}
```

配置文件目前支持站点信息的配置，以及文档的配置。

文档的配置在`DocInfos`节点，它是一个数组，可以配置多个文档。每个文档只需要填写`文档名称`以及支持的`语言`和`版本`即可。

> [!IMPORTANT]
> 注意，`BaseHref`尾部的`/`是必需的。
>
> 如果你配置了自定义域名，并且没有使用子目录，请将BaseHref设置为`/`。

> [!NOTE]
> `RepositoryUrl` 和 `Branch` 用于生成文档编辑链接；`Domain` 会用于生成 `sitemap.xml` 和页面 canonical。
>
> `EnableBlog` 设置为 `false` 时，将不生成博客页面和博客数据，不显示顶部 Blogs 菜单及主页最新博客，也不会将博客加入 sitemap。

### 📃编写文档内容

现在我们可以编写文档了，首先创建一个文件夹，名称不限，如`Content`，然后在该目录下创建：

- blogs目录：该目录下的内容在生成时将作为博客内容
- docs目录：该目录下的内容在生成时将作为文档内容
- about.md：该文档将作为关于页内容进行展示
- custom目录：用于覆盖内置资源或添加自定义静态文件

docs目录需要与配置文件中文档的配置相对应，先是`文档名称`,然后是`语言`,然后是`版本`，其目录结构如下:

- docs
  - EasyDoc
    - zh-cn
      - 2.0
        - doc1.md
        - doc2.md
    - en-us
      - 2.0
        - doc1.md
        - doc2.md
        - xxx
          - doc.md
  - example
    - zh-cn
      - 1.0
        - doc.md

按照以上约定管理您的文档即可。

### Markdown 中使用图片

图片不要求必须放在名为 `images` 的目录中。图片路径是相对于当前 Markdown 文件解析的，可以和 Markdown 文件放在同一目录，也可以放在任意子目录中，但引用路径必须正确：

```text
Content/docs/EasyDoc/zh-cn/2.0/
├── 快速开始.md
├── logo.svg
└── assets/
    └── architecture.png
```

```markdown
![Logo](logo.svg)
![架构图](assets/architecture.png)
```

博客和文档中的本地图片会在生成时复制。目前支持复制的扩展名是 `.jpg`、`.jpeg`、`.png`、`.gif` 和 `.svg`，建议使用小写扩展名。图片应放在对应的内容目录下；远程 `http://` 或 `https://` 图片地址会直接保留，不会复制到输出目录。

版本化文档页支持正确的相对图片路径。不过，生成的文档入口页 `docs/<文档名>.html` 使用该文档的第一篇内容，当前构建器只会改写以 `./_images` 开头的入口页图片路径。如果图片还需要在文档入口页显示，请使用以下约定：

```text
Content/docs/EasyDoc/zh-cn/2.0/
├── 快速开始.md
└── _images/
    └── architecture.png
```

```markdown
![架构图](./_images/architecture.png)
```

### 🔨生成静态站点

在仓库根目录下，我们执行以下命令

```pwsh
ezdoc build .\webinfo.json
```

`build`接收一个`webinfo.json`路径，输入目录和输出目录由配置文件中的`ContetPath`和`OutputPath`决定，默认生成到`WebSite`目录。旧版本的`ezdoc build <内容目录> <输出目录>`命令格式已不再适用。

你可以使用`http-server`命令来启动一个本地服务器，查看生成的内容。

🎉 `WebApp`目录下就是静态网站需要的一切，你可以将它自由的部署到你需要的地方。

更多内容查看[官方文档](https://dusi.dev/docs/EasyDocs.html)。

### 🎨自定义样式和静态文件

在`Content/custom`目录下放置文件，构建完成后会递归复制到输出目录的相同相对路径，并覆盖已经存在的文件。例如：

```text
Content/custom/
├── css/
│   ├── app.css
│   ├── docs.css
│   └── markdown.css
├── js/site.js
└── images/banner.svg
```

因此可以通过`custom/css/app.css`、`custom/css/docs.css`和`custom/css/markdown.css`覆盖内置样式，也可以添加图片、字体或 JavaScript 等静态资源。任意新增的 CSS（例如`custom/css/site.css`）只会被复制，不会自动注入内置页面；如需加载它，需要覆盖对应的 HTML 页面，或将样式合并到内置 CSS 文件中。

`custom`文件是在所有页面生成完成后复制的，所以同路径文件会覆盖生成结果，`custom/index.html`等文件也可以替换对应的生成页面。修改后需要重新执行`ezdoc build`。

### 🧩产品内容

除了博客和技术文档外，还可以通过 `ProductInfos` 配置多语言产品内容：

```json
{
  "ProductInfos": [
    {
      "Name": "MyProduct",
      "Description": "一个包含多语言产品文档的示例产品",
      "Logo": "logo.svg",
      "Languages": ["en-us", "zh-cn"],
      "DefaultLanguage": "en-us"
    }
  ]
}
```

产品内容使用 `Content/products/<产品名称>/<语言>/...` 目录，`DefaultLanguage` 必须属于 `Languages`，并且对应语言目录必须存在。默认语言目录中的第一篇 Markdown 会作为产品入口页，生成到 `products/<产品名称>.html`。

示例目录结构如下：

```text
Content/
└── products/
    └── MyProduct/
        ├── logo.svg
        ├── privacy-policy.html
        ├── en-us/
        │   ├── .order
        │   └── overview.md
        └── zh-cn/
            ├── .order
            └── overview.md
```

`.order` 文件中的条目不带 `.md` 后缀。产品目录根部的非 Markdown 文件会原样复制，例如 `privacy-policy.html` 可通过 `products/MyProduct/privacy-policy.html` 访问。
