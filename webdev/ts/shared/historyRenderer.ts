export interface HistoryRendererData {
  created: Date;
  id: string;
  modified: Date;
  size: number;
  percentSolved: number;

  renderGrid(canvas: HTMLCanvasElement): void;
  startGameAsync(): Promise<void>;
}

export interface HistoryRendererOptions {
  filledColor?: string;
  unfilledColor?: string;
  borderColor?: string;
  textColor?: string;
}

export class HistoryRenderer {
  private options: HistoryRendererOptions;

  constructor(
    private $: JQueryStatic,
    private historyDiv: JQuery<HTMLElement>,
    private getHistoryData: () => Promise<HistoryRendererData[]>,
    options?: HistoryRendererOptions
  ) {
    this.options = options || {};
  }

  private rendereredIds = new Set<string>();

  public async renderHistoryAsync(currentGameId: string): Promise<void> {
    const historyData = await this.getHistoryData();
    const removedIds = this.computeRemovedIds(historyData);

    // Build a map of existing entries to avoid repeated linear DOM searches
    const existingEntries = new Map<string, JQuery<HTMLElement>>();
    this.historyDiv.find('[data-history-id]').each((_i, el) => {
      const $el = this.$(el) as JQuery<HTMLElement>;
      const id = $el.attr('data-history-id');
      if (id) existingEntries.set(id, $el);
    });

    // Remove old entries and currentGameId (needs to be updated)
    removedIds.add(currentGameId);
    for (const id of removedIds) {
      const $entry = existingEntries.get(id);
      $entry?.remove();
      existingEntries.delete(id);
    }

    // Sort by descending modified date
    const sorted = historyData.slice().sort((a, b) => {
      const ma = a.modified.getTime();
      const mb = b.modified.getTime();
      return mb - ma;
    });

    for (const data of sorted) {
      let $entry = existingEntries.get(data.id);
      if (!$entry || $entry.length === 0) {
        $entry = this.$('<div>')
          .attr('data-history-id', data.id)
          .addClass('history-entry') as JQuery<HTMLElement>;
        existingEntries.set(data.id, $entry);

        const $canvas = this.$('<canvas>')
          .attr({ width: 100, height: 100 })
          .addClass('history-canvas') as JQuery<HTMLCanvasElement>;
        $entry.append($canvas);

        const $info = this.$('<div>').addClass('history-info');
        const $created = this.$('<div>').text(
          `Created: ${formatDate(data.created)}`
        );
        const $modified = this.$('<div>').text(
          `Modified: ${formatDate(data.modified)}`
        );
        const $size = this.$('<div>').text(`Size: ${data.size}`);

        $info.append($created, $modified, $size);
        $entry.append($info);

        // Bind start action
        $canvas.on('click', async () => {
          await data.startGameAsync();
        });

        data.renderGrid($canvas[0]);
        drawPercentOverlay($canvas[0], data.percentSolved, this.options);
        this.historyDiv.append($entry);
      } else {
        // Ensure correct order by appending existing element
        this.historyDiv.append($entry);
      }
    }
  }

  private computeRemovedIds(historyData: HistoryRendererData[]) {
    const removedIds = new Set(this.rendereredIds);
    for (const data of historyData) {
      removedIds.delete(data.id);
    }

    this.rendereredIds.clear();
    for (const data of historyData) {
      this.rendereredIds.add(data.id);
    }
    return removedIds;
  }
}

function formatDate(date: Date): string {
  const d = new Date(date);
  const now = new Date();

  const isTodayInCurrentLocale =
    d.getFullYear() === now.getFullYear() &&
    d.getMonth() === now.getMonth() &&
    d.getDate() === now.getDate();

  if (isTodayInCurrentLocale) {
    return d.toLocaleTimeString();
  }

  return d.toLocaleDateString();
}

function drawPercentOverlay(
  canvas: HTMLCanvasElement,
  percent: number,
  options?: HistoryRendererOptions
): void {
  const ctx = canvas.getContext('2d');
  if (!ctx) return;

  const p = Math.max(0, Math.min(100, percent));
  const w = canvas.width;
  const h = canvas.height;
  const cx = w / 2;
  const cy = h / 2;
  const radius = Math.min(w, h) / 2 - 4;
  const start = -Math.PI / 2;
  const end = start + (p / 100) * 2 * Math.PI;

  const filledColor = options?.filledColor || 'rgba(83, 168, 147, 0.6)';
  const unfilledColor = options?.unfilledColor || 'rgba(0,0,0,0.1)';
  const borderColor = options?.borderColor || 'rgba(0,0,0,0.1)';
  const textColor = options?.textColor || 'rgba(255,255,255,0.8)';

  ctx.save();

  // Unfilled portion
  ctx.beginPath();
  ctx.moveTo(cx, cy);
  ctx.arc(cx, cy, radius, end, start + 2 * Math.PI);
  ctx.closePath();
  ctx.fillStyle = unfilledColor;
  ctx.fill();

  // Filled portion
  if (p > 0) {
    ctx.beginPath();
    ctx.moveTo(cx, cy);
    ctx.arc(cx, cy, radius, start, end);
    ctx.closePath();
    ctx.fillStyle = filledColor;
    ctx.fill();
  }

  // Circle border
  ctx.beginPath();
  ctx.arc(cx, cy, radius, 0, 2 * Math.PI);
  ctx.lineWidth = 1.5;
  ctx.strokeStyle = borderColor;
  ctx.stroke();

  ctx.fillStyle = textColor;
  const fontSize = 9;
  ctx.font = `${fontSize}pt sans-serif`;
  ctx.textAlign = 'center';
  ctx.textBaseline = 'middle';
  ctx.fillText(`${Math.round(p)}%`, cx, cy);

  ctx.restore();
}
