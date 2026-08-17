<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@{Title}</title>
    <meta name="description" content="@{Description}" />
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
    <meta property="og:type" content="website" />
    <meta property="og:url" content="@{CanonicalUrl}" />
    <meta name="twitter:card" content="summary" />
    <meta name="twitter:title" content="@{Title}" />
    <meta name="twitter:description" content="@{Description}" />
    <base href="@{BaseUrl}" />
    <link rel="stylesheet" href="@{BaseUrl}css/app.css" />
    <link rel="icon" type="image/png" href="@{BaseUrl}@{FaviconPath}" />
    <script>const baseUrl = '@{BaseUrl}';</script>
    <script src="@{BaseUrl}js/index.js"></script>
</head>
<body class="site-body">
    <div class="site-header">
    <div class="layout-container site-header-inner">
      <div>
        <a href="@{BaseUrl}" class="site-logo">@{Name}</a>
      </div>
      <div class="site-nav">
         @{navigations}
      </div>
      <div class="site-actions">
      </div>
    </div>
    </div>

    <div class="layout-container page-content">
    @{blogs}

    @{docs}

    @{products}
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
