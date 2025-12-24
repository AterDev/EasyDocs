<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@{Title}</title>
    <meta name="description" content="@{Description}" />
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
        @{TopActions}
      </div>
    </div>
    </div>

    <div class="container mx-auto" style="margin-bottom: 48px;">
      <div id="docData" data-id="@{DocId}" class="hidden" data-docName="@{DocName}" data-language="@{Language}" data-version="@{Version}"></div>
      <div class="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-4 gap-3 mt-2 pt-2">
        <div class="max-md:hidden md:block lg:col-span-1 sticky pe-4 top-0 h-fit">
        <div>
          <strong>Version</strong>
        </div>
        @{LeftNav}
        </div>
        <div class="col-span-1 md:col-span-2 lg:col-span-2 markdown-content">
            <div class="flex justify-between items-center ">
                <div>📆 @{UpdateTime}</div>
                <div><a href="@{GithubLink}" target="_blank" rel="noopener noreferrer" title="Edit">🖋️</a></div>
             </div>
        <div>
        @{DocContent}</div>
        </div>
        <div class="max-lg:hidden lg:block lg:col-span-1">
        @{TOC}
        </div>
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