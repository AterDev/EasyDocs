﻿<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@{Name}</title>
    <meta name="description" content="@{Description}" />
    <base href="/" />
    <link rel="stylesheet" href="https://rsms.me/inter/inter.css">
    <link rel="stylesheet" href="@{BaseUrl}css/app.css" />
    <link rel="icon" type="image/png" href="@{BaseUrl}favicon.ico" />
    <script>const baseUrl = '@{BaseUrl}';</script>
    <script src="//unpkg.com/alpinejs" defer></script>
    <script src="@{BaseUrl}js/index.js"></script>
    <style>
        [x-cloak] {
          display: none !important;
        }
    </style>
</head>
<body class="dark:bg-neutral-900">
    <div class="text-white py-2 bg-block">
    <div class="container mx-auto flex items-center space-x-4">
      <div class="flex-none">
        <a href="/" class="text-2xl font-semibold hidden sm:block text-blue-600">@{Name}</a>
      </div>
      <div class="flex-grow text-left flex space-x-4 items-center">
         @{navigations}
      </div>
      <div class="flex-none flex items-center">
      </div>
    </div>
    </div>

    <div class="container mx-auto mt-2" id="index-content">

    </div>
    </div>

    <div class="py-4 fixed bottom-0 w-full bg-block">
    <div class="container mx-auto text-center">
        <p class="text-neutral-600 dark:text-neutral-300">
        @{Name}
        <a class="text-blue-600" target="_blank" href="https://github.com/AterDev/EasyBlog">Powered by Ater Blog</a>
        </p>
    </div>
    </div>
</body>
</html>