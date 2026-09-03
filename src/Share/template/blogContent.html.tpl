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
    <link rel="stylesheet" href="@{BaseUrl}css/docs.css">
    <link rel="stylesheet" href="@{BaseUrl}css/markdown.css">
    <link rel="icon" type="image/png" href="@{BaseUrl}@{FaviconPath}" />
    <script>const baseUrl = '@{BaseUrl}';</script>
    <script src="@{BaseUrl}js/docs.js"></script>
    <script src="@{BaseUrl}js/markdown.js"></script>
    <title>@{Title}-@{Name}</title>
    @{ExtensionHead}
</head>
<body class="site-body">
    <div class="site-header">
        <div class="layout-container site-header-inner">
            <div>
                <a href="@{BaseUrl}" class="site-logo">@{Name}</a>
            </div>
            <div class="site-nav">
                @{NavMenus}
            </div>
        </div>
    </div>
    <div class="blog-detail-layout page-content">
        <aside class="blog-detail-sidebar doc-sidebar">
        @{side}
        </aside>
        <main class="blog-detail-main markdown-content">
        <div class="doc-header">
            <div class="doc-meta">👨‍💻 @{DocAuthor} &nbsp;&nbsp; 📆 @{UpdateTime}</div>
        </div>
        @{content}
        </main>
        <aside class="blog-detail-toc">
            @{toc}
        </aside>
    </div>
    <div class="site-footer">
      <div class="layout-container">
        <p class="footer-text">
        @{FooterText}
        </p>
      </div>
    </div>
</body>
</html>
