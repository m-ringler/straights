// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

import * as cheerio from 'cheerio';
import * as fs from 'fs';

export function extractIdsFromHtml(htmlPath: string): Set<string> {
  const html = fs.readFileSync(htmlPath, 'utf-8');
  const $ = cheerio.load(html);
  const ids = new Set<string>();
  $('[id]').each((_, el) => {
    const id = $(el).attr('id');
    if (id) ids.add(id);
  });
  return ids;
}

export function extractJQueryIdsFromTs(tsPath: string): Set<string> {
  const ts = fs.readFileSync(tsPath, 'utf-8');
  const regex = /\$\(['"]#([^'"]+)['"]\)/g;
  const ids = new Set<string>();
  let match: string[] | null;
  while ((match = regex.exec(ts)) !== null) {
    ids.add(match[1]);
  }
  return ids;
}
