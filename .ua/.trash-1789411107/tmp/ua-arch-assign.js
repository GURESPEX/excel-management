const fs = require('fs');
const input = JSON.parse(fs.readFileSync(process.argv[2], 'utf8'));
const nodes = input.fileNodes;

function classify(n) {
  const idPath = n.id.replace(/^[a-z]+:/, '');
  const p = n.filePath || idPath;
  const id = n.id;

  // --- .NET API: ExcelManagement.Api (presentation) ---
  if (/^excel-management-api\/src\/ExcelManagement\.Api\//.test(p)) return 'layer:api-presentation';

  // --- .NET API: Application ---
  if (/^excel-management-api\/src\/ExcelManagement\.Application\//.test(p)) return 'layer:api-application';

  // --- .NET API: Domain ---
  if (/^excel-management-api\/src\/ExcelManagement\.Domain\//.test(p)) return 'layer:api-domain';

  // --- .NET API: Infrastructure ---
  if (/^excel-management-api\/src\/ExcelManagement\.Infrastructure\//.test(p)) return 'layer:api-infrastructure';

  // --- .NET API: McpServer ---
  if (/^excel-management-api\/src\/ExcelManagement\.McpServer\//.test(p)) return 'layer:api-mcpserver';

  // --- .NET API: tests ---
  if (/^excel-management-api\/tests\//.test(p)) return 'layer:api-tests';

  // --- .NET API: Dockerfile / dockerignore -> infra ---
  if (/^excel-management-api\/(Dockerfile|\.dockerignore)/.test(p)) return 'layer:infrastructure-devops';

  // --- .NET API: solution file + .vs IDE cache -> tooling/project-meta ---
  if (/^excel-management-api\/(ExcelManagement\.sln|\.vs\/)/.test(p)) return 'layer:project-meta';

  // --- Web frontend: Dockerfile/dockerignore -> infra ---
  if (/^excel-management-web\/(Dockerfile|\.dockerignore)/.test(p)) return 'layer:infrastructure-devops';

  // --- Web frontend: everything else under excel-management-web ---
  if (/^excel-management-web\//.test(p)) return 'layer:web-app';

  // --- infra/ (docker-compose, infra README) ---
  if (/^infra\//.test(p)) return 'layer:infrastructure-devops';

  // --- CI pipelines ---
  if (/^\.github\//.test(p)) return 'layer:infrastructure-devops';

  // --- Project docs ---
  if (/^docs\//.test(p)) return 'layer:documentation';
  if (p === 'AGENTS.md' || p === 'CLAUDE.md') return 'layer:documentation';

  // --- Tooling / agent config / issue tracker / requirements ---
  if (/^\.beads\//.test(p)) return 'layer:project-meta';
  if (/^\.agents\//.test(p)) return 'layer:project-meta';
  if (/^\.claude\//.test(p)) return 'layer:project-meta';
  if (/^\.codex\//.test(p)) return 'layer:project-meta';
  if (/^\.ua\//.test(p)) return 'layer:project-meta';
  if (/^requirements\//.test(p)) return 'layer:project-meta';

  return 'layer:UNCLASSIFIED';
}

const layers = {};
for (const n of nodes) {
  const l = classify(n);
  if (!layers[l]) layers[l] = [];
  layers[l].push(n.id);
}

for (const [l, ids] of Object.entries(layers)) {
  console.log(l, ids.length);
}
const total = nodes.length;
const sum = Object.values(layers).reduce((a, b) => a + b.length, 0);
console.log('total', total, 'sum', sum, total === sum ? 'OK' : 'MISMATCH');

if (layers['layer:UNCLASSIFIED']) {
  console.log('UNCLASSIFIED:', layers['layer:UNCLASSIFIED']);
}

fs.writeFileSync(process.argv[3], JSON.stringify(layers, null, 2));
