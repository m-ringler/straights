// SPDX-FileCopyrightText: 2020 Luis Walter, 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

type Game = import('./game').Game;
type Field = import('./game').Field;
type UndoStack<T> = import('./undoStack').UndoStack<T>;
type ApiResult = import('./generate-str8ts').ApiResult;

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
}

interface JQueryLike {
  (selector: any, ...args: any[]): JQuerySelection;
}

function _getButtonColors(darkMode: boolean) {
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

const _darkMode = window.matchMedia('(prefers-color-scheme: dark)').matches;

const _buttonColors = _getButtonColors(_darkMode);

const dialogs = {
  WELCOME: 1,
  GENERATED: 2,
  LOADING: 3,
  SOLUTION: 4,
  RESTART: 5,
  ABOUT: 6,
  HINT: 7,
};
const MIN_GRID_SIZE = 4;
const MAX_GRID_SIZE = 12;
const DEFAULT_GRID_SIZE = 9;
const DEFAULT_DIFFICULTY = 3;

// We wrap the UI behavior into a single controller class to avoid leaking many globals
class UIController {
  // state
  private _starttime!: number;
  private _timer!: ReturnType<typeof setTimeout>;
  private _noteMode = false;
  private _minCodeSize!: number;
  private _game!: Game;
  private _gameCode!: string;
  private _gameHistory!: typeof import('./gameHistory.js');
  private _gameUrl!: string | URL;
  private _difficulty = DEFAULT_DIFFICULTY;
  private _currentGridSize = 12;
  private _generateGridSize = DEFAULT_GRID_SIZE;
  private _undoStack!: UndoStack<Field>;
  private _hintField: Field | null = null;

  // injected dependencies
  private $: JQueryLike;
  private win: Window;

  // imported functions (TODO: import the types from generate-str8ts.ts)
  private _generate!: (arg0: number, arg1: number) => Promise<ApiResult>;
  private _generateHint!: (arg0: number[][][]) => Promise<ApiResult>;

  // module loader promise so external code can wait
  readonly modulePromise: Promise<void>;

  constructor($: any, win: Window) {
    this.$ = $ as JQueryLike;
    this.win = win;
    this.modulePromise = this._importModules();
  }

  private async _importModules() {
    const undoStackModule = await import('./undoStack.js');
    this._undoStack = new undoStackModule.UndoStack(
      this._renderUndoButton.bind(this)
    );

    const gameModule = await import('./game.js');
    this._game = new gameModule.Game(this.$, _darkMode);
    this._minCodeSize = gameModule.minCodeSize;

    this._gameHistory = await import('./gameHistory.js');

    const generateModule = await import('./generate-str8ts.js');
    const loadedFunctions = generateModule.load_generate();
    this._generate = loadedFunctions.generate;
    this._generateHint = loadedFunctions.generateHint;
  }

  // Button Functions
  restart() {
    this.showDialog(false);
    this._game.restart();
    this._undoStack.clear();
  }

  toggleNoteMode() {
    this._noteMode = !this._noteMode;
    const color = this._noteMode
      ? _buttonColors.BUTTONDOWN
      : _buttonColors.BUTTONUP;
    this.$('#notes').css('background-color', color);
  }

  private _renderCounters() {
    this.$('#check-counter').text(this._game.check_count);
    this.$('#hint-counter').text(this._game.hint_count);
  }

  check() {
    this._game.check();
    this._renderCounters();
    this._saveState();
  }

  async hint() {
    this.closeHint();

    const checkResult = this._game.checkForHint();
    this._renderCounters();

    if (!(checkResult.isSolved || checkResult.isWrong)) {
      await this._generateAndDisplayHint();
    }

    this._saveState();
  }

  private async _generateAndDisplayHint() {
    let resp: ApiResult | undefined;
    try {
      resp = await this._generateHint(this._game.toJsonArray());
    } catch (ex) {
      console.error('Hint generation failed or unsupported:', ex);
      return;
    }

    if (resp && resp.status === 0 && resp.message) {
      const hintData = JSON.parse(resp.message) as HintData;

      this._hintField = this._game.get(hintData.y, hintData.x);
      this._hintField.setHint(hintData.number);

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
      this._positionHintDialog();
      this.showDialog(dialogs.HINT);
    } else if (resp && resp.message) {
      console.error('Failed to generate a hint:', resp.message);
    }
  }

  closeHint() {
    if (this._hintField) {
      this._hintField.setHint(undefined);
      this._hintField = null;
      this.showDialog(false);
    }
  }

  private _positionHintDialog() {
    if (!this._hintField) {
      return;
    }

    const fieldElement = this._hintField.getElement()[0];
    const dialog = this.$('#hint-dialog');
    this._positionPopup(fieldElement, dialog);
  }

  private _positionPopup(target: Element, popup: JQuerySelection) {
    const targetPos = target.getBoundingClientRect();
    const windowHeight = this.$(this.win).height();
    const windowWidth = this.$(this.win).width();

    if (!windowHeight || !windowWidth) {
      return;
    }

    // Determine the vertical position
    let popupTop;
    if (targetPos.top + targetPos.height / 2 > windowHeight / 2) {
      popupTop = targetPos.top + this.win.scrollY - (popup.outerHeight() ?? 0);
    } else {
      popupTop = targetPos.top + this.win.scrollY + targetPos.height;
    }

    // Determine the horizontal position
    let popupLeft;
    if (targetPos.left + targetPos.width / 2 > windowWidth / 2) {
      popupLeft = targetPos.left + this.win.scrollX - (popup.outerWidth() ?? 0);
    } else {
      popupLeft = targetPos.left + this.win.scrollX + targetPos.width;
    }

    // Set the position of the dialog
    popup.css({
      position: 'absolute',
      top: popupTop,
      left: popupLeft,
    });
  }

  showSolution() {
    this.showDialog(false);
    clearInterval(this._timer);
    this._game.showSolution();
    this._undoStack.clear();
  }

  undo() {
    if (this._undoStack.length > 0 && !this._game.isSolved) {
      const field = this._undoStack.pop()!;
      const gameField = this._game.get(field.row, field.col);
      gameField.copyFrom(field);
      gameField.wrong = false;
      this._game.selectCell(field.row, field.col);
      gameField.render();
    }
  }

  // exposed helper for external code that used to call _game.selectCell directly
  selectCell(row: number, col: number) {
    this._game.selectCell(row, col);
  }

  private _renderUndoButton(length: number) {
    const undoButton = this.$('#undo');
    if (length == 0 || this._game.isSolved) {
      undoButton.prop('disabled', true);
      undoButton.attr('disabled', 'disabled'); // Ensure attribute is present for CSS to update
    } else {
      undoButton.prop('disabled', false);
      undoButton.removeAttr('disabled'); // Ensure attribute is removed for CSS to update
    }
  }

  // General Functions
  private _changeGridSize(newGridSize: number): void {
    if (newGridSize == this._currentGridSize) {
      return;
    }

    this._currentGridSize = Math.min(newGridSize, MAX_GRID_SIZE);
    this._currentGridSize = Math.max(this._currentGridSize, MIN_GRID_SIZE);
    this._showHideButtonsAndCells();
  }

  private _showHideButtonsAndCells() {
    for (let i = 1; i <= this._currentGridSize; i++) {
      this.$(`td[data-button="bn${i}"]`).show();
    }
    for (let i = this._currentGridSize + 1; i <= MAX_GRID_SIZE; i++) {
      this.$(`td[data-button="bn${i}"]`).hide();
    }
    for (let r = 0; r < this._currentGridSize; r++) {
      this.$('#r' + r).show();
      for (let c = 0; c < this._currentGridSize; c++) {
        this.$(`#ce${r}_${c}`).show();
      }
      for (let c = this._currentGridSize; c < MAX_GRID_SIZE; c++) {
        this.$(`#ce${r}_${c}`).hide();
      }
    }
    for (let r = this._currentGridSize; r < MAX_GRID_SIZE; r++) {
      this.$('#r' + r).hide();
    }
  }

  private _createGrid() {
    for (let r = 0; r < MAX_GRID_SIZE; r++) {
      let row = `<tr class="row" id="r${r}" row="${r}">`;
      for (let c = 0; c < MAX_GRID_SIZE; c++) {
        row += `<td class="cell" id="ce${r}_${c}" row="${r}" col="${c}"></td>`;
      }
      row += '</tr>';
      this.$('.container').append(row);
    }
  }

  private _restartTimer() {
    this._starttime = new Date().getTime();
    this._timer = setInterval(() => {
      const diff = new Date().getTime() - this._starttime;
      const minutes = Math.floor(diff / 60000);
      const seconds = Math.floor(diff / 1000 - minutes * 60);
      this.$('#time').text(
        (minutes < 10 ? '0' : '') +
          minutes +
          ':' +
          (seconds < 10 ? '0' : '') +
          seconds
      );
    }, 1000);
  }

  private _getURLParameter(name: string) {
    if (!this.win.location.search) return null;
    const urlParams = new URLSearchParams(this.win.location.search);
    return urlParams.get(name);
  }

  private _removeURLParameter(paramKey: string): void {
    // Get the current URL and its search part
    const url: URL = new URL(this.win.location.href);
    const searchParams: URLSearchParams = new URLSearchParams(url.search);

    // Remove the specified parameter
    searchParams.delete(paramKey);

    // Update the URL without reloading the page
    url.search = searchParams.toString();
    this.win.history.replaceState({}, '', url);
  }

  async generateNewGame() {
    this.showDialog(dialogs.LOADING);
    clearInterval(this._timer);
    this.$('#generate-button').prop('disabled', true);
    try {
      const data = await this._generate(
        this._generateGridSize,
        this._difficulty
      );
      if (data.status === 0 && data.message.length > this._minCodeSize) {
        console.log('Game:', data.message);
        this._gameUrl =
          this.win.location.href.split('?')[0] + '?code=' + data.message;
        this._gameCode = data.message;
        this.showDialog(dialogs.GENERATED);
        return;
      } else {
        console.error('Error generating game:', data.message);
      }
    } catch (error) {
      console.error('Error fetching game:', error);
    }
    this.showDialog(false);
    this.$('#generate-button').prop('disabled', false);
  }

  private _loadSettings() {
    const loadSetting = (
      sliderId: string,
      storageKey: string,
      defaultValue: number
    ) => {
      const slider = this.$(`#${sliderId}`);
      const storedValue = localStorage.getItem(storageKey);

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
      this.$(`#${sliderId.replace('-slider', '')}`).text(validatedValue);
      return validatedValue;
    };

    try {
      this._generateGridSize = loadSetting(
        'grid-size-slider',
        'generate.gridSize',
        DEFAULT_GRID_SIZE
      );
      this._difficulty = loadSetting(
        'difficulty-slider',
        'generate.difficulty',
        DEFAULT_DIFFICULTY
      );
    } catch (error) {
      console.warn('Failed to load settings:', error, 'Using defaults.');
      this._generateGridSize = DEFAULT_GRID_SIZE;
      this._difficulty = DEFAULT_DIFFICULTY;
    }
  }

  changeDifficulty() {
    this._difficulty = Number(this.$('#difficulty-slider').val());
    this.$('#difficulty').text(this._difficulty);
    localStorage.setItem('generate.difficulty', String(this._difficulty));
  }

  changeGenerateSize() {
    this._generateGridSize = Number(this.$('#grid-size-slider').val());
    this.$('#grid-size').text(this._generateGridSize);
    localStorage.setItem('generate.gridSize', String(this._generateGridSize));
  }

  private async _startGame() {
    let hasGame = false;
    if (this._gameCode && this._gameCode.length > this._minCodeSize) {
      this._undoStack.clear();
      this.$('.container').removeClass('finished');
      this.showDialog(false);

      const parsedGame = this._game.parseGame(this._gameCode);
      if (parsedGame) {
        this._game = parsedGame;
        hasGame = true;

        this._changeGridSize(this._game.size);

        this._restoreGameState();

        this._restartTimer();
        this._renderCounters();
      }
    }

    if (!hasGame) {
      await this.generateNewGame();
    }
  }

  private _restoreGameState() {
    const stateUrlParameter = this._getURLParameter('state');
    this._removeURLParameter('state');
    let stateLoaded = false;
    if (stateUrlParameter) {
      try {
        this._game.restoreStateBase64(stateUrlParameter);
        stateLoaded = true;
      } catch (ex) {
        console.error(ex);
      }
    }

    if (!stateLoaded) {
      this._gameHistory.restoreGameState(this._gameCode, this._game);
    }
  }

  generateNewGameAgain() {
    this.showDialog(dialogs.WELCOME);
    this.$('#cancel-new-game').show();
  }

  showDialog(dialog: number | boolean) {
    this.$('#welcome-dialog').hide();
    this.$('#start-dialog').hide();
    this.$('#loading-dialog').hide();
    this.$('#solution-dialog').hide();
    this.$('#restart-dialog').hide();
    this.$('#about-dialog').hide();
    this.$('#hint-dialog').hide();
    if (dialog != dialogs.HINT) {
      this.closeHint();
    }

    if (dialog) {
      this.$('.dialog-outer-container').show();
      switch (dialog) {
        case dialogs.LOADING:
          this.$('#loading-dialog').show();
          break;
        case dialogs.WELCOME:
          this.$('#welcome-dialog').show();
          break;
        case dialogs.GENERATED:
          this.win.history.pushState({}, '', this._gameUrl);
          this._startGame();
          break;
        case dialogs.SOLUTION:
          if (!this._game.isSolved) {
            this.$('#solution-dialog').show();
          } else {
            this.$('.dialog-outer-container').hide();
          }
          break;
        case dialogs.RESTART:
          if (!this._game.isSolved) {
            this.$('#restart-dialog').show();
          } else {
            this.$('.dialog-outer-container').hide();
          }
          break;
        case dialogs.ABOUT:
          this.$('#current-game').attr('href', this.win.location.href);
          this.$('#about-dialog').show();
          break;
        case dialogs.HINT:
          this.$('#hint-dialog').show();
          break;
      }
    } else {
      this.$('.dialog-outer-container').hide();
    }
  }

  private _onKeyDown(e: KeyboardEvent) {
    if (this._game.isSolved) return;

    let handled = false;
    const key = e.key;
    if (this._handleCursorKey(e)) {
      handled = true;
    } else if (key >= '0' && key <= '9') {
      this._handleNumberKey(Number(key));
      handled = true;
    } else if (key == 'n') {
      this.toggleNoteMode();
      handled = true;
    } else if (key == 'z' && e.ctrlKey) {
      this.undo();
      handled = true;
    } else if (key === 'Backspace' || key === 'Delete') {
      this._handleDelete();
      handled = true;
    }

    if (handled) {
      e.preventDefault();
    }
  }

  private _handleCursorKey(e: { which: number }) {
    switch (e.which) {
      case 37: // left
        this._game.moveSelection(-1, 0);
        break;
      case 38: // up
        this._game.moveSelection(0, -1);
        break;
      case 39: // right
        this._game.moveSelection(1, 0);
        break;
      case 40: // down
        this._game.moveSelection(0, 1);
        break;
      default:
        return false;
    }

    return true;
  }

  private _firstDigit: number | null = null;
  private _digitTimer: ReturnType<typeof setTimeout> | undefined = undefined;
  private _twoDigitTimeout = 500;
  private _handleNumberKey(num: number) {
    if (this._firstDigit == null) {
      if (this._currentGridSize < 10 || num !== 1) {
        this._handleNumberInput(num);
      } else {
        this._firstDigit = num;
        this._digitTimer = setTimeout(() => {
          this._handleNumberInput(num);
          this._firstDigit = null;
        }, this._twoDigitTimeout);
      }
    } else {
      clearTimeout(this._digitTimer);
      const firstNum = this._firstDigit;
      const combinedNum = this._firstDigit * 10 + num;
      this._firstDigit = null;
      if (combinedNum <= this._currentGridSize) {
        this._handleNumberInput(combinedNum);
      } else {
        this._handleNumberInput(firstNum);
        this._handleNumberKey(num);
      }
    }
  }

  private _handleNumberInput(num: number) {
    if (num < 1 || num > this._currentGridSize) {
      return;
    }

    const activeField = this._game.getActiveField();
    if (!activeField || !activeField.isEditable()) {
      return;
    }

    this._undoStack.push(activeField.copy());

    if (this._noteMode) {
      activeField.setNote(num);
    } else {
      activeField.setUser(num);
      this._game.checkSolved();

      if (this._game.isSolved) {
        this._undoStack.clear();
        this.$('.container').addClass('finished');
        this._onResize();
        clearInterval(this._timer);
      }
    }

    this._saveState();
  }

  private _saveState() {
    this._gameHistory.saveGameState(this._gameCode, this._game);
  }

  private _handleDelete() {
    const field = this._game.getActiveField();
    if (!field || !field.isEditable()) {
      return;
    }

    this._undoStack.push(field.copy());
    field.clear();
  }

  async copyCurrentLink() {
    try {
      let link = this.win.location.href;
      if (this._game) {
        const stateBase64 = await this._game.dumpStateBase64();
        link += `&state=${stateBase64}`;
      }

      await this.win.navigator.clipboard.writeText(link);
      const copyBtn = this.$('.copy-link');
      copyBtn.text('Link copied!');
      setTimeout(() => copyBtn.text('🔗'), 1000);
    } catch (err) {
      console.error('Failed to copy:', err);
    }
  }

  private _handleGameLoad(popstate = false) {
    const code = this._getURLParameter('code');
    const currentKey = this.win.location.href;

    if (code && code.length > this._minCodeSize) {
      this._gameUrl = currentKey;
      this._gameCode = code;
      if (popstate) {
        this._startGame();
      } else {
        this.showDialog(dialogs.GENERATED);
      }
    } else {
      const latestKey = this._gameHistory.getLatestGameKey();
      if (latestKey) {
        // Reload the current page with the latest game code
        this.win.location.href =
          this.win.location.href.split('?')[0] + '?code=' + latestKey;
        return;
      }

      // Nothing stored, show welcome dialog.
      this.showDialog(dialogs.WELCOME);
    }
  }

  private _onResize() {
    this.closeHint();
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
        Math.floor(this.win.innerWidth / this._currentGridSize - 2),
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
  async start(): Promise<void> {
    // ensure modules are loaded
    await this.modulePromise;

    // initial UI setup
    this._createGrid();
    this._onResize();
    this._loadSettings();
    this._handleGameLoad();

    // event handlers for UI elements
    this.$('td[id^="ce"]').on('click', (evt) => {
      // Game fields
      const el = evt.currentTarget as Element;
      const row = Number(this.$(el).attr('row'));
      const col = Number(this.$(el).attr('col'));
      this.selectCell(row, col);
    });
    this.$('td[data-button^="bn"]').on('click', (evt) => {
      // Number buttons
      const el = evt.currentTarget as Element;
      const num = Number(this.$(el).text());
      this._handleNumberInput(num);
    });

    // wire page-level events here so they can call private methods
    this.win.addEventListener('popstate', () => {
      this._handleGameLoad(true);
    });
    this.$(document).on('keydown', (e) => {
      this._onKeyDown(e as any);
    });
    this.$(this.win).on('resize', () => {
      this._onResize();
    });

    // Controls wired from index.html
    this.$('#undo').on('click', () => this.undo());
    this.$('#notes').on('click', () => this.toggleNoteMode());
    this.$('#check').on('click', () => this.check());
    this.$('#hint').on('click', () => this.hint());
    this.$('#show-solution').on('click', () =>
      this.showDialog(dialogs.SOLUTION)
    );

    this.$('#new').on('click', () => this.generateNewGameAgain());
    this.$('#restart').on('click', () => this.showDialog(dialogs.RESTART));
    this.$('#about').on('click', () => this.showDialog(dialogs.ABOUT));

    this.$('#grid-size-slider').on('input', () => this.changeGenerateSize());
    this.$('#difficulty-slider').on('input', () => this.changeDifficulty());

    this.$('#generate-button').on('click', () => this.generateNewGame());
    this.$('#cancel-new-game').on('click', () => this.showDialog(false));

    this.$('#confirm-show-solution').on('click', () => this.showSolution());
    this.$('#confirm-restart').on('click', () => this.restart());

    // Copy link and force-update actions
    this.$('#copy-link').on('click', () => this.copyCurrentLink());

    // The force-update action proper is registered in a script block in index.html
    this.$('#force-update').on('click', () => this.showDialog(false));

    // Hint dialog close handlers (close the hint on click)
    this.$('#hint-dialog').on('click', () => this.closeHint());
    this.$('#hint-close').on('click', (e) => {
      e.stopPropagation();
      this.closeHint();
    });

    // generic close buttons for dialogs (hide overlay)
    this.$('.close-button')
      .not('#hint-close')
      .on('click', () => this.showDialog(false));
  }
}

const _ui = new UIController($, window);

$(() => _ui.start());
