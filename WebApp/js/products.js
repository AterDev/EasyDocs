class Products {
  productName = '';
  language = '';
  docId = '';

  constructor() {
    document.addEventListener('DOMContentLoaded', () => this.init());
  }

  init() {
    const dataEl = document.getElementById('productData') || document.getElementById('productSearchData');
    if (dataEl) {
      this.productName = dataEl.getAttribute('data-productName') || '';
      this.language = dataEl.getAttribute('data-language') || '';
      this.docId = dataEl.getAttribute('data-id') || '';
    }

    if (this.docId) {
      document.querySelectorAll('[data-doc-id="' + this.docId + '"]').forEach(item => {
        item.classList.add('active');
        let parent = item.parentElement;
        while (parent) {
          if (parent.tagName === 'UL' && !parent.classList.contains('root-list')) {
            parent.classList.add('active');
          }
          if (parent.tagName === 'LI') {
            parent.querySelector('.caret')?.classList.add('caret-down');
          }
          parent = parent.parentElement;
        }
      });
    }

    document.querySelectorAll('.caret').forEach(caret => {
      caret.addEventListener('click', () => {
        caret.parentElement?.querySelector('.nested')?.classList.toggle('active');
        caret.classList.toggle('caret-down');
      });
    });

    this.initMobileNav();
    this.initSearch();
    this.initSearchResults();
  }

  initMobileNav() {
    const listNav = document.getElementById('listNav');
    const mobileNav = document.getElementById('mobileNav');
    const navOverlay = document.getElementById('navOverlay');
    const closeNav = document.getElementById('closeNav');
    if (!listNav || !mobileNav || !navOverlay || !closeNav) return;

    const close = () => {
      mobileNav.classList.remove('open');
      navOverlay.classList.remove('show');
      document.body.style.overflow = '';
    };

    listNav.addEventListener('click', () => {
      mobileNav.classList.add('open');
      navOverlay.classList.add('show');
      document.body.style.overflow = 'hidden';
    });
    closeNav.addEventListener('click', close);
    navOverlay.addEventListener('click', close);
    mobileNav.addEventListener('click', event => {
      if (event.target.tagName === 'A') close();
    });
  }

  initSearch() {
    const input = document.getElementById('productSearchInput');
    const button = document.getElementById('productSearchBtn');
    if (!input || !button || !this.productName || !this.language) return;

    const goSearch = () => {
      const keyword = input.value.trim();
      const url = baseUrl + 'products/' + this.productName + '/' + this.language +
        '/search.html?keyword=' + encodeURIComponent(keyword);
      window.location.href = url;
    };
    button.addEventListener('click', goSearch);
    input.addEventListener('keydown', event => {
      if (event.key === 'Enter') goSearch();
    });
  }

  async selectLanguage(language) {
    if (language === this.language) {
      window.location.reload();
      return;
    }

    const data = await this.getData(language);
    const firstDoc = this.getFirstDoc(data);
    if (!firstDoc) {
      alert('The language ' + language + ' is not available for this product.');
      return;
    }

    const target = this.buildDocumentUrl(firstDoc.HtmlPath, language);
    const response = await fetch(target, { method: 'HEAD' });
    if (!response.ok) {
      alert('The language ' + language + ' is not available for this product.');
      return;
    }
    window.location.href = target;
  }

  async getData(language) {
    const response = await fetch(baseUrl + 'data/products/' + this.productName + '/' + language + '.json');
    if (!response.ok) return null;
    return response.json();
  }

  buildDocumentUrl(htmlPath, language) {
    let relativePath = (htmlPath || '').replaceAll('\\', '/').replace(/^\/+/, '');
    const productPrefix = this.productName + '/';
    if (relativePath.startsWith(productPrefix)) {
      relativePath = relativePath.slice(productPrefix.length);
    }
    const languagePrefix = language + '/';
    if (relativePath.startsWith(languagePrefix)) {
      relativePath = relativePath.slice(languagePrefix.length);
    }
    return baseUrl + 'products/' + this.productName + '/' + language + '/' + relativePath;
  }

  getFirstDoc(catalog) {
    if (!catalog) return null;
    if (catalog.FirstDocHtmlPath) return { HtmlPath: catalog.FirstDocHtmlPath };
    if (catalog.Docs && catalog.Docs.length > 0) return catalog.Docs[0];
    for (const child of catalog.Children || []) {
      const doc = this.getFirstDoc(child);
      if (doc) return doc;
    }
    return null;
  }

  initSearchResults() {
    const dataEl = document.getElementById('productSearchData');
    const resultEl = document.getElementById('productSearchResult');
    if (!dataEl || !resultEl) return;

    const keyword = (new URLSearchParams(window.location.search).get('keyword') || '').trim().toLowerCase();
    const input = document.getElementById('productSearchInput');
    if (input) input.value = keyword;

    const dataUrl = baseUrl + 'data/products/' + this.productName + '/' + this.language + '-search.json';
    fetch(dataUrl)
      .then(response => response.json())
      .then(items => {
        const matches = keyword
          ? items.filter(item => [item.Title, ...(item.Headings || [])].join(' ').toLowerCase().includes(keyword))
          : items.slice(0, 50);
        resultEl.innerHTML = '';
        if (matches.length === 0) {
          resultEl.innerHTML = '<div class="search-empty">No Match Results!</div>';
          return;
        }
        matches.forEach(item => {
          const card = document.createElement('div');
          card.className = 'card';
          const inner = document.createElement('div');
          inner.className = 'card-body';
          const title = document.createElement('a');
          title.href = this.buildDocumentUrl(item.HtmlPath, this.language);
          title.target = '_blank';
          title.className = 'card-title-link';
          title.innerText = '📄 ' + item.Title;
          inner.appendChild(title);
          card.appendChild(inner);
          resultEl.appendChild(card);
        });
      });
  }
}

const products = new Products();
