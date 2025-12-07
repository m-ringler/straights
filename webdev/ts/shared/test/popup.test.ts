import { describe, it, expect, vi, beforeEach } from 'vitest';
import { positionPopup, WindowLayoutData } from '../popup';

describe('positionPopup', () => {
  let mockTarget: Element;
  let mockPopup: any;
  let mockWindowLayout: WindowLayoutData;

  // Helper method to create a mock DOMRect
  const mockRect = (
    left: number,
    top: number,
    right: number,
    bottom: number
  ) => ({
    top,
    left,
    width: right - left,
    height: bottom - top,
    bottom,
    right,
    x: left,
    y: top,
    toJSON: () => {},
  });

  beforeEach(() => {
    mockTarget = {
      getBoundingClientRect: vi.fn(),
    } as unknown as Element;

    mockPopup = {
      outerHeight: vi.fn(),
      outerWidth: vi.fn(),
      css: vi.fn(),
    };

    mockWindowLayout = {
      width: 1200,
      height: 800,
      scrollX: 20,
      scrollY: 30,
    };
  });

  it('should not set position if window dimensions are undefined', () => {
    mockWindowLayout.width = undefined;
    mockWindowLayout.height = undefined;
    positionPopup(mockTarget, mockPopup, mockWindowLayout);
    expect(mockPopup.css).not.toHaveBeenCalled();
  });

  it('should position popup above target if target is in the bottom half of the window', () => {
    vi.spyOn(mockTarget, 'getBoundingClientRect').mockReturnValue(
      mockRect(300, 650, 450, 730)
    );
    vi.spyOn(mockPopup, 'outerHeight').mockReturnValue(250);
    vi.spyOn(mockPopup, 'outerWidth').mockReturnValue(200);

    positionPopup(mockTarget, mockPopup, mockWindowLayout);

    expect(mockPopup.css).toHaveBeenCalledWith({
      position: 'absolute',
      top: 650 + 30 - 250, // popupTop = targetPos.top + scrollY - popup.outerHeight()
      left: 300 + 20 + 150, // popupLeft = targetPos.right + scrollX
    });
  });

  it('should position popup below target if target is in the top half of the window', () => {
    vi.spyOn(mockTarget, 'getBoundingClientRect').mockReturnValue(
      mockRect(200, 150, 320, 210)
    );
    vi.spyOn(mockPopup, 'outerHeight').mockReturnValue(180);
    vi.spyOn(mockPopup, 'outerWidth').mockReturnValue(160);

    positionPopup(mockTarget, mockPopup, mockWindowLayout);

    expect(mockPopup.css).toHaveBeenCalledWith({
      position: 'absolute',
      top: 150 + 30 + 60, // popupTop = targetPos.top + scrollY + targetPos.height
      left: 320 + 20, // popupLeft = targetPos.right + scrollX
    });
  });

  it('should position popup to the left of target if target is in the right half of the window', () => {
    vi.spyOn(mockTarget, 'getBoundingClientRect').mockReturnValue(
      mockRect(900, 250, 1080, 320)
    );
    vi.spyOn(mockPopup, 'outerHeight').mockReturnValue(220);
    vi.spyOn(mockPopup, 'outerWidth').mockReturnValue(190);

    positionPopup(mockTarget, mockPopup, mockWindowLayout);

    expect(mockPopup.css).toHaveBeenCalledWith({
      position: 'absolute',
      top: 250 + 30 + 70, // popupTop = targetPos.top + scrollY + targetPos.height
      left: 900 + 20 - 190, // popupLeft = targetPos.left + scrollX - popup.outerWidth()
    });
  });

  it('should position popup to the right of target if target is in the left half of the window', () => {
    vi.spyOn(mockTarget, 'getBoundingClientRect').mockReturnValue(
      mockRect(100, 350, 240, 440)
    );
    vi.spyOn(mockPopup, 'outerHeight').mockReturnValue(210);
    vi.spyOn(mockPopup, 'outerWidth').mockReturnValue(170);

    positionPopup(mockTarget, mockPopup, mockWindowLayout);

    expect(mockPopup.css).toHaveBeenCalledWith({
      position: 'absolute',
      top: 350 + 30 + 90, // popupTop = targetPos.top + scrollY + targetPos.height
      left: 100 + 20 + 140, // popupLeft = targetPos.left + scrollX + targetPos.width
    });
  });

  it('should handle scroll offsets and unique values correctly', () => {
    mockWindowLayout.scrollX = 40;
    mockWindowLayout.scrollY = 50;

    vi.spyOn(mockTarget, 'getBoundingClientRect').mockReturnValue(
      mockRect(400, 450, 560, 550)
    );
    vi.spyOn(mockPopup, 'outerHeight').mockReturnValue(300);
    vi.spyOn(mockPopup, 'outerWidth').mockReturnValue(250);

    positionPopup(mockTarget, mockPopup, mockWindowLayout);

    expect(mockPopup.css).toHaveBeenCalledWith({
      position: 'absolute',
      top: 450 + 50 - 300, // popupTop = targetPos.top + scrollY - popup.outerHeight()
      left: 560 + 40, // popupLeft = targetPos.right + scrollX
    });
  });
});
