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
<body class="dark:bg-neutral-900 pb-4">
    <div class="container mx-auto flex mt-2" style="margin-bottom: 48px;">
        <div class="w-1/4 max-sm:hidden sm:block text-right pr-4 mt-3">
        @{side}
        </div>
        <div class="sm:w-3/4 sm:pr-4 w-full markdown-content px-3">
        @{content}
        </div>
        <div class="w-1/4 mt-1 max-sm:hidden sm:flex">
            @{toc}
        </div>
    </div>
    <div class="footer py-2 bottom-0 w-full fixed">
      <div class="container mx-auto text-center">
        <p class="text-neutral-600 dark:text-neutral-300 mb-0">
        @{Name}
        <a class="text-blue-600" target="_blank" href="https://github.com/AterDev/EasyBlog">Powered by EasyDocs</a>
        </p>
      </div>
    </div>
</body>
</html>