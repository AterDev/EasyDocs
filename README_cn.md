# EasyDocs

![NuGet Version](https://img.shields.io/nuget/v/Ater.EasyDocs)


🌐 [Englisth](./README.md)   🌐[中文](./README-cn.md)

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

## 🛠️使用工具

您需要有一个`代码仓库`用来存储您的`markdown`文档，我们假设你的仓库在目录`MyDocs`中。

现在定位到`MyDocs`目录。

### 配置文件

使用`ezdoc init`命令初始化`webinfo.json`文件，或手动创建该文件，文件内容如下：

```json
{
  "Name": "Niltor Blog", // 博客名称，显示在主页顶部导航
  "Description": "🗽 for freedom",// 说明，显示在主页顶部中间
  "AuthorName": "Ater", // 作者名称，显示在博客列表
  "BaseHref": "/blazor-blog/", // 子目录
  "Domain": "https://aterdev.github.io", // 域名，生成sitemap使用，不生成则留空
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

### 📃编写文档内容

现在我们可以编写文档了，首先创建一个文件夹，名称不限，如`Content`，然后在该目录下创建：

- blogs目录：该目录下的内容在生成时将作为博客内容
- docs目录：该目录下的内容在生成时将作为文档内容
- about.md：该文档将作为关于页内容进行展示

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

### 🔨生成静态站点

在仓库根目录下，我们执行以下命令

```pwsh
ezdoc build .\Content .\WebApp
```

该命令将会把`Content`目录下的所有内容生成静态站点，并生成到`WebApp`目录下。

你可以使用`http-server`命令来启动一个本地服务器，查看生成的内容。

🎉 `WebApp`目录下就是静态网站需要的一切，你可以将它自由的部署到你需要的地方。

更多内容查看[官方文档](https://github.com/AterDev/EasyDocs).
