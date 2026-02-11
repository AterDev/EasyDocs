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

<body class="dark:bg-neutral-900">
  <div class="text-white py-2 bg-block">
    <div class="container mx-auto flex items-center space-x-4">
      <div class="flex-none">
        <a href="/" class="text-2xl font-semibold max-sm:hidden sm:block">@{Name}</a>
      </div>
      <div class="flex-grow text-left flex space-x-4 items-center">
         @{NavMenus}
      </div>
      <div class="flex-none flex items-center gap-2">
        <input id="docSearchInput" placeholder="Search Docs"
          class="px-4 py-2 border border-gray-600 rounded-lg dark:bg-neutral-800 text-black dark:text-white focus:outline-none" />
        <button id="docSearchBtn" class="ml-2 bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded-lg">
          Search
        </button>
        @{TopActions}
      </div>
    </div>
    </div>

    <div class="container mx-auto" style="margin-bottom: 48px;">
      <div id="docData" class="hidden" data-id="" data-docName="@{DocName}" data-language="@{Language}" data-version="@{Version}"></div>
      <div id="docSearchData" class="hidden" data-docName="@{DocName}" data-language="@{Language}" data-version="@{Version}"></div>

      <div class="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-4 gap-3 mt-2 pt-2">
        <div class="max-md:hidden md:block lg:col-span-1 sticky pe-4 top-0 h-fit">
            <div>
              <strong>Version</strong>
            </div>
            @{LeftNav}
        </div>
        <div class="col-span-1 md:col-span-2 lg:col-span-2 px-3">
            <div class="flex justify-between items-center mb-2">
                <div class="text-neutral-200">🔎 Search Results</div>
            </div>
            <div id="docSearchResult" class="space-y-3"></div>
        </div>
        <div class="max-lg:hidden lg:block lg:col-span-1">
            @{TOC}
        </div>
      </div>
    </div>
    <div class="footer py-2 bottom-0 w-full fixed z-10">
    <div class="container mx-auto text-center">
        <p class="text-neutral-600 dark:text-neutral-300 mb-0">
        @{Name}
        <a class="text-blue-600" target="_blank" href="https://github.com/AterDev/EasyBlog">Powered by EasyDocs</a>
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
          resultEl.innerHTML = '<div class="text-neutral-400">No Match Results!</div>';
          return;
        }
        items.forEach(item => {
          const card = document.createElement('div');
          card.className = 'w-full rounded overflow-hidden shadow-lg dark:bg-neutral-800';
          const inner = document.createElement('div');
          inner.className = 'px-6 py-3';
          const title = document.createElement('a');
          title.href = buildLink(item.HtmlPath);
          title.target = '_blank';
          title.className = 'block text-lg py-2 text-neutral-600 hover:text-neutral-800 dark:text-neutral-300 dark:hover:text-neutral-100';
          title.innerText = `📄 ${item.Title}`;
          inner.appendChild(title);
          if (item.UpdatedTime) {
            const time = document.createElement('div');
            time.className = 'text-neutral-500 text-sm';
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
