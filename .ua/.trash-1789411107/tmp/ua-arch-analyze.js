#!/usr/bin/env node
'use strict';
const fs = require('fs');
const path = require('path');

function fail(msg) {
  console.error('ERROR: ' + msg);
  process.exit(1);
}

const inputPath = process.argv[2];
const outputPath = process.argv[3];
if (!inputPath || !outputPath) fail('usage: node ua-arch-analyze.js <input.json> <output.json>');

let raw;
try {
  raw = JSON.parse(fs.readFileSync(inputPath, 'utf8'));
} catch (e) {
  fail('failed to read/parse input: ' + e.message);
}

const fileNodes = raw.fileNodes || [];
const importEdges = raw.importEdges || [];
const allEdges = raw.allEdges || [];

const nodeById = new Map();
for (const n of fileNodes) nodeById.set(n.id, n);

// ---------- A. Directory grouping ----------
function dirOf(p) {
  const idx = p.lastIndexOf('/');
  return idx === -1 ? '' : p.substring(0, idx);
}

const filePaths = fileNodes.map(n => n.filePath || n.name || '').filter(Boolean);

function commonPrefix(paths) {
  if (paths.length === 0) return '';
  const split = paths.map(p => p.split('/'));
  const minLen = Math.min(...split.map(s => s.length));
  const prefix = [];
  for (let i = 0; i < minLen - 1; i++) { // -1 to leave at least filename
    const seg = split[0][i];
    if (split.every(s => s[i] === seg)) prefix.push(seg);
    else break;
  }
  return prefix.length ? prefix.join('/') + '/' : '';
}

const prefix = commonPrefix(filePaths);

function groupForPath(p) {
  let rest = p.startsWith(prefix) ? p.substring(prefix.length) : p;
  const segs = rest.split('/');
  if (segs.length > 1) return segs[0];
  // flat: no subdirectory after prefix -> group by file/extension pattern
  const base = segs[0];
  if (/\.(test|spec)\./.test(base) || /^test_/.test(base) || /_test\.go$/.test(base) || /Test\.java$/.test(base)) return 'test';
  if (/\.config\./.test(base) || /^(tsconfig|package|vite\.config|tailwind\.config)/.test(base)) return 'config';
  const extMatch = base.match(/\.([a-zA-Z0-9]+)$/);
  return extMatch ? extMatch[1] : 'root';
}

const directoryGroups = {};
for (const n of fileNodes) {
  const p = n.filePath || n.name || '';
  const g = groupForPath(p);
  if (!directoryGroups[g]) directoryGroups[g] = [];
  directoryGroups[g].push(n.id);
}

// ---------- B. Node type grouping ----------
const nodeTypeGroups = {};
for (const n of fileNodes) {
  if (!nodeTypeGroups[n.type]) nodeTypeGroups[n.type] = [];
  nodeTypeGroups[n.type].push(n.id);
}

// ---------- C. Import adjacency ----------
const fanOut = {};
const fanIn = {};
const importsFrom = new Map(); // id -> set of target ids
for (const e of importEdges) {
  if (!importsFrom.has(e.source)) importsFrom.set(e.source, new Set());
  importsFrom.get(e.source).add(e.target);
  fanOut[e.source] = (fanOut[e.source] || 0) + 1;
  fanIn[e.target] = (fanIn[e.target] || 0) + 1;
}

function groupOf(id) {
  for (const [g, ids] of Object.entries(directoryGroups)) {
    if (ids.includes(id)) return g;
  }
  return null;
}
const idToGroup = new Map();
for (const [g, ids] of Object.entries(directoryGroups)) for (const id of ids) idToGroup.set(id, g);

// ---------- D. Cross-category dependency analysis ----------
const crossCategoryMap = new Map(); // key fromType|toType|edgeType -> count
for (const e of allEdges) {
  const s = nodeById.get(e.source), t = nodeById.get(e.target);
  if (!s || !t) continue;
  if (s.type === t.type) continue; // cross-category only
  const key = `${s.type}|${t.type}|${e.type}`;
  crossCategoryMap.set(key, (crossCategoryMap.get(key) || 0) + 1);
}
const crossCategoryEdges = [];
for (const [key, count] of crossCategoryMap.entries()) {
  const [fromType, toType, edgeType] = key.split('|');
  crossCategoryEdges.push({ fromType, toType, edgeType, count });
}

// ---------- E. Inter-group import frequency ----------
const interGroupMap = new Map();
for (const e of importEdges) {
  const g1 = idToGroup.get(e.source), g2 = idToGroup.get(e.target);
  if (!g1 || !g2 || g1 === g2) continue;
  const key = `${g1}|${g2}`;
  interGroupMap.set(key, (interGroupMap.get(key) || 0) + 1);
}
const interGroupImports = [];
for (const [key, count] of interGroupMap.entries()) {
  const [from, to] = key.split('|');
  interGroupImports.push({ from, to, count });
}

// ---------- F. Intra-group import density ----------
const intraGroupDensity = {};
for (const g of Object.keys(directoryGroups)) {
  let internalEdges = 0;
  let totalEdges = 0;
  for (const e of importEdges) {
    const g1 = idToGroup.get(e.source), g2 = idToGroup.get(e.target);
    if (g1 === g || g2 === g) {
      totalEdges++;
      if (g1 === g && g2 === g) internalEdges++;
    }
  }
  intraGroupDensity[g] = {
    internalEdges,
    totalEdges,
    density: totalEdges > 0 ? Number((internalEdges / totalEdges).toFixed(3)) : 0
  };
}

// ---------- G. Directory pattern matching ----------
const dirPatterns = [
  [['routes', 'api', 'controllers', 'endpoints', 'handlers', 'controller', 'routers', 'blueprints'], 'api'],
  [['services', 'core', 'lib', 'domain', 'logic', 'composables', 'mailers', 'jobs', 'channels', 'signals', 'internal'], 'service'],
  [['models', 'db', 'data', 'persistence', 'repository', 'entities', 'migrations', 'entity', 'sql', 'database', 'schema'], 'data'],
  [['components', 'views', 'pages', 'ui', 'layouts', 'screens'], 'ui'],
  [['middleware', 'plugins', 'interceptors', 'guards'], 'middleware'],
  [['utils', 'helpers', 'common', 'shared', 'tools', 'templatetags', 'pkg'], 'utility'],
  [['config', 'constants', 'env', 'settings', 'management', 'commands'], 'config'],
  [['__tests__', 'test', 'tests', 'spec', 'specs'], 'test'],
  [['types', 'interfaces', 'schemas', 'contracts', 'dtos', 'dto', 'request', 'response'], 'types'],
  [['hooks'], 'hooks'],
  [['store', 'state', 'reducers', 'actions', 'slices'], 'state'],
  [['assets', 'static', 'public'], 'assets'],
  [['cmd'], 'entry'],
  [['bin'], 'entry'],
  [['docs', 'documentation', 'wiki'], 'documentation'],
  [['deploy', 'deployment', 'infra', 'infrastructure'], 'infrastructure'],
  [['.github', '.gitlab', '.circleci'], 'ci-cd'],
  [['k8s', 'kubernetes', 'helm', 'charts'], 'infrastructure'],
  [['terraform', 'tf'], 'infrastructure'],
  [['docker'], 'infrastructure'],
  [['serializers'], 'api']
];

function matchPattern(dirName) {
  const lower = dirName.toLowerCase();
  for (const [names, label] of dirPatterns) {
    if (names.includes(lower)) return label;
  }
  return null;
}

const patternMatches = {};
for (const g of Object.keys(directoryGroups)) {
  const m = matchPattern(g);
  if (m) patternMatches[g] = m;
}

// File-level pattern overrides (informational; not altering directoryGroups)
function fileLevelPattern(n) {
  const p = n.filePath || '';
  const base = n.name || path.basename(p);
  if (/\.(test|spec)\./.test(base) || /^test_/.test(base) || /_test\.go$/.test(base) || /Test\.java$/.test(base) || /_spec\.rb$/.test(base) || /Test\.php$/.test(base) || /Tests\.cs$/.test(base)) return 'test';
  if (/\.d\.ts$/.test(base)) return 'types';
  if (base === 'index.ts' || base === 'index.js' || base === '__init__.py') return 'entry';
  if (base === 'manage.py') return 'entry';
  if (base === 'wsgi.py' || base === 'asgi.py') return 'config';
  if (base === 'main.go' && /cmd\//.test(p)) return 'entry';
  if ((base === 'main.rs' || base === 'lib.rs') && /^src\//.test(p)) return 'entry';
  if (base === 'Application.java' || base === 'Program.cs') return 'entry';
  if (base === 'config.ru') return 'entry';
  if (['Cargo.toml', 'go.mod', 'Gemfile', 'pom.xml', 'build.gradle', 'composer.json'].includes(base)) return 'config';
  if (/^Dockerfile/.test(base) || /^docker-compose\./.test(base)) return 'infrastructure';
  if (/\.tf$/.test(base) || /\.tfvars$/.test(base)) return 'infrastructure';
  if (/^\.github\/workflows\//.test(p) || base === '.gitlab-ci.yml' || base === 'Jenkinsfile') return 'ci-cd';
  if (/\.sql$/.test(base)) return 'data';
  if (/\.(graphql|gql|proto)$/.test(base)) return 'types';
  if (/\.(md|rst)$/.test(base)) return 'documentation';
  if (base === 'Makefile') return 'infrastructure';
  return null;
}

const fileLevelPatterns = {};
for (const n of fileNodes) {
  const m = fileLevelPattern(n);
  if (m) fileLevelPatterns[n.id] = m;
}

// ---------- H. Deployment topology ----------
const infraFiles = [];
let hasDockerfile = false, hasCompose = false, hasK8s = false, hasTerraform = false, hasCI = false;
for (const n of fileNodes) {
  const p = n.filePath || '';
  const base = n.name || path.basename(p);
  if (/^Dockerfile/.test(base)) { hasDockerfile = true; infraFiles.push(p); }
  else if (/^docker-compose\./.test(base)) { hasCompose = true; infraFiles.push(p); }
  else if (/\.ya?ml$/.test(base) && /k8s|kubernetes|helm/i.test(p)) { hasK8s = true; infraFiles.push(p); }
  else if (/\.tf$/.test(base) || /\.tfvars$/.test(base)) { hasTerraform = true; infraFiles.push(p); }
  else if (/^\.github\/workflows\//.test(p) || base === '.gitlab-ci.yml' || base === 'Jenkinsfile') { hasCI = true; infraFiles.push(p); }
}

// ---------- I. Data pipeline detection ----------
const schemaFiles = [];
const migrationFiles = [];
const dataModelFiles = [];
const apiHandlerFiles = [];
for (const n of fileNodes) {
  const p = n.filePath || '';
  const g = idToGroup.get(n.id);
  if (/\.sql$/.test(p) || /\.(graphql|gql|proto)$/.test(p)) schemaFiles.push(p);
  if (/migrations?\//i.test(p)) migrationFiles.push(p);
  if (patternMatches[g] === 'data') dataModelFiles.push(p);
  if (patternMatches[g] === 'api') apiHandlerFiles.push(p);
}

// ---------- J. Documentation coverage ----------
const docFilePaths = fileNodes.filter(n => n.type === 'document').map(n => n.filePath || '');
const groupsWithDocsSet = new Set();
for (const g of Object.keys(directoryGroups)) {
  const hasReadme = docFilePaths.some(d => dirOf(d) === g || (dirOf(d) === '' && g === 'root'));
  const hasRefDoc = docFilePaths.some(d => d.toLowerCase().includes(g.toLowerCase()));
  if (hasReadme || hasRefDoc) groupsWithDocsSet.add(g);
}
const totalGroups = Object.keys(directoryGroups).length;
const groupsWithDocs = groupsWithDocsSet.size;
const undocumentedGroups = Object.keys(directoryGroups).filter(g => !groupsWithDocsSet.has(g));

// ---------- K. Dependency direction ----------
const dependencyDirection = [];
const seenPairs = new Set();
for (const { from, to, count } of interGroupImports) {
  const pairKey = [from, to].sort().join('|');
  if (seenPairs.has(pairKey)) continue;
  seenPairs.add(pairKey);
  const reverse = interGroupImports.find(x => x.from === to && x.to === from);
  const reverseCount = reverse ? reverse.count : 0;
  if (count > reverseCount) dependencyDirection.push({ dependent: from, dependsOn: to });
  else if (reverseCount > count) dependencyDirection.push({ dependent: to, dependsOn: from });
}

// ---------- fileStats ----------
const filesPerGroup = {};
for (const [g, ids] of Object.entries(directoryGroups)) filesPerGroup[g] = ids.length;
const nodeTypeCounts = {};
for (const [t, ids] of Object.entries(nodeTypeGroups)) nodeTypeCounts[t] = ids.length;

const result = {
  scriptCompleted: true,
  commonPrefix: prefix,
  directoryGroups,
  nodeTypeGroups,
  crossCategoryEdges,
  interGroupImports,
  intraGroupDensity,
  patternMatches,
  fileLevelPatterns,
  deploymentTopology: {
    hasDockerfile, hasCompose, hasK8s, hasTerraform, hasCI,
    infraFiles: [...new Set(infraFiles)]
  },
  dataPipeline: {
    schemaFiles: [...new Set(schemaFiles)],
    migrationFiles: [...new Set(migrationFiles)],
    dataModelFiles: [...new Set(dataModelFiles)],
    apiHandlerFiles: [...new Set(apiHandlerFiles)]
  },
  docCoverage: {
    groupsWithDocs,
    totalGroups,
    coverageRatio: totalGroups > 0 ? Number((groupsWithDocs / totalGroups).toFixed(3)) : 0,
    undocumentedGroups
  },
  dependencyDirection,
  fileStats: {
    totalFileNodes: fileNodes.length,
    filesPerGroup,
    nodeTypeCounts
  },
  fileFanIn: fanIn,
  fileFanOut: fanOut
};

try {
  fs.writeFileSync(outputPath, JSON.stringify(result, null, 2));
} catch (e) {
  fail('failed to write output: ' + e.message);
}

console.log('OK: wrote ' + outputPath);
process.exit(0);
