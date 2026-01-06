class Docs {
  docName = '';
  language = 'en-US';
  version = '1.0';
  docId = '';

  constructor() {
    document.addEventListener('DOMContentLoaded', () => this.init());
  }

  init() {
    const self = this;
    const docData = document.getElementById('docData');
    if (docData) {
      this.docName = docData.getAttribute('data-docName');
      this.language = docData.getAttribute('data-language');
      this.version = docData.getAttribute('data-version');
      this.docId = docData.getAttribute('data-id');
    }

    // Find all elements with the docId (both mobile and desktop navigation)
    const docLis = document.querySelectorAll(`[data-doc-id="${this.docId}"]`);
    docLis.forEach(docLi => {
      docLi.classList.add('active');
      let parent = docLi.parentElement;

      while (parent) {
        if (parent.tagName === 'UL') {
          if (parent.classList.contains('root-list')) {
            break;
          }
          parent.classList.add('active');
        }
        if (parent.tagName === 'LI') {
          // 取相同层级的 class= caret
          const caret = parent.querySelector('.caret');
          if (caret) {
            caret.classList.add('caret-down');
          }
        }
        parent = parent.parentElement;
      }
    });

    // set selected value for version select
    const versionSelect = document.getElementById('versionSelect');
    if (versionSelect) {
      const options = versionSelect.querySelectorAll('option');
      options.forEach(option => {
        if (option.value === this.version) {
          option.selected = true;
        }
      });
    }

    document.getElementById('versionSelect')?.addEventListener('change', function () {
      const selectedValue = this.value;
      if (selectedValue != this.version) {
        self.redirectToVersion(selectedValue);
      }
    });

    var toggler = document.getElementsByClassName("caret");
    for (var i = 0; i < toggler.length; i++) {
      toggler[i].addEventListener("click", function () {
        this.parentElement.querySelector(".nested").classList.toggle("active");
        this.classList.toggle("caret-down");
      });
    }

    // Mobile navigation drawer
    this.initMobileNav();
  }

  initMobileNav() {
    const listNav = document.getElementById('listNav');
    const mobileNav = document.getElementById('mobileNav');
    const navOverlay = document.getElementById('navOverlay');
    const closeNav = document.getElementById('closeNav');

    if (!listNav || !mobileNav || !navOverlay || !closeNav) {
      return;
    }

    const openNav = () => {
      mobileNav.classList.add('open');
      navOverlay.classList.add('show');
      document.body.style.overflow = 'hidden';
    };

    const closeNavDrawer = () => {
      mobileNav.classList.remove('open');
      navOverlay.classList.remove('show');
      document.body.style.overflow = '';
    };

    listNav.addEventListener('click', openNav);
    closeNav.addEventListener('click', closeNavDrawer);
    navOverlay.addEventListener('click', closeNavDrawer);

    // Close drawer when navigating to a doc link
    mobileNav.addEventListener('click', (e) => {
      if (e.target.tagName === 'A') {
        closeNavDrawer();
      }
    });
  }

  async selectLanguage(language) {
    if (language === this.language) {
      window.location.reload();
      return;
    }
    let doc = await this.getFirstDoc(language, null);

    if (doc) {
      if (doc.HtmlPath) {
        var url = new URL(window.location.href);
        let relativePath = doc.HtmlPath.replace(this.docName, '');
        url.pathname = `/docs/${this.docName}/${language}/${this.version}${relativePath}`;

        fetch(url.href)
          .then(response => {
            if (response.status === 404) {
              alert(`The language ${language} is not available for this document.`);
              return;
            } else {
              window.location.href = url.href;
            }
          });
      }
    } else {
      alert(`The language ${language} is not available for this document.`);
    }
  }

  async redirectToVersion(version) {
    let doc = await this.getFirstDoc(null, version);
    if (doc) {
      if (doc.HtmlPath) {
        var url = new URL(window.location.href);
        let relativePath = doc.HtmlPath.replace(this.docName, '');
        url.pathname = `/docs/${this.docName}/${this.language}/${version}${relativePath}`;

        fetch(url.href)
          .then(response => {
            if (response.status === 404) {
              alert(`The version ${version} is not available for this document.`);
              return;
            } else {
              window.location.href = url.href;
            }
          });
      }
    } else {
      alert(`The version ${version} is not available for this document.`);
    }
  }

  async getData(language, version) {
    language = language || this.language;
    version = version || this.version;
    const url = `/data/${this.docName}/${language}-${version}.json`;

    let res = await fetch(url);
    if (!res.ok) {
      console.log('Error:', res.status);
      return null;
    }
    const data = await res.json();
    return data;
  }

  async getFirstDoc(language, version) {
    let data = await this.getData(language, version);
    if (data) {
      if (data.Docs.length > 0) {
        return data.Docs[0];
      } else {
        let notNullFirst = data.Children.filter(item => item.Docs.length > 0);
        if (notNullFirst.length === 0) {
          return null;
        }
        return notNullFirst[0].Docs[0];
      }
    }
  }

}
const doc = new Docs();
