// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

export interface WindowLayoutData {
  width: number | undefined;
  height: number | undefined;
  scrollX: number;
  scrollY: number;
}

export interface PopupElement {
  outerHeight: () => number | null;
  outerWidth: () => number | null;
  css: (styles: Record<string, number | string>) => void;
}

export function positionPopup(
  target: Element,
  popup: PopupElement,
  windowLayout: WindowLayoutData
) {
  const targetPos = target.getBoundingClientRect();
  const windowHeight = windowLayout.height;
  const windowWidth = windowLayout.width;

  if (!windowHeight || !windowWidth) {
    return;
  }

  // Determine the vertical position
  let popupTop: number;
  if (targetPos.top + targetPos.height / 2 > windowHeight / 2) {
    popupTop =
      targetPos.top + windowLayout.scrollY - (popup.outerHeight() ?? 0);
  } else {
    popupTop = targetPos.top + windowLayout.scrollY + targetPos.height;
  }

  // Determine the horizontal position
  let popupLeft: number;
  if (targetPos.left + targetPos.width / 2 > windowWidth / 2) {
    popupLeft =
      targetPos.left + windowLayout.scrollX - (popup.outerWidth() ?? 0);
  } else {
    popupLeft = targetPos.left + windowLayout.scrollX + targetPos.width;
  }

  // Set the position of the dialog
  popup.css({
    position: 'absolute',
    top: popupTop,
    left: popupLeft,
  });
}
