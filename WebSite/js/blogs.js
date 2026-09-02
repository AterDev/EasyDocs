const BaseUrl = baseUrl;
class Index {
    blogs = [];
    allBlogs = [];
    catalogs = [];
    webInfo;
    constructor() {
        document.addEventListener('DOMContentLoaded', () => this.init());
    }
    init() {
        this.getData();
        this.addEvent();
    }
    getData() {
        fetch(BaseUrl + 'data/blogs.json')
            .then(res => res.json()).then((data) => {
                this.allBlogs = this.getAllBlogs(data).sort((a, b) => {
                    return new Date(b.PublishTime).getTime() - new Date(a.PublishTime).getTime();
                });
                this.blogs = this.allBlogs.slice(0, 50);
                this.catalogs = data.Children;
            });
        fetch(BaseUrl + 'data/webinfo.json')
            .then(res => res.json()).then((data) => {
                this.webInfo = data;
            });
    }
    addEvent() {
        const self = this;
        document.getElementById('searchBtn').addEventListener('click', function () {
            var searchText = document.getElementById('searchText').value;
            self.search(searchText);
        });
        document.getElementById('searchText').addEventListener('keydown', function (e) {
            if (e.key === 'Enter') {
                var searchText = document.getElementById('searchText').value;
                self.search(searchText);
            }
        });
        document.getElementById("catalog-list").addEventListener('click', function (e) {
            if (e.target.classList.contains('filter-item')) {
                let catalogName = e.target.dataset.catalog ?? '';
                self.filterBlogs(catalogName, 'all');
            }
        });
        document.getElementById("date-list").addEventListener('click', function (e) {
            if (e.target.classList.contains('filter-item')) {
                let date = e.target.dataset.date ?? '';
                self.filterBlogs('all', date);
            }
        });
        var dates = document.querySelectorAll('.publish-time');
        dates.forEach((date) => {
            const dateTime = date.dataset.time;
            date.innerText = self.timeAgo(new Date(dateTime));
        });
    }
    search(key) {
        if (!key) {
            this.blogs = this.allBlogs.slice(0, 50);
        }
        else {
            this.blogs = this.allBlogs.filter(blog => blog.Title.toLowerCase().includes(key));
        }
        this.renderBlogs();
    }
    filterBlogs(catalogName, date) {
        if (catalogName != 'all') {
            let catalog = this.catalogs.find(catalog => catalog.Name == catalogName);
            if (catalog) {
                this.blogs = catalog.Docs;
            }
        }
        else if (date != 'all') {
            this.blogs = this.allBlogs.filter(blog => blog.PublishTime.substr(0, 7) == date);
        }
        else {
            this.blogs = this.allBlogs.slice(0, 50);
        }
        this.renderBlogs();
    }
    getAllBlogs(rootCatalog) {
        let blogs = [];
        blogs.push(...rootCatalog.Docs);
        if (rootCatalog.Children && rootCatalog.Children.length > 0) {
            rootCatalog.Children.forEach(catalog => {
                blogs.push(...this.getAllBlogs(catalog));
            });
        }
        return blogs;
    }
    renderBlogs() {
        let blogList = document.getElementById('blogList');
        if (blogList) {
            const pathName = window.location.pathname.replace('/blogs.html', '');
            blogList.innerHTML = '';
            this.blogs.forEach(blog => {
                let blogDiv = document.createElement('div');
                blogDiv.className = 'card';
                let blogInnerDiv = document.createElement('div');
                blogInnerDiv.className = 'card-body';
                let blogTitleDiv = document.createElement('div');
                blogTitleDiv.className = 'card-title';
                let blogLink = document.createElement('a');
                blogLink.href = pathName + 'blogs/' + blog.HtmlPath;
                blogLink.target = '_blank';
                blogLink.className = 'card-title-link';
                blogLink.innerText = "📑 " + blog.Title;
                blogTitleDiv.appendChild(blogLink);
                blogInnerDiv.appendChild(blogTitleDiv);
                let blogInfoP = document.createElement('p');
                blogInfoP.className = 'card-meta';
                blogInfoP.innerHTML = `👨‍💻 ${this.webInfo.AuthorName} &nbsp;&nbsp; ⏱️ ${this.timeAgo(new Date(blog.PublishTime))}`;
                blogInnerDiv.appendChild(blogInfoP);
                blogDiv.appendChild(blogInnerDiv);
                blogList?.appendChild(blogDiv);
            });
        }
    }
    timeAgo(date) {
        const seconds = Math.floor((new Date().getTime() - date.getTime()) / 1000);
        const intervals = {
            '年': 31536000,
            '月': 2592000,
            '天': 86400,
            '小时': 3600,
            '分钟': 60,
            '秒': 1
        };
        let counter;
        let values = [];
        for (const [unit, secondsPerUnit] of Object.entries(intervals)) {
            counter = Math.floor(seconds / secondsPerUnit);
            values.push([counter, unit]);
        }
        for (let i = 0; i < values.length; i++) {
            const [counter, unit] = values[i];
            if (counter > 0) {
                if (unit === '年') {
                    let month = Math.floor((seconds - counter * intervals[unit]) / intervals['月']);
                    let str = month > 0 ? month + '月' : '';
                    return `${counter}${unit}${str}前`;
                }
                if (unit === '月') {
                    let day = Math.floor((seconds - counter * intervals[unit]) / intervals['天']);
                    let str = day > 0 ? day + '天' : '';
                    return `${counter}${unit}${str}前`;
                }
                if (unit === '天') {
                    let hour = Math.floor((seconds - counter * intervals[unit]) / intervals['小时']);
                    let str = hour > 0 ? hour + '小时' : '';
                    return `${counter}${unit}${str}前`;
                }
                return `${counter}${unit}前`;
            }
        }
        return '刚刚';
    }
}
new Index();
