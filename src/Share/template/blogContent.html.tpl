<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <meta name="description" content="@{Title}" />
    <meta name="keywords" content="@{Keywords}" />
    <meta name="author" content="@{AuthorName}" />
    <meta name="robots" content="index, follow" />
    <meta name="generator" content="EasyDocs" />
    <meta name="application-name" content="@{Name}" />
    <meta name="color-scheme" content="light dark" />
    <link rel="canonical" href="@{CanonicalUrl}" />
    <meta property="og:site_name" content="@{Name}" />
    <meta property="og:title" content="@{Title}" />
    <meta property="og:description" content="@{Description}" />
    <meta property="og:type" content="article" />
    <meta property="og:url" content="@{CanonicalUrl}" />
    <meta name="twitter:card" content="summary" />
    <meta name="twitter:title" content="@{Title}" />
    <meta name="twitter:description" content="@{Description}" />
    <link rel="stylesheet" href="@{BaseUrl}css/app.css">
    <link rel="stylesheet" href="@{BaseUrl}css/markdown.css">
    <link rel="icon" type="image/png" href="@{BaseUrl}@{FaviconPath}" />
    <script src="@{BaseUrl}js/markdown.js"></script>
    <title>@{Title}-@{Name}</title>
    @{ExtensionHead}
</head>
<body class="site-body">
    <div class="layout-container page-content content-layout">
        <div class="content-side content-side-right">
        @{side}
        </div>
        <div class="content-main markdown-content">
        @{content}
        </div>
        <div class="content-side">
            @{toc}
        </div>
    </div>
    <div class="site-footer">
      <div class="layout-container">
        <p class="footer-text">
        @{Name}
        <a class="footer-link" target="_blank" href="https://github.com/AterDev/EasyBlog">Powered by EasyDocs</a>
        </p>
      </div>
    </div>
</body>
</html>