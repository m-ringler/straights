export interface HistoryRendererData {
  created: Date;
  id: string;
  modified: Date;
  size: number;
  percentSolved: number;

  renderGrid(canvas: HTMLCanvasElement): void;
  startGameAsync(): Promise<void>;
}

export class HistoryRenderer {
  constructor(
    private $: JQueryStatic,
    private historyDiv: JQuery<HTMLElement>,
    private getHistoryData: () => Promise<HistoryRendererData[]>
  ) {}

  private rendereredIds = new Set<string>();

  public async renderHistoryAsync(): Promise<void> {
    const historyData = await this.getHistoryData();
    const removedIds = new Set(this.rendereredIds);
    for (const data of historyData) {
      removedIds.delete(data.id);
      this.rendereredIds.add(data.id);
    }

    // Remove old entries
    for (const id of removedIds) {
      this.historyDiv.find(`[data-history-id="${id}"]`).remove();
      this.rendereredIds.delete(id);
    }

    // Sort by descending modified date
    const sorted = historyData.slice().sort((a, b) => {
      const ma = a.modified.getTime();
      const mb = b.modified.getTime();
      return mb - ma;
    });

    // Build a map of existing entries to avoid repeated linear DOM searches
    const existingEntries = new Map<string, JQuery<HTMLElement>>();
    this.historyDiv.find('[data-history-id]').each((_i, el) => {
      const $el = this.$(el) as JQuery<HTMLElement>;
      const id = $el.attr('data-history-id');
      if (id) existingEntries.set(id, $el);
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
        const $percent = this.$('<div>').text(
          `Solved: ${data.percentSolved} %`
        );

        $info.append($created, $modified, $size, $percent);
        $entry.append($info);

        // Bind start action
        $entry.on('click', async () => {
          await data.startGameAsync();
        });

        data.renderGrid($canvas[0]);
        this.historyDiv.append($entry);
      } else {
        // Ensure correct order by appending existing element
        this.historyDiv.append($entry);
      }
    }
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
