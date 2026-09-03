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
    <meta name="twitter:card" content="summary" />
    <meta name="twitter:title" content="@{Title}" />
    <meta name="twitter:description" content="@{Description}" />
    <link rel="stylesheet" href="@{BaseUrl}css/app.css" />
    <link rel="stylesheet" href="@{BaseUrl}css/docs.css" />
    <link rel="stylesheet" href="@{BaseUrl}css/markdown.css" />
    <link rel="icon" type="image/png" href="@{BaseUrl}@{FaviconPath}" />
    <script>const baseUrl = '@{BaseUrl}';</script>
    <script src="@{BaseUrl}js/docs.js"></script>
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
          <input id="docSearchInput" placeholder="Search Docs" class="search-input" />
          <button id="docSearchBtn" class="search-button">
            Search
          </button>
        </div>
        <div class="action-group">
          @{TopActions}
        </div>
      </div>
    </div>
    </div>

    <div class="doc-page-content page-content">
      <div id="docData" data-id="@{DocId}" class="hidden" data-docName="@{DocName}" data-language="@{Language}" data-version="@{Version}"></div>
      
      <!-- Mobile Navigation Drawer Overlay -->
      <div id="navOverlay" class="nav-overlay"></div>
      
      <!-- Mobile Navigation Drawer -->
      <div id="mobileNav" class="mobile-nav-drawer">
        <div class="mobile-nav-header">
          <button id="closeNav" class="close-nav-btn">✕</button>
        </div>
        <div class="mobile-nav-content">
          @{LeftNav}
        </div>
      </div>
      
        <div class="doc-layout">
        <div class="doc-sidebar">
            <div>
              <strong>Version</strong>
            </div>
            @{LeftNav}
        </div>
        <div class="doc-main markdown-content">
          <div class="doc-header">
            <div class="doc-meta">👨‍💻 @{DocAuthor} &nbsp;&nbsp; 📆 @{UpdateTime}</div>
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
        <div class="doc-toc">
            @{TOC}
        </div>
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

  <script>
    (function () {
      const docData = document.getElementById('docData');
      const inputEl = document.getElementById('docSearchInput');
      const btnEl = document.getElementById('docSearchBtn');
      if (!docData || !inputEl || !btnEl) return;

      const docName = docData.getAttribute('data-docName');
      const language = docData.getAttribute('data-language');
      const version = docData.getAttribute('data-version');

      function goSearch() {
        const keyword = inputEl.value.trim();
        const url = `${baseUrl}docs/${docName}/${language}/${version}/search.html?keyword=${encodeURIComponent(keyword)}`;
        window.location.href = url;
      }

      btnEl.addEventListener('click', goSearch);
      inputEl.addEventListener('keydown', (e) => {
        if (e.key === 'Enter') {
          goSearch();
        }
      });
    })();
  </script>
</body>
</html>
