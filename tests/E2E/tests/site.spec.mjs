import { test, expect } from '@playwright/test';

const baseHref = '/e2e/';
const productEnglish = `${baseHref}products/MyProduct/en-us/Getting%20Started.html`;
const productChinese = `${baseHref}products/MyProduct/zh-cn/Getting%20Started.html`;
const productSearch = `${baseHref}products/MyProduct/en-us/search.html`;
const docsHomepage = `${baseHref}docs/MyProduct.html`;

test.describe('generated EasyDocs product site', () => {
  test('homepage exposes Docs and Products navigation', async ({ page }) => {
    await page.goto('');

    await expect(page.locator('body')).toContainText('Docs');
    await expect(page.locator('body')).toContainText('Products');
    await expect(page.getByRole('button', { name: /Products/ }).locator('.dropdown-icon')).toBeVisible();
    await expect(page.locator('a[href$="/products/MyProduct.html"]:visible').first()).toBeVisible();
    await expect(page.locator('img[src*="products/MyProduct/logo.png"]')).toBeVisible();
    await expect(page.locator('.blog-card:visible').filter({ hasText: 'MyProduct' }).first()).toBeVisible();
  });

  test('product navigation opens the default-language landing page', async ({ page, request }) => {
    await page.goto('');
    const productLink = page.locator('a[href$="/products/MyProduct.html"]:visible').first();
    await expect(productLink).toBeVisible();
    const productHref = await productLink.getAttribute('href');
    expect(productHref).toMatch(/products\/MyProduct\.html$/);
    if (!productHref) {
      throw new Error('The MyProduct product link has no href.');
    }

    const landingResponse = await request.get(productHref);
    expect(landingResponse.ok()).toBeTruthy();
    expect(await landingResponse.text()).toContain('Welcome to MyProduct');

    await page.goto(productHref);

    await expect(page).toHaveURL(/\/products\/MyProduct\.html$/);
    await expect(page.locator('.doc-main')).toContainText('Welcome to MyProduct');
  });

  test('documentation homepage resolves local images and styles blockquotes in dark theme', async ({ page, request }) => {
    await page.emulateMedia({ colorScheme: 'dark' });
    await page.goto(docsHomepage);

    const quote = page.locator('blockquote');
    await expect(quote).toContainText('A documented quote.');
    await expect(quote.locator(':scope > p')).toHaveCount(2);

    const quoteStyles = await quote.evaluate(element => {
      const styles = getComputedStyle(element);
      const paragraphStyles = getComputedStyle(element.querySelector(':scope > p'));
      return {
        backgroundColor: styles.backgroundColor,
        borderLeftColor: styles.borderLeftColor,
        borderLeftWidth: styles.borderLeftWidth,
        color: styles.color,
        paragraphColor: paragraphStyles.color
      };
    });
    expect(quoteStyles.backgroundColor).toBe('rgb(30, 30, 30)');
    expect(quoteStyles.borderLeftColor).toBe('rgb(64, 64, 64)');
    expect(quoteStyles.borderLeftWidth).toBe('4px');
    expect(quoteStyles.color).toBe('rgb(156, 163, 175)');
    expect(quoteStyles.paragraphColor).toBe(quoteStyles.color);

    const image = page.locator('img[alt="Architecture"]');
    await expect(image).toHaveAttribute(
      'src',
      './MyProduct/en-us/1.0/assets/architecture.svg?version=1#diagram'
    );
    const imageSrc = await image.getAttribute('src');
    if (!imageSrc) {
      throw new Error('The documentation homepage image has no src.');
    }

    const imageResponse = await request.get(new URL(imageSrc, page.url()).toString());
    expect(imageResponse.ok()).toBeTruthy();
  });

  test('English product documentation renders three columns and its navigation', async ({ page }) => {
    await page.goto(productEnglish);

    await expect(page.locator('.doc-layout')).toBeVisible();
    await expect(page.locator('.doc-sidebar')).toBeVisible();
    await expect(page.locator('.doc-main')).toBeVisible();
    await expect(page.locator('.doc-toc')).toBeVisible();
    await expect(page.locator('.doc-sidebar')).toContainText('Getting Started');
    await expect(page.locator('.doc-main')).toContainText('Welcome to MyProduct');

    const descriptionCell = page.locator('td').filter({ hasText: 'deliberately long table cell' });
    await expect(descriptionCell).toBeVisible();
    const tableTextLayout = await descriptionCell.evaluate(cell => {
      const range = document.createRange();
      range.selectNodeContents(cell);
      const cellRect = cell.getBoundingClientRect();
      const textRect = range.getBoundingClientRect();
      const lineTops = [...range.getClientRects()].map(rect => Math.round(rect.top));

      return {
        whiteSpace: getComputedStyle(cell).whiteSpace,
        lineCount: new Set(lineTops).size,
        staysInsideCell: textRect.right <= cellRect.right + 1
      };
    });
    expect(tableTextLayout.whiteSpace).toBe('normal');
    expect(tableTextLayout.lineCount).toBeGreaterThan(1);
    expect(tableTextLayout.staysInsideCell).toBeTruthy();
  });

  test('language switch changes the product documentation language', async ({ page }) => {
    await page.goto(productEnglish);
    await page.getByTitle('Language').click();

    const chineseOption = page.getByRole('link', { name: 'zh-cn', exact: true });
    await expect(chineseOption).toBeVisible();
    await chineseOption.click();

    await expect(page).toHaveURL(new RegExp(`${escapeRegExp(productChinese)}`));
    await expect(page.locator('.doc-main')).toContainText('欢迎使用 MyProduct');
  });

  test('product search returns the matching document from generated data', async ({ page, request }) => {
    await page.goto(`${productSearch}?keyword=Release`);

    await expect(page).toHaveURL(new RegExp(`${escapeRegExp(productSearch)}\\?keyword=Release$`));
    await expect(page.locator('#productSearchResult')).toContainText('Release Notes');
    await expect(page.locator('#productSearchResult a')).toHaveAttribute(
      'href',
      /products\/MyProduct\/en-us\//
    );

    const dataResponse = await request.get(`${baseHref}data/products/MyProduct/en-us.json`);
    const searchResponse = await request.get(`${baseHref}data/products/MyProduct/en-us-search.json`);
    expect(dataResponse.ok()).toBeTruthy();
    expect(searchResponse.ok()).toBeTruthy();
    expect(JSON.parse((await dataResponse.text()).replace(/^\uFEFF/, ''))).toBeTruthy();
    expect(JSON.parse((await searchResponse.text()).replace(/^\uFEFF/, ''))).toEqual(expect.arrayContaining([
      expect.objectContaining({ Title: 'Release Notes' })
    ]));
  });

  test('serves original privacy-policy HTML and generated About page', async ({ page }) => {
    await page.goto(`${baseHref}products/MyProduct/privacy-policy.html`);
    await expect(page).toHaveTitle('Privacy Policy');
    await expect(page.locator('body')).toContainText('Privacy Policy');
    await expect(page.locator('a')).toHaveAttribute('href', './en-us/Getting%20Started.html');

    await page.goto(`${baseHref}about.html`);
    await expect(page.locator('body')).toContainText('About EasyDocs E2E');
    await expect(page.locator('.about-layout')).toBeVisible();
    await expect(page.locator('.about-main')).toContainText('About EasyDocs E2E');
    await expect(page.locator('.blog-detail-sidebar')).toHaveCount(0);
  });

  test('honors BaseHref for document metadata and assets', async ({ page }) => {
    await page.goto('');

    await expect(page.locator('base')).toHaveAttribute('href', '/e2e/');
    await expect(page.locator('link[rel="canonical"]')).toHaveAttribute(
      'href',
      'https://example.test/e2e/'
    );
    await expect(page.locator('link[rel="stylesheet"]').first()).toHaveAttribute(
      'href',
      /\/e2e\/css\//
    );
  });
});

function escapeRegExp(value) {
  return value.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
}
