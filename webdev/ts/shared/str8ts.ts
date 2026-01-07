// SPDX-FileCopyrightText: 2020 Luis Walter, 2025-2026 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

// module imports
import * as Str8ts from './game.js';
import * as Renderer from './gameRenderer.js';
import * as api from './str8ts-api.js';
import * as Popup from './popup.js';

// module member imports
import { UndoStack } from './undoStack.js';
import { NumberInput } from './numberInput.js';
import { GameHistory, type GameState } from './gameHistory.js';
import * as HistoryRendererModule from './historyRenderer.js';
import * as CheckerboardModule from './checkerboard.js';
import { decodeGridFromBase64Url } from './encoder.js';

// type imports
import type { ApiResult } from './str8ts-api.js';

// JSON data returned by the generateHint function
type HintData = {
  x: number;
  y: number;
  number: number;
  rule: string;
  direction: 'horizontal' | 'vertical';
};

// Global Constants

// Lightweight interface that describes the jQuery-like surface used by
// UIController. We intentionally keep this small and focused so the
// UIController can be constructed with a thin mock in tests.
interface JQuerySelection {
  empty(): unknown;
  on(event: string, handler?: any): this;
  css(props: any): this;
  css(key: string, val: any): this;
  text(val?: any): this | string;
  html(val?: any): this;
  append(val: any): this;
  addClass(val: string): this;
  removeClass(val?: string): this;
  show(): this;
  hide(): this;
  val(val?: any): any;
  attr(key: string, val?: any): any;
  prop(key: string, val?: any): any;
  removeAttr(key: string): this;
  position(): { left: number; top: number } | undefined;
  outerHeight(): number | null;
  outerWidth(): number | null;
  height(): number | undefined;
  width(): number | undefined;
  not(selector: string): JQuerySelection;

  // slick carousel methods
  slick(options: any, arg1?: any): any;
  on(
    event: 'afterChange',
    callback: (event: Event, slick: any, currentSlide: number) => void
  ): void;
}

interface JQueryLike {
  (selector: any, ...args: any[]): JQuerySelection;
}

const GridLayoutOptions = [
  { id: 'PointSymmetric', caption: 'Point Symmetric', apiValue: 7 },
  { id: 'DiagonallySymmetric', caption: 'Diagonally Symmetric', apiValue: 3 },
  {
    id: 'HorizontallySymmetric',
    caption: 'Horizontally Symmetric',
    apiValue: 4,
  },
  { id: 'VerticallySymmetric', caption: 'Vertically Symmetric', apiValue: 5 },
  {
    id: 'HorizontallyAndVerticallySymmetric',
    caption: 'Horiz. & Vert. Symmetric',
    apiValue: 6,
  },
  { id: 'Random', caption: 'Random', apiValue: 0 },
  { id: 'Uniform', caption: 'Uniform', apiValue: 1 },
];

const dialogs = {
  NEW_GAME: 1,
  GENERATING_NEW_GAME: 3,
  SOLUTION: 4,
  RESTART: 5,
  ABOUT: 6,
  HINT: 7,
  HISTORY: 8,
};
const MIN_GRID_SIZE = 4;
const MAX_GRID_SIZE = 12;
const DEFAULT_GRID_SIZE = 9;
const DEFAULT_DIFFICULTY = 3;

// We wrap the UI behavior into a single controller class to avoid leaking many globals
export class UIController {
  // state
  private starttime!: number;
  private timer!: ReturnType<typeof setTimeout>;
  private isInNoteMode = false;
  private game!: Str8ts.Game;
  private gameCode!: string;
  private gameUrl!: string | URL;
  private generateDifficulty = DEFAULT_DIFFICULTY;
  private currentGridSize = 12;
  private generateGridSize = DEFAULT_GRID_SIZE;
  private generateLayout = 'PointSymmetric';
  private undoStack!: UndoStack<Str8ts.Field>;
  private hintField: Str8ts.Field | null = null;
  private buttonColors: ButtonColors;
  private numberInput: NumberInput;
  private gameHistory: GameHistory<Str8ts.DumpedStateRead>;
  private historyRenderer: HistoryRendererModule.HistoryRenderer;

  // injected dependencies
  private $: JQueryLike;
  private win: Window;
  private renderer: Renderer.JQueryFieldRenderer;
  private setSelectedLayoutOption:
    | ((selectedOption: string) => void)
    | undefined;

  constructor($: JQueryLike, win: Window) {
    this.$ = $;
    this.win = win;
    this.undoStack = new UndoStack(this.renderUndoButton.bind(this));
    const darkMode = win.matchMedia('(prefers-color-scheme: dark)').matches;
    this.buttonColors = getButtonColors(darkMode);
    this.renderer = new Renderer.JQueryFieldRenderer(this.$ as any, darkMode);
    this.game = new Str8ts.Game(this.renderer);
    this.gameHistory = new GameHistory<Str8ts.DumpedStateRead>(localStorage);
    this.numberInput = new NumberInput(
      async (num: number) => await this.handleNumberInputAsync(num)
    );

    this.historyRenderer = new HistoryRendererModule.HistoryRenderer(
      this.$ as any,
      this.$('#history-div') as any,
      async () => await this.getHistoryRendererDataAsync()
    );
  }

  private async getHistoryRendererDataAsync(): Promise<
    HistoryRendererModule.HistoryRendererData[]
  > {
    const historyData = this.gameHistory.getAllSavedGames();
    const cbOptions = this.getHistoryCheckerboardOptions();

    const result: HistoryRendererModule.HistoryRendererData[] = historyData.map(
      (entry) => this.getHistoryRendererDataItem(entry, cbOptions)
    );

    return result;
  }

  private getHistoryCheckerboardOptions(): CheckerboardModule.CheckerboardOptions {
    const borderColor = this.$(':root').css(
      '--color-cell-border'
    ) as unknown as string;
    const cbOptions = {
      gridSizePixels: 100.0,
      borderColor: borderColor,
      trueColor: this.renderer.colors.BG_BLACK,
      falseColor: this.renderer.colors.BG_WHITEKNOWN,
    };
    return cbOptions;
  }

  private getHistoryRendererDataItem(
    entry: { key: string; data: GameState<Str8ts.DumpedStateRead> },
    cbOptions: CheckerboardModule.CheckerboardOptions
  ) {
    const modified = new Date(entry.data.timestamp);
    const historyData = (entry.data.data?.data ??
      {}) as Partial<Str8ts.HistoryData>;
    const code = entry.key;

    const created = historyData.created
      ? new Date(historyData.created)
      : undefined;
    const cb =
      historyData.size && historyData.checkerboard
        ? { checkerboard: historyData.checkerboard, size: historyData.size }
        : { checkerboard: 'HBEQQCBgQCAACAA=', size: 9 };
    return {
      id: entry.key,
      modified: modified,
      created: created,
      size: historyData.size,
      percentSolved: historyData.percentSolved,
      renderGrid: (canvas: HTMLCanvasElement) => {
        const cb1 = decodeGridFromBase64Url(cb.checkerboard, cb.size);
        CheckerboardModule.renderCheckerboard(canvas, cb1, cbOptions);
      },
      startGameAsync: async () => {
        await this.startGameCodeAsync(code);
      },
    };
  }

  // Button Functions
  private async restartAsync() {
    await this.showDialogAsync(false);
    this.game.restart();
    this.undoStack.clear();
  }

  private toggleNoteMode() {
    this.isInNoteMode = !this.isInNoteMode;
    const color = this.isInNoteMode
      ? this.buttonColors.BUTTONDOWN
      : this.buttonColors.BUTTONUP;
    this.$('#toggle-notes-mode-button').css('background-color', color);
  }

  private renderCounters() {
    this.$('#check-counter').text(this.game.check_count);
    this.$('#hint-counter').text(this.game.hint_count);
  }

  private check() {
    this.game.check();
    this.renderCounters();
    this.saveState();
  }

  private async generateAndDisplayHintAsync() {
    await this.closeHintAsync();

    const checkResult = this.game.checkForHint();
    this.renderCounters();

    if (!(checkResult.isSolved || checkResult.isWrong)) {
      await this.generateAndDisplayHintCoreAsync();
    }

    this.saveState();
  }

  private async generateAndDisplayHintCoreAsync() {
    let resp: ApiResult | undefined;
    try {
      resp = await api.generateHint(this.game.toJsonArray());
    } catch (ex) {
      console.error('Hint generation failed or unsupported:', ex);
      return;
    }

    if (resp && resp.status === 0 && resp.message) {
      const hintData = JSON.parse(resp.message) as HintData;

      this.hintField = this.game.get(hintData.y, hintData.x);
      this.hintField.setHint(hintData.number);

      // hintData.rule is either ColumnNameInPascalCase or BlockNameInPascalCase.
      const ruleWords = hintData.rule.split(/(?=[A-Z])/);
      const ruleType = ruleWords[0];
      const ruleName = ruleWords.slice(1).join(' ');
      const ruleTarget =
        ruleType == 'Block'
          ? `${hintData.direction} block`
          : hintData.direction == 'horizontal'
            ? 'row'
            : 'column';

      this.$('#hint-text').html(
        `Hint: ${hintData.number} can be removed by applying the <a href="https://github.com/m-ringler/straights/wiki/Rules-of-Str8ts#${hintData.rule}" target="rules">${ruleName} rule</a> to the ${ruleTarget}.`
      );

      const popup = this.$('#hint-dialog');
      popup.css(
        Popup.getPopupPosition(
          popup,
          this.renderer.getElement(this.hintField)[0].getBoundingClientRect(),
          this.win.document.body.getBoundingClientRect()
        )
      );
      await this.showDialogAsync(dialogs.HINT);
    } else if (resp && resp.message) {
      console.error('Failed to generate a hint:', resp.message);
    }
  }

  private async closeHintAsync() {
    if (this.hintField) {
      this.hintField.setHint(undefined);
      this.hintField = null;
      await this.showDialogAsync(false);
    }
  }

  private async showSolutionAsync() {
    await this.showDialogAsync(false);
    clearInterval(this.timer);
    this.game.showSolution();
    this.undoStack.clear();
  }

  private undo(): void {
    if (this.undoStack.length > 0 && !this.game.isSolved) {
      const field = this.undoStack.pop()!;
      const gameField = this.game.get(field.row, field.col);
      gameField.copyFrom(field);
      gameField.wrong = false;
      this.setActiveField(field.row, field.col);
    }
  }

  private setActiveField(row: number, col: number) {
    this.game.setActiveField(row, col);
  }

  private toggleNoOrAllNotes(row: number, col: number) {
    this.setActiveField(row, col);
    this.pushActiveFieldToUndoStack();
    this.game.get(row, col).toggleNoOrAllNotes();
  }

  private renderUndoButton(length: number) {
    const undoButton = this.$('#undo-button');
    if (length == 0 || this.game.isSolved) {
      undoButton.prop('disabled', true);
      undoButton.attr('disabled', 'disabled'); // Ensure attribute is present for CSS to update
    } else {
      undoButton.prop('disabled', false);
      undoButton.removeAttr('disabled'); // Ensure attribute is removed for CSS to update
    }
  }

  // General Functions
  private changeGridSize(newGridSize: number): void {
    if (newGridSize == this.currentGridSize) {
      return;
    }

    this.currentGridSize = Math.min(newGridSize, MAX_GRID_SIZE);
    this.currentGridSize = Math.max(this.currentGridSize, MIN_GRID_SIZE);
    this.showHideButtonsAndCells();
  }

  private showHideButtonsAndCells() {
    for (let i = 1; i <= this.currentGridSize; i++) {
      this.$(`td[data-button="bn${i}"]`).show();
    }
    for (let i = this.currentGridSize + 1; i <= MAX_GRID_SIZE; i++) {
      this.$(`td[data-button="bn${i}"]`).hide();
    }
    for (let r = 0; r < this.currentGridSize; r++) {
      this.$('#r' + r).show();
      for (let c = 0; c < this.currentGridSize; c++) {
        this.$(`#ce${r}_${c}`).show();
      }
      for (let c = this.currentGridSize; c < MAX_GRID_SIZE; c++) {
        this.$(`#ce${r}_${c}`).hide();
      }
    }
    for (let r = this.currentGridSize; r < MAX_GRID_SIZE; r++) {
      this.$('#r' + r).hide();
    }
  }

  private createGrid() {
    for (let r = 0; r < MAX_GRID_SIZE; r++) {
      let row = `<tr class="row" id="r${r}" data-row="${r}">`;
      for (let c = 0; c < MAX_GRID_SIZE; c++) {
        row += `<td class="cell" id="ce${r}_${c}" data-row="${r}" data-col="${c}"></td>`;
      }
      row += '</tr>';
      this.$('.container').append(row);
    }
  }

  private restartTimer() {
    this.starttime = new Date().getTime();
    this.timer = setInterval(() => {
      const diff = new Date().getTime() - this.starttime;
      const minutes = Math.floor(diff / 60000);
      const seconds = Math.floor(diff / 1000 - minutes * 60);
      this.$('#time-counter').text(
        (minutes < 10 ? '0' : '') +
          minutes +
          ':' +
          (seconds < 10 ? '0' : '') +
          seconds
      );
    }, 1000);
  }

  private getURLParameter(name: string) {
    if (!this.win.location.search) return null;
    const urlParams = new URLSearchParams(this.win.location.search);
    return urlParams.get(name);
  }

  private removeURLParameter(paramKey: string): void {
    // Get the current URL and its search part
    const url: URL = new URL(this.win.location.href);
    const searchParams: URLSearchParams = new URLSearchParams(url.search);

    // Remove the specified parameter
    searchParams.delete(paramKey);

    // Update the URL without reloading the page
    url.search = searchParams.toString();
    this.win.history.replaceState({}, '', url);
  }

  private async generateNewGameAsync() {
    await this.showDialogAsync(dialogs.GENERATING_NEW_GAME);
    clearInterval(this.timer);
    this.$('#confirm-new-game-button').prop('disabled', true);
    try {
      let layoutOption = GridLayoutOptions.find(
        (o) => o.id === this.generateLayout
      );
      if (layoutOption === undefined) {
        console.error('Invalid layout option selected:', this.generateLayout);
        layoutOption = GridLayoutOptions[0];
      }
      const data = await api.generate(
        this.generateGridSize,
        this.generateDifficulty,
        layoutOption.apiValue
      );
      if (data.status === 0 && data.message.length > Str8ts.minCodeSize) {
        const code = data.message;
        await this.startGameCodeAsync(code);
        return;
      } else {
        console.error('Error generating game:', data.message);
      }
    } catch (error) {
      console.error('Error fetching game:', error);
    }

    await this.showDialogAsync(false);
    this.$('#confirm-new-game-button').prop('disabled', false);
  }

  private async startGameCodeAsync(code: string) {
    console.log('Game:', code);
    const emojis = this.getURLParameter('emojis');
    this.gameUrl = this.win.location.href.split('?')[0] + '?code=' + code;
    if (emojis != null) {
      this.gameUrl += '&emojis=' + emojis;
    }
    this.gameCode = code;
    await this.startGameAsync(true);
  }

  private loadSettings() {
    var values = loadNewGameSettings(this.$, (key) =>
      localStorage.getItem(key)
    );

    this.generateGridSize = values.generateGridSize;
    this.generateDifficulty = values.generateDifficulty;
    this.generateLayout = values.generateLayout;
  }

  changeDifficulty() {
    this.generateDifficulty = Number(this.$('#difficulty-slider').val());
    this.$('#difficulty-text').text(this.generateDifficulty);
    localStorage.setItem(
      'generate.difficulty',
      String(this.generateDifficulty)
    );
  }

  changeGenerateSize() {
    this.generateGridSize = Number(this.$('#grid-size-slider').val());
    this.$('#grid-size-text').text(this.generateGridSize);
    localStorage.setItem('generate.gridSize', String(this.generateGridSize));
  }

  changeLayoutOption(selectedOption: string) {
    this.generateLayout = selectedOption ? selectedOption : 'PointSymmetric';
    localStorage.setItem('generate.layout', this.generateLayout);
  }

  private async startGameAsync(shouldSetLocationHref: boolean) {
    let hasGame = false;
    if (this.gameCode && this.gameCode.length > Str8ts.minCodeSize) {
      this.undoStack.clear();
      this.$('.container').removeClass('finished');
      await this.showDialogAsync(false);

      const emojiString = this.getURLParameter('emojis');
      this.renderer.setEmojis(emojiString);
      const parsedGame = this.game.parseGameCode(this.gameCode);
      if (parsedGame) {
        this.game = parsedGame;
        hasGame = true;

        this.changeGridSize(this.game.size);

        await this.restoreGameStateAsync();

        this.restartTimer();
        this.renderCounters();

        if (shouldSetLocationHref && this.gameUrl != this.win.location.href) {
          this.SetLocationHref(new URL(this.gameUrl));
        }

        this.saveState();
      }
    }

    if (!hasGame) {
      await this.generateNewGameAsync();
    }
  }

  private async restoreGameStateAsync() {
    const stateLoadedFromUrl = await this.tryLoadStateFromUrlParameterAsync();
    if (!stateLoadedFromUrl) {
      await this.gameHistory.restoreGameStateAsync(this.gameCode, this.game);
    }
  }

  private async tryLoadStateFromUrlParameterAsync() {
    const stateUrlParameter = this.getURLParameter('state');
    if (!stateUrlParameter) {
      return false;
    }

    try {
      this.removeURLParameter('state');
      await this.game.restoreStateBase64Async(stateUrlParameter);
      this.saveState();
      return true;
    } catch (ex) {
      console.error(ex);
      return false;
    }
  }

  async showNewGameDialogWithCancelButtonAsync() {
    await this.showDialogAsync(dialogs.NEW_GAME);
    this.$('#cancel-new-game-button').show();
  }

  async showDialogAsync(dialog: number | false) {
    this.$('#new-game-dialog').hide();
    this.$('#generating-new-game-dialog').hide();
    this.$('#solution-dialog').hide();
    this.$('#restart-dialog').hide();
    this.$('#about-dialog').hide();
    this.$('#hint-dialog').hide();
    this.$('#history-dialog').hide();

    if (dialog != dialogs.HINT) {
      await this.closeHintAsync();
    }

    if (dialog) {
      this.$('.dialog-outer-container').show();
      switch (dialog) {
        case dialogs.GENERATING_NEW_GAME:
          this.$('#generating-new-game-dialog').show();
          break;
        case dialogs.NEW_GAME:
          this.$('#new-game-dialog').show();
          // Force a re-render of the carousel to fix display issues
          this.$('.carousel').slick('setPosition');
          this.setSelectedLayoutOption!(this.generateLayout);
          break;
        case dialogs.SOLUTION:
          if (!this.game.isSolved) {
            this.$('#solution-dialog').show();
          } else {
            this.$('.dialog-outer-container').hide();
          }
          break;
        case dialogs.RESTART:
          if (!this.game.isSolved) {
            this.$('#restart-dialog').show();
          } else {
            this.$('.dialog-outer-container').hide();
          }
          break;
        case dialogs.ABOUT:
          const link = await this.getCurrentLinkAsync();
          this.$('#current-game-link').attr('href', link);
          this.$('#about-dialog').show();
          break;
        case dialogs.HINT:
          this.$('#hint-dialog').show();
          break;
        case dialogs.HISTORY:
          await this.updateHistoryDivAsync();
          this.$('#history-dialog').show();
          break;
      }
    } else {
      this.$('.dialog-outer-container').hide();
    }
  }

  private async updateHistoryDivAsync() {
    await this.historyRenderer.renderHistoryAsync(this.gameCode);
  }

  private SetLocationHref(url: string | URL) {
    this.win.history.pushState({}, '', url);
  }

  private async onKeyDownAsync(e: KeyboardEvent) {
    if (this.game.isSolved) return;

    let handled = false;
    const key = e.key;
    if (this.handleCursorKey(e)) {
      handled = true;
    } else if (key >= '0' && key <= '9') {
      handled = await this.handleDigitKeyAsync(Number(key));
    } else if (key == 'n') {
      this.toggleNoteMode();
      handled = true;
    } else if (key == 'h' && e.altKey) {
      this.showDialogAsync(dialogs.HISTORY);
      handled = true;
    } else if (key == 'z' && e.ctrlKey) {
      this.undo();
      handled = true;
    } else if (key === 'Backspace' || key === 'Delete') {
      this.handleDelete();
      handled = true;
    }

    if (handled) {
      e.preventDefault();
    }
  }

  private handleCursorKey(e: { which: number }) {
    switch (e.which) {
      case 37: // left
        this.game.moveSelection(-1, 0);
        break;
      case 38: // up
        this.game.moveSelection(0, -1);
        break;
      case 39: // right
        this.game.moveSelection(1, 0);
        break;
      case 40: // down
        this.game.moveSelection(0, 1);
        break;
      default:
        return false;
    }

    return true;
  }

  private async handleDigitKeyAsync(digit: number) {
    const handled = digit <= this.currentGridSize;
    if (handled) {
      await this.numberInput.handleDigitAsync(digit, this.currentGridSize);
    }

    return handled;
  }

  private async handleNumberInputAsync(num: number) {
    if (num < 1 || num > this.currentGridSize) {
      return;
    }

    const activeField = this.game.getActiveField();
    if (!activeField || !activeField.isEditable()) {
      return;
    }

    this.pushToUndoStack(activeField);

    if (this.isInNoteMode) {
      activeField.setNote(num);
    } else {
      activeField.setUser(num);
      this.game.checkSolved();

      if (this.game.isSolved) {
        this.undoStack.clear();
        this.$('.container').addClass('finished');
        await this.onResizeAsync();
        clearInterval(this.timer);
      }
    }

    this.saveState();
  }

  private pushActiveFieldToUndoStack() {
    const activeField = this.game.getActiveField();
    if (!activeField || !activeField.isEditable()) {
      return;
    }

    this.pushToUndoStack(activeField);
  }

  private pushToUndoStack(activeField: Str8ts.Field) {
    this.undoStack.push(activeField.copy());
  }

  private saveState() {
    this.gameHistory.saveGameState(this.gameCode, this.game);
  }

  private handleDelete() {
    const field = this.game.getActiveField();
    if (!field || !field.isEditable()) {
      return;
    }

    this.undoStack.push(field.copy());
    field.clear();
  }

  private async getCurrentLinkAsync() {
    let link = this.win.location.href;
    if (this.game) {
      const stateBase64 = await this.game.dumpStateBase64Async();
      link += `&state=${stateBase64}`;
    }
    return link;
  }

  async copyCurrentLinkAsync() {
    try {
      const link = await this.getCurrentLinkAsync();
      await this.win.navigator.clipboard.writeText(link);
      const copyBtn = this.$('#copy-link-button');
      const originalText = copyBtn.text();
      copyBtn.text('Link copied!');
      setTimeout(() => copyBtn.text(originalText), 1000);
    } catch (err) {
      console.error('Failed to copy:', err);
    }
  }

  private async handleGameLoadAsync() {
    const code = this.getURLParameter('code');
    const currentKey = this.win.location.href;

    if (code && code.length > Str8ts.minCodeSize) {
      this.gameUrl = currentKey;
      this.gameCode = code;

      await this.startGameAsync(false);
    } else {
      let latestKey = this.gameHistory.getLatestGameKey();
      if (latestKey) {
        await this.startGameCodeAsync(latestKey);
      } else {
        await this.generateNewGameAsync();
      }
    }
  }

  private async onResizeAsync() {
    await this.closeHintAsync();
    if (
      this.win.innerWidth / 2 - 45 <
      (this.$('.controls').position()?.left ?? 0)
    ) {
      // Large screen
      this.$('#buttons-small').hide();
      this.$('#buttons-large').show();
      this.$('.cell').css({
        'font-size': '22pt',
        width: '41px',
        height: '41px',
      });
      this.$('.mini').css('font-size', '9pt');
      this.$('#hint-dialog').css('width', '235px');
    } else {
      // Small screen
      const cellwidth = Math.min(
        Math.floor(this.win.innerWidth / this.currentGridSize - 2),
        41
      );
      this.$('#buttons-small').show();
      this.$('#buttons-large').hide();
      this.$('.container').css({ margin: '5px 2px' });
      this.$('.controls').css({ margin: '0px 2px' });
      this.$('.cell').css({
        'font-size': '17pt',
        width: `${cellwidth}px`,
        height: `${cellwidth}px`,
      });
      this.$('.mini').css('font-size', '8pt');
      this.$('#hint-dialog').css('width', '150px');
    }
  }

  // single public startup entry point for DOM initialisation
  async startAsync() {
    // initial UI setup
    this.createGrid();
    await this.onResizeAsync();

    this.renderLayoutCarousel();
    this.loadSettings();
    await this.handleGameLoadAsync();

    // event handlers for UI elements
    const gridCells = this.$('td[id^="ce"]');
    gridCells.on('click', (evt: Event) => {
      const { row, col } = this.getRowAndColumnOfTargetCell(evt);
      this.setActiveField(row, col);
    });
    gridCells.on('dblclick', (evt: Event) => {
      const { row, col } = this.getRowAndColumnOfTargetCell(evt);
      this.toggleNoOrAllNotes(row, col);
    });

    const numberButtons = this.$('td[data-button^="bn"]');
    numberButtons.on('click', async (evt: Event) => {
      const el = evt.currentTarget as Element;
      const num = Number(this.$(el).text());
      await this.handleNumberInputAsync(num);
    });

    // wire page-level events here so they can call private methods
    this.win.addEventListener('popstate', async () => {
      await this.handleGameLoadAsync();
    });
    this.$(document).on('keydown', async (e: KeyboardEvent) => {
      await this.onKeyDownAsync(e);
    });
    this.$(this.win).on('resize', async () => {
      await this.onResizeAsync();
    });

    // Controls wired from index.html
    this.$('#undo-button').on('click', () => this.undo());
    this.$('#toggle-notes-mode-button').on('click', () =>
      this.toggleNoteMode()
    );
    this.$('#check-button').on('click', () => this.check());
    this.$('#show-hint-button').on(
      'click',
      async () => await this.generateAndDisplayHintAsync()
    );
    this.$('#show-solution-button').on(
      'click',
      async () => await this.showDialogAsync(dialogs.SOLUTION)
    );

    this.$('#show-new-game-dialog-button').on(
      'click',
      async () => await this.showNewGameDialogWithCancelButtonAsync()
    );
    this.$('#show-restart-dialog-button').on(
      'click',
      async () => await this.showDialogAsync(dialogs.RESTART)
    );
    this.$('#show-about-dialog-button').on(
      'click',
      async () => await this.showDialogAsync(dialogs.ABOUT)
    );
    this.$('#show-history-dialog-button').on(
      'click',
      async () => await this.showDialogAsync(dialogs.HISTORY)
    );

    this.$('#grid-size-slider').on('input', () => this.changeGenerateSize());
    this.$('#difficulty-slider').on('input', () => this.changeDifficulty());

    this.$('#confirm-new-game-button').on(
      'click',
      async () => await this.generateNewGameAsync()
    );
    this.$('#cancel-new-game-button').on(
      'click',
      async () => await this.showDialogAsync(false)
    );

    this.$('#confirm-show-solution-button').on(
      'click',
      async () => await this.showSolutionAsync()
    );
    this.$('#confirm-restart-button').on(
      'click',
      async () => await this.restartAsync()
    );

    // Copy link and force-update actions
    this.$('#copy-link-button').on(
      'click',
      async () => await this.copyCurrentLinkAsync()
    );

    // The force-update action proper is registered in a script block in index.html
    this.$('#force-update').on(
      'click',
      async () => await this.showDialogAsync(false)
    );

    // Hint dialog close handlers (close the hint on click)
    this.$('#hint-dialog').on('click', async () => await this.closeHintAsync());
    this.$('#hint-close').on('click', async (e: Event) => {
      e.stopPropagation();
      await this.closeHintAsync();
    });

    // generic close buttons for dialogs (hide overlay)
    this.$('.close-button')
      .not('#hint-close') // special handler above
      .on('click', async () => await this.showDialogAsync(false));
  }

  private getRowAndColumnOfTargetCell(evt: Event) {
    const selection = this.$(evt.currentTarget);
    const row = Number(selection.attr('data-row'));
    const col = Number(selection.attr('data-col'));
    return { row, col };
  }

  private renderLayoutCarousel() {
    const $carousel = this.$('.carousel');
    $carousel.empty(); // Clear any existing content

    GridLayoutOptions.forEach((option) => {
      $carousel.append(`
      <div class="carousel-slide" data-id="${option.id}">
        <img src="layout-img/g_${option.id}.png" alt="${option.caption}" loading="lazy">
        <div class="caption">${option.caption}</div>
      </div>
    `);
    });

    $carousel.slick({
      dots: true,
      infinite: false,
      speed: 300,
      slidesToShow: 1,
      slidesToScroll: 1,
      arrows: true,
    });

    $carousel.on(
      'afterChange',
      (event: Event, slick: any, currentSlide: number) => {
        const currentOption = GridLayoutOptions[currentSlide];
        this.changeLayoutOption(currentOption.id);
      }
    );

    this.setSelectedLayoutOption = (selectedOption: string) => {
      const index = GridLayoutOptions.findIndex(
        (option) => option.id === selectedOption
      );
      if (index !== -1) {
        $carousel.slick('slickGoTo', index);
      }
    };
  }
}

interface ButtonColors {
  BUTTONDOWN: string;
  BUTTONUP: string;
}

function getButtonColors(darkMode: boolean): ButtonColors {
  const buttonColorsLight = {
    BUTTONDOWN: '#335',
    BUTTONUP: '#b1cffc',
  };

  const buttonColorsDark = {
    BUTTONDOWN: '#e7d9cdff' /* color-button-active */,
    BUTTONUP: '#555' /* color-button-background */,
  };

  return darkMode ? buttonColorsDark : buttonColorsLight;
}

function loadNewGameSettings(
  $$: JQueryLike,
  getStoredValue: (key: string) => string | null
): {
  generateGridSize: number;
  generateDifficulty: number;
  generateLayout: string;
} {
  function loadSetting(
    sliderId: string,
    storageKey: string,
    defaultValue: number
  ) {
    const slider = $$(`#${sliderId}`);
    const storedValue = getStoredValue(storageKey);

    let validatedValue = defaultValue;
    if (storedValue !== null) {
      const value = Number(storedValue);
      const min = Number(slider.attr('min'));
      const max = Number(slider.attr('max'));

      if (value >= min && value <= max) {
        validatedValue = value;
      }
    }

    slider.val(validatedValue);
    $$(`#${sliderId.replace('-slider', '-text')}`).text(validatedValue);
    return validatedValue;
  }

  try {
    const layout = getStoredValue('generate.layout') || 'PointSymmetric';
    return {
      generateGridSize: loadSetting(
        'grid-size-slider',
        'generate.gridSize',
        DEFAULT_GRID_SIZE
      ),
      generateDifficulty: loadSetting(
        'difficulty-slider',
        'generate.difficulty',
        DEFAULT_DIFFICULTY
      ),
      generateLayout: layout,
    };
  } catch (error) {
    console.warn('Failed to load settings:', error, 'Using defaults.');
    return {
      generateGridSize: DEFAULT_GRID_SIZE,
      generateDifficulty: DEFAULT_DIFFICULTY,
      generateLayout: 'PointSymmetric',
    };
  }
}
