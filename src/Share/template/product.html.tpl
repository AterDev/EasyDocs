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
    <meta property="og:type" content="article" />
    <meta property="og:url" content="@{CanonicalUrl}" />
    <link rel="stylesheet" href="@{BaseUrl}css/app.css" />
    <link rel="stylesheet" href="@{BaseUrl}css/docs.css" />
    <link rel="stylesheet" href="@{BaseUrl}css/markdown.css" />
    <link rel="icon" type="image/png" href="@{BaseUrl}@{FaviconPath}" />
    <script>const baseUrl = '@{BaseUrl}';</script>
    <script src="@{BaseUrl}js/products.js"></script>
    <script src="@{BaseUrl}js/markdown.js"></script>
    @{ExtensionHead}
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
        <div class="action-group">
          @{TopActions}
        </div>
      </div>
    </div>
  </div>

  <div class="doc-page-content page-content">
    <div id="productData" class="hidden" data-id="@{DocId}" data-productName="@{ProductName}" data-language="@{Language}"></div>
    <div id="navOverlay" class="nav-overlay"></div>
    <div id="mobileNav" class="mobile-nav-drawer">
      <div class="mobile-nav-header">
        <button id="closeNav" class="close-nav-btn">✕</button>
      </div>
      <div class="mobile-nav-content">@{LeftNav}</div>
    </div>

    <div class="doc-layout">
      <div class="doc-sidebar">@{LeftNav}</div>
      <div class="doc-main markdown-content">
        <div class="doc-header">
          <div class="doc-meta">📆 @{UpdateTime}</div>
          <div class="doc-toolbar">
            <button id="listNav" class="nav-toggle-btn doc-toolbar-btn" title="Navigation" aria-label="Open navigation menu">
              <span>📑</span>
            </button>
            <a href="@{EditLink}" target="_blank" rel="noopener noreferrer" class="doc-toolbar-link" title="Edit">
              <span>🖋️</span>
            </a>
          </div>
        </div>
        <div>@{DocContent}</div>
      </div>
      <div class="doc-toc">@{TOC}</div>
    </div>
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
