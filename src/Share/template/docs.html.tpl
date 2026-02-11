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

<body class="dark:bg-neutral-900">
  <div class="text-white py-2 bg-block">
    <div class="container mx-auto flex items-center space-x-4">
      <div class="flex-none">
        <a href="/" class="text-2xl font-semibold max-sm:hidden sm:block">@{Name}</a>
      </div>
      <div class="flex-grow text-left flex space-x-4 items-center">
         @{NavMenus}
      </div>
      <div class="flex-none flex items-center">
        <div class="flex items-center gap-2">
          <input id="docSearchInput" placeholder="Search Docs"
            class="px-4 py-2 border border-gray-600 rounded-lg dark:bg-neutral-800 text-black dark:text-white focus:outline-none" />
          <button id="docSearchBtn" class="ml-2 bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded-lg">
            Search
          </button>
        </div>
        <div class="ms-3">
          @{TopActions}
        </div>
      </div>
    </div>
    </div>

    <div class="container mx-auto" style="margin-bottom: 48px;">
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
      
      <div class="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-4 gap-3 mt-2 pt-2">
        <div class="max-md:hidden md:block lg:col-span-1 sticky pe-4 top-0 h-fit">
            <div>
              <strong>Version</strong>
            </div>
            @{LeftNav}
        </div>
        <div class="col-span-1 md:col-span-2 lg:col-span-2 markdown-content px-3">
            <div class="flex justify-between items-center mb-2">
                <div class="text-neutral-200">📆 @{UpdateTime}</div>
                <div class="flex gap-3 items-center">
                    <button id="listNav" class="nav-toggle-btn" title="Navigation" aria-label="Open navigation menu">
                        <span class="text-xl">📑</span>
                    </button>
                    <a href="@{GithubLink}" target="_blank" rel="noopener noreferrer" class="p-1 rounded hover:bg-neutral-100 dark:hover:bg-neutral-800 transition-colors" title="Edit on GitHub">
                        <span class="text-xl">🖋️</span>
                    </a>
                </div>
             </div>
            <div>@{DocContent}</div>
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