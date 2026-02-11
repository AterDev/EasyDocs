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
    <base href="/" />
    <link rel="stylesheet" href="@{BaseUrl}css/app.css" />
    <link rel="icon" type="image/png" href="@{BaseUrl}@{FaviconPath}" />
    <script>const baseUrl = '@{BaseUrl}';</script>
    <script src="@{BaseUrl}js/blogs.js"></script>
    <style>
        .dropdown:focus-within .dropdown-content {
          display: block;
        }
    </style>
</head>

<body class="dark:bg-neutral-900">
    <div class="text-white py-2 bg-block">
    <div class="container mx-auto flex items-center space-x-4">
      <div class="flex-none">
        <a href="/" class="text-2xl font-semibold max-sm:hidden sm:block text-blue-600">@{Name}</a>
      </div>
      <div class="flex-grow text-left flex space-x-4 items-center">
         @{navigations}
      </div>
      <div class="flex-none flex items-center">
        <input id="searchText" placeholder="博客标题"
          class="px-4 py-2 border border-gray-600 rounded-lg dark:bg-neutral-800 text-black dark:text-white focus:outline-none" />
        <button id="searchBtn" class="ml-2 bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded-lg">
          搜索
        </button>
      </div>
    </div>
    </div>

    <div class="container mx-auto mt-2" style="margin-bottom: 48px;">
    <div class="flex">
        <div class="sm:w-3/4 sm:pr-4 w-full">
        <!-- 博客卡片列表 -->
        <div class="px-3" id="blogList">
            <!-- 博客卡片内容 -->
            @{blogList}
        </div>
    </div>

    <div class="w-1/4 mt-1 max-sm:hidden sm:block">
        <!-- 分类 -->
        @{siderbar}
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
</body>
</html>