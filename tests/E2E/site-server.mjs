import { createServer } from 'node:http';
import { promises as fs } from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import { spawn } from 'node:child_process';
import { fileURLToPath } from 'node:url';

const e2eRoot = path.dirname(fileURLToPath(import.meta.url));
const repoRoot = path.resolve(e2eRoot, '..', '..');
const fixtureRoot = path.join(e2eRoot, 'fixtures');
const baseHref = normalizeBaseHref(process.env.E2E_BASE_HREF || '/e2e/');
const port = Number(process.env.E2E_PORT || 4173);

const tempRoot = await fs.mkdtemp(path.join(os.tmpdir(), 'easydocs-e2e-'));
const contentRoot = path.join(tempRoot, 'Content');
const outputRoot = path.join(tempRoot, 'WebSite');
const configPath = path.join(tempRoot, 'webinfo.json');

await fs.cp(path.join(fixtureRoot, 'Content'), contentRoot, { recursive: true });

const fixtureConfig = JSON.parse(
  await fs.readFile(path.join(fixtureRoot, 'webinfo.json'), 'utf8')
);
fixtureConfig.ContetPath = contentRoot;
fixtureConfig.OutputPath = outputRoot;
fixtureConfig.BaseHref = baseHref;
await fs.writeFile(configPath, `${JSON.stringify(fixtureConfig, null, 2)}\n`, 'utf8');

const cli = await resolveCli();
await runCli(cli, configPath);

const server = createServer(async (request, response) => {
  try {
    const requestUrl = new URL(request.url || '/', `http://${request.headers.host}`);
    const relativeUrl = getRelativeUrl(requestUrl.pathname);
    const filePath = resolveContainedPath(outputRoot, relativeUrl);
    const stat = await fs.stat(filePath);
    const servedPath = stat.isDirectory() ? path.join(filePath, 'index.html') : filePath;
    const content = await fs.readFile(servedPath);

    response.writeHead(200, {
      'Content-Type': contentType(servedPath),
      'Cache-Control': 'no-store'
    });
    response.end(content);
  } catch (error) {
    const status = error?.code === 'ENOENT' ? 404 : 400;
    response.writeHead(status, { 'Content-Type': 'text/plain; charset=utf-8' });
    response.end(status === 404 ? 'Not Found' : 'Bad Request');
  }
});

const cleanup = async () => {
  server.close();
  await fs.rm(tempRoot, { recursive: true, force: true });
};

process.once('SIGINT', async () => {
  await cleanup();
  process.exit(0);
});
process.once('SIGTERM', async () => {
  await cleanup();
  process.exit(0);
});
process.once('exit', () => {
  // The parent process normally terminates via SIGTERM, which performs async cleanup.
  // This handler intentionally does not attempt filesystem work during synchronous exit.
});

server.listen(port, '127.0.0.1', () => {
  console.log(`EasyDocs E2E server listening on http://127.0.0.1:${port}${baseHref}`);
});

async function resolveCli() {
  const configuredDll = process.env.E2E_CLI_DLL?.trim();
  const releaseDll = configuredDll || path.join(
    repoRoot,
    'src',
    'BuildSite',
    'bin',
    'Release',
    'net10.0',
    'BuildSite.dll'
  );

  try {
    await fs.access(releaseDll);
    return { command: 'dotnet', args: [releaseDll] };
  } catch {
    throw new Error(
      `Release CLI not found at ${releaseDll}. Build src/BuildSite in Release before running E2E, or set E2E_CLI_DLL.`
    );
  }
}

function runCli(cli, config) {
  return new Promise((resolve, reject) => {
    const child = spawn(cli.command, [...cli.args, 'build', config], {
      cwd: repoRoot,
      stdio: ['ignore', 'pipe', 'pipe'],
      windowsHide: true
    });
    let stdout = '';
    let stderr = '';
    child.stdout.on('data', (chunk) => { stdout += chunk; });
    child.stderr.on('data', (chunk) => { stderr += chunk; });
    child.on('error', reject);
    child.on('close', (code) => {
      if (code === 0) {
        process.stdout.write(stdout);
        resolve();
        return;
      }
      reject(new Error(`Release CLI failed with exit code ${code}.\n${stdout}\n${stderr}`));
    });
  });
}

function normalizeBaseHref(value) {
  const withLeadingSlash = value.startsWith('/') ? value : `/${value}`;
  return withLeadingSlash.endsWith('/') ? withLeadingSlash : `${withLeadingSlash}/`;
}

function getRelativeUrl(pathname) {
  if (pathname === baseHref.slice(0, -1)) {
    return 'index.html';
  }
  if (!pathname.startsWith(baseHref)) {
    throw new Error('Request is outside BaseHref');
  }
  return decodeURIComponent(pathname.slice(baseHref.length)) || 'index.html';
}

function resolveContainedPath(root, relativeUrl) {
  const normalized = relativeUrl.replaceAll('/', path.sep);
  const candidate = path.resolve(root, normalized);
  const relative = path.relative(root, candidate);
  if (relative.startsWith('..') || path.isAbsolute(relative)) {
    throw new Error('Path traversal rejected');
  }
  return candidate;
}

function contentType(filePath) {
  const types = {
    '.css': 'text/css; charset=utf-8',
    '.gif': 'image/gif',
    '.html': 'text/html; charset=utf-8',
    '.ico': 'image/x-icon',
    '.jpeg': 'image/jpeg',
    '.jpg': 'image/jpeg',
    '.js': 'text/javascript; charset=utf-8',
    '.json': 'application/json; charset=utf-8',
    '.png': 'image/png',
    '.svg': 'image/svg+xml',
    '.txt': 'text/plain; charset=utf-8',
    '.webp': 'image/webp'
  };
  return types[path.extname(filePath).toLowerCase()] || 'application/octet-stream';
}
