import { StockItem } from '../models/models';

/**
 * Parses an iPhone-style model name into a (generation, tier) pair so stock
 * can be sorted newest-first, Pro Max > Pro > Estándar within each generation.
 * Anything that doesn't look like "iPhone <n>" (gen = -1) sinks to the bottom,
 * sorted alphabetically among itself, so non-iPhone stock never breaks the list.
 */
function parseModel(modelName: string): { gen: number; tier: number } {
  const name = (modelName || '').toLowerCase();

  const numMatch = name.match(/iphone\s*(\d{1,2})/);
  let gen: number;
  if (numMatch) {
    gen = parseInt(numMatch[1], 10);
  } else if (/\bxs\b|\bxr\b|\bx\b/.test(name)) {
    gen = 10;
  } else if (/\bse\b/.test(name)) {
    gen = 1;
  } else {
    gen = -1;
  }

  const tier = /pro\s*max/.test(name) ? 2 : /\bpro\b/.test(name) ? 1 : 0;

  return { gen, tier };
}

/** Newest generation first, then Pro Max > Pro > Estándar, then more storage first, then color. */
export function compareStockByModel(a: StockItem, b: StockItem): number {
  const pa = parseModel(a.modelName);
  const pb = parseModel(b.modelName);

  if (pa.gen !== pb.gen) {
    if (pa.gen === -1) return 1;
    if (pb.gen === -1) return -1;
    return pb.gen - pa.gen;
  }
  if (pa.tier !== pb.tier) return pb.tier - pa.tier;

  const storageDiff = (b.storageGb ?? 0) - (a.storageGb ?? 0);
  if (storageDiff !== 0) return storageDiff;

  return (a.color ?? '').localeCompare(b.color ?? '', 'es');
}

export function sortStockByModel(items: StockItem[]): StockItem[] {
  return [...items].sort(compareStockByModel);
}
