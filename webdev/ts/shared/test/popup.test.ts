import { describe, it, expect } from 'vitest';
import { getPopupPosition, PopupElement } from '../popup';

describe('getPopupPosition', () => {
  const mockPopup: PopupElement = {
    outerHeight: () => 120,
    outerWidth: () => 250,
  };

  const bodyClientRect = {
    top: -13,
    left: 104,
    width: 1280,
    height: 800,
  } as DOMRect;

  it('positions the popup below and to the right of the target by default', () => {
    const targetClientRect = {
      top: 137,
      bottom: 187,
      left: 304,
      right: 404,
      width: 100,
      height: 50,
    } as DOMRect;

    const result = getPopupPosition(
      mockPopup,
      targetClientRect,
      bodyClientRect
    );

    expect(result.top).toBe(187 + 13);
    expect(result.left).toBe(404 - 104);
    expect(result.position).toBe('absolute');
  });

  it('positions the popup above the target if it is below the center of the body', () => {
    const targetClientRect = {
      top: 663,
      bottom: 687,
      left: 304,
      right: 404,
      width: 100,
      height: 50,
    } as DOMRect;

    const result = getPopupPosition(
      mockPopup,
      targetClientRect,
      bodyClientRect
    );

    expect(result.top).toBe(663 + 13 - 120);
    expect(result.left).toBe(404 - 104);
  });

  it('positions the popup to the left of the target if it is to the right of the center of the body', () => {
    const targetClientRect = {
      top: 137,
      bottom: 187,
      left: 1104,
      right: 1204,
      width: 100,
      height: 50,
    } as DOMRect;

    const result = getPopupPosition(
      mockPopup,
      targetClientRect,
      bodyClientRect
    );

    expect(result.top).toBe(187 + 13);
    expect(result.left).toBe(1104 - 104 - 250);
  });

  it('positions the popup above and to the left if the target is in the bottom-right corner', () => {
    const targetClientRect = {
      top: 687,
      bottom: 763,
      left: 1204,
      right: 1304,
      width: 100,
      height: 50,
    } as DOMRect;

    const result = getPopupPosition(
      mockPopup,
      targetClientRect,
      bodyClientRect
    );

    expect(result.top).toBe(687 + 13 - 120);
    expect(result.left).toBe(1204 - 104 - 250);
  });

  it('handles null popup dimensions gracefully', () => {
    const mockPopupWithNull: PopupElement = {
      outerHeight: () => null,
      outerWidth: () => null,
    };

    const targetClientRect = {
      top: 137,
      bottom: 187,
      left: 304,
      right: 404,
      width: 100,
      height: 50,
    } as DOMRect;

    const result = getPopupPosition(
      mockPopupWithNull,
      targetClientRect,
      bodyClientRect
    );

    expect(result.top).toBe(200);
    expect(result.left).toBe(300);
  });
});
