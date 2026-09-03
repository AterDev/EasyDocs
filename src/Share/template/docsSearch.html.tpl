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
    <link rel="stylesheet" href="@{BaseUrl}css/app.css" />
    <link rel="stylesheet" href="@{BaseUrl}css/docs.css" />
    <link rel="stylesheet" href="@{BaseUrl}css/markdown.css" />
    <link rel="icon" type="image/png" href="@{BaseUrl}@{FaviconPath}" />
    <script>const baseUrl = '@{BaseUrl}';</script>
    <script src="@{BaseUrl}js/docs.js"></script>
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
      <div id="docData" class="hidden" data-id="" data-docName="@{DocName}" data-language="@{Language}" data-version="@{Version}"></div>
      <div id="docSearchData" class="hidden" data-docName="@{DocName}" data-language="@{Language}" data-version="@{Version}"></div>

      <div class="doc-layout">
        <div class="doc-sidebar">
            <div>
              <strong>Version</strong>
            </div>
            @{LeftNav}
        </div>
        <div class="doc-main">
          <div class="doc-header">
            <div class="doc-meta">🔎 Search Results</div>
          </div>
          <div id="docSearchResult" class="search-results"></div>
        </div>
        <div class="doc-toc">
            @{TOC}
        </div>
      </div>
    </div>
      <div class="site-footer">
      <div class="layout-container">
        <p class="footer-text">
        @{FooterText}
        </p>
    </div>
  </div>

  <script>
    (function () {
      const dataEl = document.getElementById('docSearchData');
      const inputEl = document.getElementById('docSearchInput');
      const btnEl = document.getElementById('docSearchBtn');
      const resultEl = document.getElementById('docSearchResult');
      if (!dataEl || !inputEl || !btnEl || !resultEl) return;

      const docName = dataEl.getAttribute('data-docName');
      const language = dataEl.getAttribute('data-language');
      const version = dataEl.getAttribute('data-version');

      const params = new URLSearchParams(window.location.search);
      const keyword = (params.get('keyword') || '').trim();
      inputEl.value = keyword;

      const searchUrl = baseUrl + `data/${docName}/${language}-${version}-search.json`;

      function normalize(text) {
        return (text || '').toLowerCase();
      }

      function buildLink(htmlPath) {
        return `${baseUrl}docs/${htmlPath}`;
      }

      function renderResults(items) {
        resultEl.innerHTML = '';
        if (!items || items.length === 0) {
          resultEl.innerHTML = '<div class="search-empty">No Match Results!</div>';
          return;
        }
        items.forEach(item => {
          const card = document.createElement('div');
          card.className = 'card';
          const inner = document.createElement('div');
          inner.className = 'card-body';
          const title = document.createElement('a');
          title.href = buildLink(item.HtmlPath);
          title.target = '_blank';
          title.className = 'card-title-link';
          title.innerText = `📄 ${item.Title}`;
          inner.appendChild(title);
          if (item.UpdatedTime) {
            const time = document.createElement('div');
            time.className = 'card-meta';
            time.innerText = `📆 ${item.UpdatedTime}`;
            inner.appendChild(time);
          }
          card.appendChild(inner);
          resultEl.appendChild(card);
        });
      }

      function runSearch(key, data) {
        if (!key) {
          renderResults(data.slice(0, 50));
          return;
        }
        const normalized = normalize(key);
        const filtered = data.filter(d => {
          const source = [d.Title, ...(d.Headings || [])].join(' ');
          return normalize(source).includes(normalized);
        });
        renderResults(filtered);
      }

      fetch(searchUrl)
        .then(res => res.json())
        .then(data => {
          runSearch(keyword, data || []);
        });

      function goSearch() {
        const value = inputEl.value.trim();
        const url = `${baseUrl}docs/${docName}/${language}/${version}/search.html?keyword=${encodeURIComponent(value)}`;
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
