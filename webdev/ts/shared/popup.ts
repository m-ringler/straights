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
  const targetIsBelowCenter = targetPos.top + targetPos.bottom > windowHeight;
  const popupTop = targetIsBelowCenter
    ? targetPos.top - (popup.outerHeight() ?? 0)
    : targetPos.bottom;

  // Determine the horizontal position
  const targetIsRightOfCenter = targetPos.left + targetPos.right > windowWidth;
  const popupLeft = targetIsRightOfCenter
    ? targetPos.left - (popup.outerWidth() ?? 0)
    : targetPos.right;

  // Set the position of the dialog
  popup.css({
    position: 'absolute',
    top: popupTop + windowLayout.scrollY,
    left: popupLeft + windowLayout.scrollX,
  });
}
