import { test, expect } from '@playwright/test';

const baseHref = '/e2e/';
const productEnglish = `${baseHref}products/MyProduct/en-us/Getting%20Started.html`;
const productChinese = `${baseHref}products/MyProduct/zh-cn/Getting%20Started.html`;
const productSearch = `${baseHref}products/MyProduct/en-us/search.html`;

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

  test('English product documentation renders three columns and its navigation', async ({ page }) => {
    await page.goto(productEnglish);

    await expect(page.locator('.doc-layout')).toBeVisible();
    await expect(page.locator('.doc-sidebar')).toBeVisible();
    await expect(page.locator('.doc-main')).toBeVisible();
    await expect(page.locator('.doc-toc')).toBeVisible();
    await expect(page.locator('.doc-sidebar')).toContainText('Getting Started');
    await expect(page.locator('.doc-main')).toContainText('Welcome to MyProduct');
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
