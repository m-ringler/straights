// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

export interface PopupElement {
  outerHeight: () => number | null | undefined;
  outerWidth: () => number | null | undefined;
}

export function getPopupPosition(
  popup: PopupElement,
  targetClientRect: DOMRect,
  bodyClientRect: DOMRect
): {
  left: number;
  top: number;
  position: 'absolute';
} {
  const targetRectAbsolute = {
    top: targetClientRect.top - bodyClientRect.top,
    bottom: targetClientRect.bottom - bodyClientRect.top,
    left: targetClientRect.left - bodyClientRect.left,
    right: targetClientRect.right - bodyClientRect.left,
  };

  // Determine the vertical position
  const targetIsBelowCenter =
    targetRectAbsolute.top + targetRectAbsolute.bottom > bodyClientRect.height;
  const popupTop = targetIsBelowCenter
    ? targetRectAbsolute.top - (popup.outerHeight() ?? 0)
    : targetRectAbsolute.bottom;

  // Determine the horizontal position
  const targetIsRightOfCenter =
    targetRectAbsolute.left + targetRectAbsolute.right > bodyClientRect.width;
  const popupLeft = targetIsRightOfCenter
    ? targetRectAbsolute.left - (popup.outerWidth() ?? 0)
    : targetRectAbsolute.right;

  // Return the position of the popup in absolute coordinates
  return {
    top: popupTop,
    left: popupLeft,
    position: 'absolute',
  };
}
