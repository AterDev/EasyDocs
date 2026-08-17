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
    <link rel="canonical" href="@{CanonicalUrl}" />
    <link rel="stylesheet" href="@{BaseUrl}css/app.css" />
    <link rel="stylesheet" href="@{BaseUrl}css/docs.css" />
    <link rel="stylesheet" href="@{BaseUrl}css/markdown.css" />
    <link rel="icon" type="image/png" href="@{BaseUrl}@{FaviconPath}" />
    <script>const baseUrl = '@{BaseUrl}';</script>
    <script src="@{BaseUrl}js/products.js"></script>
</head>
<body class="site-body docs-page">
  <div class="site-header">
    <div class="layout-container site-header-inner">
      <div>
        <a href="@{BaseUrl}" class="site-logo">@{Name}</a>
      </div>
      <div class="site-nav">
         @{NavMenus}
      </div>
      <div class="site-actions">
        <div class="search-group">
          <input id="productSearchInput" placeholder="Search Product" class="search-input" />
          <button id="productSearchBtn" class="search-button">Search</button>
        </div>
        <div class="action-group">@{TopActions}</div>
      </div>
    </div>
  </div>

  <div class="doc-page-content page-content">
    <div id="productSearchData" class="hidden" data-productName="@{ProductName}" data-language="@{Language}"></div>
    <div class="doc-layout">
      <div class="doc-sidebar">@{LeftNav}</div>
      <div class="doc-main">
        <div class="doc-header"><div class="doc-meta">🔎 Search Results</div></div>
        <div id="productSearchResult" class="search-results"></div>
      </div>
      <div class="doc-toc">@{TOC}</div>
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
